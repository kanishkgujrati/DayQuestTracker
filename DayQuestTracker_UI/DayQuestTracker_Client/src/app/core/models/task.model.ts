export enum FrequencyType {
  Daily = 1,
  Weekly = 2,
  Custom = 3
}

export enum CompletionStatus {
  Completed = 1,
  Skipped = 2
}

export interface HabitTask {
  id: string;
  categoryId: string;
  categoryName: string;
  title: string;
  description: string | null;
  difficulty: number;
  frequencyType: FrequencyType;
  targetPerWeek: number | null;
  scheduledDays: number[];
  xpValue: number;
  createdAt: string;
}

export interface DailyTaskView {
  taskId: string;
  title: string;
  description: string | null;
  categoryName: string;
  categoryColor: string;
  difficulty: number;
  xpValue: number;
  frequencyType: FrequencyType;
  completionId: string | null;
  status: CompletionStatus | null;
  notes: string | null;
  currentStreak: number;
}

export interface CreateHabitTaskRequest {
  categoryId: string;
  title: string;
  description?: string;
  difficulty: number;
  frequencyType: FrequencyType;
  targetPerWeek?: number;
  scheduledDays?: number[];
}