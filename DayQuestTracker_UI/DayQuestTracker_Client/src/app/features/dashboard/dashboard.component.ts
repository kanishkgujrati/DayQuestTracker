import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { DailyTaskView, CompletionStatus } from '../../core/models/task.model';
import {
  loadDailyTasks,
  logCompletion,
  undoCompletion,
  setSelectedDate,
} from '../../store/dashboard/dashboard.actions';
import {
  selectDailyTasks,
  selectSelectedDate,
  selectDashboardLoading,
  selectDailyProgress,
} from '../../store/dashboard/dashboard.selectors';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
})
export class DashboardComponent implements OnInit {
  dailyTasks$!: Observable<DailyTaskView[]>;
  selectedDate$!: Observable<string>;
  isLoading$!: Observable<boolean>;
  progress$!: Observable<any>;

  CompletionStatus = CompletionStatus;

  // Week days for date navigation
  weekDays: { date: string; label: string; dayName: string }[] = [];

  constructor(private store: Store) {}

  ngOnInit(): void {
    this.dailyTasks$ = this.store.select(selectDailyTasks);
    this.selectedDate$ = this.store.select(selectSelectedDate);
    this.isLoading$ = this.store.select(selectDashboardLoading);
    this.progress$ = this.store.select(selectDailyProgress);

    this.generateWeekDays();

    // Load today's tasks on init
    const today = new Date().toISOString().split('T')[0];
    this.store.dispatch(loadDailyTasks({ date: today }));
  }

  generateWeekDays(): void {
    const today = new Date();
    this.weekDays = [];

    // Generate last 6 days + today
    for (let i = 6; i >= 0; i--) {
      const date = new Date(today);
      date.setDate(today.getDate() - i);
      const dateStr = date.toISOString().split('T')[0];

      this.weekDays.push({
        date: dateStr,
        label:
          i === 0
            ? 'Today'
            : i === 1
              ? 'Yesterday'
              : date.toLocaleDateString('en', { weekday: 'short' }),
        dayName: date.toLocaleDateString('en', { day: 'numeric' }),
      });
    }
  }

  onDateSelect(date: string): void {
    this.store.dispatch(setSelectedDate({ date }));
    this.store.dispatch(loadDailyTasks({ date }));
  }

  onComplete(task: DailyTaskView, date: string): void {
    if (task.completionId) {
      // Already logged — undo it
      this.store.dispatch(
        undoCompletion({
          completionId: task.completionId,
          date,
        }),
      );
    } else {
      // Not logged — mark as completed
      this.store.dispatch(
        logCompletion({
          taskId: task.taskId,
          date,
          status: CompletionStatus.Completed,
        }),
      );
    }
  }

  onSkip(task: DailyTaskView, date: string): void {
    this.store.dispatch(
      logCompletion({
        taskId: task.taskId,
        date,
        status: CompletionStatus.Skipped,
      }),
    );
  }

  getDifficultyStars(difficulty: number): string {
    return '⭐'.repeat(difficulty);
  }

  isToday(date: string): boolean {
    return date === new Date().toISOString().split('T')[0];
  }

  isFuture(date: string): boolean {
    return date > new Date().toISOString().split('T')[0];
  }
}
