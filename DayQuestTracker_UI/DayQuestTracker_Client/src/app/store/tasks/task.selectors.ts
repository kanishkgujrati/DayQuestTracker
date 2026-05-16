import { createFeatureSelector, createSelector } from '@ngrx/store';
import { TaskState } from './task.reducer';
import { FrequencyType } from '../../core/models/task.models';

export const selectTaskState = createFeatureSelector<TaskState>('task');

export const selectAllTasks = createSelector(selectTaskState, (state) => state.tasks);

export const selectTasksLoading = createSelector(selectTaskState, (state) => state.isLoading);

export const selectTasksSaving = createSelector(selectTaskState, (state) => state.isSaving);

export const selectTaskError = createSelector(selectTaskState, (state) => state.error);

export const selectTasksByCategory = (categoryId: string) =>
  createSelector(selectAllTasks, (tasks) => tasks.filter((t) => t.categoryId === categoryId));

export const selectDailyTasks = createSelector(selectAllTasks, (tasks) =>
  tasks.filter((t) => t.frequencyType === FrequencyType.Daily),
);
