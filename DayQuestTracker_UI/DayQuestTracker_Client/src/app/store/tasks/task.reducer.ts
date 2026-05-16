import { createReducer, on } from '@ngrx/store';
import { HabitTask } from '../../core/models/task.models';
import * as TaskActions from './task.actions';

export interface TaskState {
  tasks: HabitTask[];
  isLoading: boolean;
  isSaving: boolean;
  error: string | null;
}

export const initialState: TaskState = {
  tasks: [],
  isLoading: false,
  isSaving: false,
  error: null,
};

export const taskReducer = createReducer(
  initialState,

  on(TaskActions.loadTasks, (state) => ({
    ...state,
    isLoading: true,
    error: null,
  })),

  on(TaskActions.loadTasksSuccess, (state, { tasks }) => ({
    ...state,
    isLoading: false,
    tasks,
  })),

  on(TaskActions.loadTasksFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    error,
  })),

  on(TaskActions.createTask, (state) => ({
    ...state,
    isSaving: true,
    error: null,
  })),

  on(TaskActions.createTaskSuccess, (state, { task }) => ({
    ...state,
    isSaving: false,
    tasks: [...state.tasks, task],
  })),

  on(TaskActions.createTaskFailure, (state, { error }) => ({
    ...state,
    isSaving: false,
    error,
  })),

  on(TaskActions.updateTask, (state) => ({
    ...state,
    isSaving: true,
    error: null,
  })),

  on(TaskActions.updateTaskSuccess, (state, { task }) => ({
    ...state,
    isSaving: false,
    tasks: state.tasks.map((t) => (t.id === task.id ? task : t)),
  })),

  on(TaskActions.updateTaskFailure, (state, { error }) => ({
    ...state,
    isSaving: false,
    error,
  })),

  on(TaskActions.deleteTask, (state) => ({
    ...state,
    isSaving: true,
    error: null,
  })),

  on(TaskActions.deleteTaskSuccess, (state, { id }) => ({
    ...state,
    isSaving: false,
    tasks: state.tasks.filter((t) => t.id !== id),
  })),

  on(TaskActions.deleteTaskFailure, (state, { error }) => ({
    ...state,
    isSaving: false,
    error,
  })),

  on(TaskActions.clearTaskError, (state) => ({
    ...state,
    error: null,
  })),
);
