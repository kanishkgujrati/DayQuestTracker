import { Component, OnInit, AfterViewInit, ElementRef, ViewChild, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Store } from '@ngrx/store';
import { Observable, Subject, takeUntil } from 'rxjs';
import { Chart, registerables } from 'chart.js';
import {
  TaskConsistency,
  DailyScoreTrend,
  TaskStreakSummary,
  CategoryPerformance,
  WeakestHabit,
} from '../../core/models/analytics.models';
import { loadAnalytics, setAnalyticsDateRange } from '../../store/analytics/analytics.actions';
import {
  selectConsistency,
  selectDailyTrend,
  selectStreaks,
  selectWeakestHabits,
  selectCategoryPerformance,
  selectAnalyticsLoading,
  selectAverageConsistency,
  selectTotalXPInPeriod,
  selectPerfectDays,
  selectTopStreaks,
  selectTotalAssignedXPInPeriod,
} from '../../store/analytics/analytics.selectors';
import { Router } from '@angular/router';

Chart.register(...registerables);

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './analytics.component.html',
})
export class AnalyticsComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('trendChart') trendChartRef!: ElementRef;
  @ViewChild('consistencyChart') consistencyChartRef!: ElementRef;

  consistency$!: Observable<TaskConsistency[]>;
  dailyTrend$!: Observable<DailyScoreTrend[]>;
  streaks$!: Observable<TaskStreakSummary[]>;
  weakestHabits$!: Observable<WeakestHabit[]>;
  categoryPerformance$!: Observable<CategoryPerformance[]>;
  isLoading$!: Observable<boolean>;
  avgConsistency$!: Observable<number>;
  totalXP$!: Observable<number>;
  perfectDays$!: Observable<number>;
  topStreaks$!: Observable<TaskStreakSummary[]>;

  totalAssignedXP$!: Observable<number>;

  selectedRange = '30';
  private trendChart?: Chart;
  private consistencyChart?: Chart;
  private destroy$ = new Subject<void>();

  constructor(
    private store: Store,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.consistency$ = this.store.select(selectConsistency);
    this.dailyTrend$ = this.store.select(selectDailyTrend);
    this.streaks$ = this.store.select(selectStreaks);
    this.weakestHabits$ = this.store.select(selectWeakestHabits);
    this.categoryPerformance$ = this.store.select(selectCategoryPerformance);
    this.isLoading$ = this.store.select(selectAnalyticsLoading);
    this.avgConsistency$ = this.store.select(selectAverageConsistency);
    this.totalXP$ = this.store.select(selectTotalXPInPeriod);
    this.perfectDays$ = this.store.select(selectPerfectDays);
    this.topStreaks$ = this.store.select(selectTopStreaks);
    this.totalAssignedXP$ = this.store.select(selectTotalAssignedXPInPeriod);

    this.loadData();
  }

  ngAfterViewInit(): void {
    // Build charts after data loads
    this.store
      .select(selectDailyTrend)
      .pipe(takeUntil(this.destroy$))
      .subscribe((data) => {
        if (data.length) this.buildTrendChart(data);
      });

    this.store
      .select(selectConsistency)
      .pipe(takeUntil(this.destroy$))
      .subscribe((data) => {
        if (data.length) this.buildConsistencyChart(data);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.trendChart?.destroy();
    this.consistencyChart?.destroy();
  }

  loadData(): void {
    const end = new Date();
    const start = new Date();
    start.setDate(end.getDate() - parseInt(this.selectedRange) + 1);

    const startDate = start.toISOString().split('T')[0];
    const endDate = end.toISOString().split('T')[0];

    this.store.dispatch(setAnalyticsDateRange({ startDate, endDate }));
    this.store.dispatch(loadAnalytics({ startDate, endDate }));
  }

  onRangeChange(days: string): void {
    this.selectedRange = days;
    this.trendChart?.destroy();
    this.consistencyChart?.destroy();
    this.loadData();
  }

  buildTrendChart(data: DailyScoreTrend[]): void {
    if (this.trendChart) this.trendChart.destroy();

    const ctx = this.trendChartRef?.nativeElement?.getContext('2d');
    if (!ctx) return;

    this.trendChart = new Chart(ctx, {
      type: 'line',
      data: {
        labels: data.map((d) => {
          const date = new Date(d.date);
          return date.toLocaleDateString('en', {
            month: 'short',
            day: 'numeric',
          });
        }),
        datasets: [
          {
            label: 'Daily Score',
            data: data.map((d) => d.score),
            borderColor: '#0ea5e9',
            backgroundColor: 'rgba(14, 165, 233, 0.1)',
            borderWidth: 2,
            fill: true,
            tension: 0.4,
            pointBackgroundColor: '#0ea5e9',
            pointRadius: 4,
            pointHoverRadius: 7, // larger on hover — signals clickability
            pointHoverBackgroundColor: '#ffffff',
            pointHoverBorderColor: '#0ea5e9',
            pointHoverBorderWidth: 2,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        //cursor: 'pointer',
        plugins: {
          legend: { display: false },
          tooltip: {
            callbacks: {
              title: (items) => {
                const idx = items[0].dataIndex;
                return new Date(data[idx].date + 'T00:00:00').toLocaleDateString('en', {
                  weekday: 'long',
                  month: 'long',
                  day: 'numeric',
                });
              },
              label: (item) => {
                const idx = item.dataIndex;
                return [
                  ` Daily Score: ${data[idx].score}%`,
                  ` XP Earned: ${data[idx].xpEarned} / ${data[idx].totalAssignedXP}`,
                  ` Completed: ${data[idx].completedTasks}/${data[idx].totalTasks}`,
                  ` Click to view details`,
                ];
              },
            },
          },
        },
        scales: {
          x: {
            grid: { color: 'rgba(255,255,255,0.05)' },
            ticks: { color: '#94a3b8', maxTicksLimit: 10 },
          },
          y: {
            min: 0,
            max: 100,
            grid: { color: 'rgba(255,255,255,0.05)' },
            ticks: {
              color: '#94a3b8',
              callback: (value) => `${value}%`,
            },
          },
        },
        onClick: (event, elements) => {
          if (!elements.length) return;
          const idx = elements[0].index;
          const clickedDate = data[idx].date;
          this.router.navigate(['/history'], {
            queryParams: { date: clickedDate },
          });
        },
      },
    });
  }

  buildConsistencyChart(data: TaskConsistency[]): void {
    if (this.consistencyChart) this.consistencyChart.destroy();

    const ctx = this.consistencyChartRef?.nativeElement?.getContext('2d');
    if (!ctx) return;

    const top10 = [...data]
      .sort((a, b) => b.consistencyPercent - a.consistencyPercent)
      .slice(0, 10);

    this.consistencyChart = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: top10.map((d) =>
          d.taskTitle.length > 15 ? d.taskTitle.substring(0, 15) + '...' : d.taskTitle,
        ),
        datasets: [
          {
            label: 'Consistency %',
            data: top10.map((d) => d.consistencyPercent),
            backgroundColor: top10.map((d) =>
              d.consistencyPercent >= 80
                ? 'rgba(34, 197, 94, 0.7)'
                : d.consistencyPercent >= 50
                  ? 'rgba(234, 179, 8, 0.7)'
                  : 'rgba(239, 68, 68, 0.7)',
            ),
            borderRadius: 6,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
        },
        scales: {
          x: {
            grid: { color: 'rgba(255,255,255,0.05)' },
            ticks: { color: '#94a3b8' },
          },
          y: {
            min: 0,
            max: 100,
            grid: { color: 'rgba(255,255,255,0.05)' },
            ticks: {
              color: '#94a3b8',
              callback: (value) => `${value}%`,
            },
          },
        },
      },
    });
  }

  getConsistencyColor(percent: number): string {
    if (percent >= 80) return 'text-green-400';
    if (percent >= 50) return 'text-yellow-400';
    return 'text-red-400';
  }

  getConsistencyBg(percent: number): string {
    if (percent >= 80) return 'bg-green-500';
    if (percent >= 50) return 'bg-yellow-500';
    return 'bg-red-500';
  }
}
