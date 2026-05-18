export interface TaskConsistency {
  taskId: string;
  taskTitle: string;
  categoryName: string;
  categoryColor: string;
  totalScheduledDays: number;
  completedDays: number;
  skippedDays: number;
  missedDays: number;
  consistencyPercent: number;
}

export interface DailyScoreTrend {
  date: string;
  score: number;
  completedTasks: number;
  totalTasks: number;
  xpEarned: number;
}

export interface TaskStreakSummary {
  taskId: string;
  taskTitle: string;
  categoryName: string;
  currentStreak: number;
  longestStreak: number;
  lastCompletedDate: string | null;
}

export interface CategoryPerformance {
  categoryId: string;
  categoryName: string;
  color: string;
  totalTasks: number;
  averageConsistency: number;
  totalXPEarned: number;
  bestStreak: number;
}

export interface WeakestHabit {
  taskId: string;
  taskTitle: string;
  categoryName: string;
  categoryColor: string;
  totalScheduledDays: number;
  completedDays: number;
  missedDays: number;
  consistencyPercent: number;
}
