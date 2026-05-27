import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { AnalyticsService } from '../../core/services/analytic.service';
import { DayHistory, DayHistoryTask, DayTaskStatus } from '../../core/models/analytics.models';

interface WeekDay {
  date: string;
  dayName: string;
  dayNumber: number;
  isToday: boolean;
  isFuture: boolean;
}

@Component({
  selector: 'app-history',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './history.component.html',
})
export class HistoryComponent implements OnInit {
  DayTaskStatus = DayTaskStatus;

  selectedDate: string = '';
  selectedDayHistory: DayHistory | null = null;
  isLoadingDay = false;
  error: string | null = null;

  weekDays: WeekDay[] = [];
  currentWeekStart: Date = new Date();

  constructor(
    private route: ActivatedRoute,
    private analyticsService: AnalyticsService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      const date = params['date'];
      if (date) {
        this.selectedDate = date;
        const d = this.parseDateSafe(date);
        this.setWeekFromDate(d);
      } else {
        this.setWeekFromDate(new Date());
        this.selectedDate = this.toDateString(new Date());
      }
      this.loadDayHistory(this.selectedDate);
    });
  }

  setWeekFromDate(date: Date): void {
    // Get Monday of the week containing this date
    const day = date.getDay();
    const daysFromMonday = day === 0 ? 6 : day - 1;
    const monday = new Date(date);
    monday.setDate(date.getDate() - daysFromMonday);
    monday.setHours(0, 0, 0, 0);
    this.currentWeekStart = monday;
    this.generateWeekDays();
  }

  generateWeekDays(): void {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    this.weekDays = [];

    for (let i = 0; i < 7; i++) {
      const date = new Date(this.currentWeekStart);
      date.setDate(this.currentWeekStart.getDate() + i);
      date.setHours(0, 0, 0, 0);

      this.weekDays.push({
        date: this.toDateString(date),
        dayName: date.toLocaleDateString('en', { weekday: 'short' }),
        dayNumber: date.getDate(),
        isToday: date.getTime() === today.getTime(),
        isFuture: date > today,
      });
    }
  }

  goToPreviousWeek(): void {
    const prev = new Date(this.currentWeekStart);
    prev.setDate(prev.getDate() - 7);
    this.currentWeekStart = prev;
    this.generateWeekDays();
  }

  goToNextWeek(): void {
    const next = new Date(this.currentWeekStart);
    next.setDate(next.getDate() + 7);
    this.currentWeekStart = next;
    this.generateWeekDays();

    // Do not allow navigating to future weeks
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    if (this.currentWeekStart > today) {
      this.setWeekFromDate(today);
    }
  }

  isNextWeekDisabled(): boolean {
    const nextWeekStart = new Date(this.currentWeekStart);
    nextWeekStart.setDate(nextWeekStart.getDate() + 7);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return nextWeekStart > today;
  }

  onDaySelect(day: WeekDay): void {
    if (day.isFuture) return;
    this.selectedDate = day.date;
    this.loadDayHistory(day.date);
  }

  loadDayHistory(date: string): void {
    this.isLoadingDay = true;
    this.error = null;
    this.selectedDayHistory = null;

    this.analyticsService.getDayHistory(date).subscribe({
      next: (data: any) => {
        this.selectedDayHistory = data;
      },
      complete: () => {
        this.isLoadingDay = false;
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.error = err.error?.error || 'Failed to load history.';
        this.isLoadingDay = false;
        this.cdr.detectChanges();
      },
    });
  }

  getWeekRangeLabel(): string {
    const end = new Date(this.currentWeekStart);
    end.setDate(end.getDate() + 6);
    const startLabel = this.currentWeekStart.toLocaleDateString('en', {
      month: 'short',
      day: 'numeric',
    });
    const endLabel = end.toLocaleDateString('en', {
      month: 'short',
      day: 'numeric',
    });
    return `${startLabel} — ${endLabel}`;
  }

  getStatusIcon(status: DayTaskStatus): string {
    switch (status) {
      case DayTaskStatus.Completed:
        return '✓';
      case DayTaskStatus.Skipped:
        return '✗';
      case DayTaskStatus.Missed:
        return '—';
    }
  }

  getStatusColor(status: DayTaskStatus): string {
    switch (status) {
      case DayTaskStatus.Completed:
        return 'text-green-400';
      case DayTaskStatus.Skipped:
        return 'text-red-400';
      case DayTaskStatus.Missed:
        return 'text-slate-500';
    }
  }

  getStatusBg(status: DayTaskStatus): string {
    switch (status) {
      case DayTaskStatus.Completed:
        return 'border-green-500/30 bg-green-500/5';
      case DayTaskStatus.Skipped:
        return 'border-red-500/30 bg-red-500/5';
      case DayTaskStatus.Missed:
        return 'border-slate-700 bg-transparent';
    }
  }

  getStatusLabel(status: DayTaskStatus): string {
    switch (status) {
      case DayTaskStatus.Completed:
        return 'Completed';
      case DayTaskStatus.Skipped:
        return 'Skipped';
      case DayTaskStatus.Missed:
        return 'Missed';
    }
  }

  getScoreColor(score: number): string {
    if (score === 100) return 'text-green-400';
    if (score >= 50) return 'text-yellow-400';
    return 'text-red-400';
  }

  getDifficultyStars(difficulty: number): string {
    return '⭐'.repeat(difficulty);
  }

  formatDate(dateStr: string): string {
    const date = this.parseDateSafe(dateStr);
    return new Date(dateStr + 'T00:00:00').toLocaleDateString('en', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  }

  // Parse yyyy-MM-dd without timezone shift
  parseDateSafe(dateStr: string): Date {
    const [year, month, day] = dateStr.split('-').map(Number);
    return new Date(year, month - 1, day); // local time, no UTC conversion
  }

  // Format Date to yyyy-MM-dd in local time
  toDateString(date: Date): string {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }
}
