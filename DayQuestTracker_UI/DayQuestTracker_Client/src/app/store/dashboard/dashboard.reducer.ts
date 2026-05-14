import { createReducer, on } from '@ngrx/store';
import { DailyTaskView } from '../../core/models/task.models';
import * as DashboardActions from './dashboard.actions';

export interface DashboardState {
  dailyTasks: DailyTaskView[];
  selectedDate: string;
  isLoading: boolean;
  error: string | null;
}

const today = new Date().toISOString().split('T')[0];

export const initialState: DashboardState = {
  dailyTasks: [],
  selectedDate: today,
  isLoading: false,
  error: null,
};

export const dashboardReducer = createReducer(
  initialState,

  on(DashboardActions.loadDailyTasks, (state) => ({
    ...state,
    isLoading: true,
    error: null,
  })),

  on(DashboardActions.loadDailyTasksSuccess, (state, { tasks }) => ({
    ...state,
    isLoading: false,
    dailyTasks: tasks,
  })),

  on(DashboardActions.loadDailyTasksFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    error,
  })),

  on(DashboardActions.setSelectedDate, (state, { date }) => ({
    ...state,
    selectedDate: date,
  })),
);
