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
  totalAssignedXP: number;
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
  hasTasks: boolean;
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

export enum DayTaskStatus {
  Completed = 1,
  Skipped = 2,
  Missed = 3,
}

export interface DayHistoryTask {
  taskId: string;
  title: string;
  categoryName: string;
  categoryColor: string;
  difficulty: number;
  xpValue: number;
  frequencyType: number;
  status: DayTaskStatus;
  notes: string | null;
  xpAwarded: number;
}

export interface DayHistory {
  date: string;
  dayName: string;
  score: number;
  totalTasks: number;
  completedCount: number;
  skippedCount: number;
  missedCount: number;
  xpEarned: number;
  totalAssignedXP: number;
  tasks: DayHistoryTask[];
}
