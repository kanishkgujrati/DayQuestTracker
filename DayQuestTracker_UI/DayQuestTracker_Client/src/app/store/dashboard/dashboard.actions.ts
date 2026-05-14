import { createAction, props } from '@ngrx/store';
import { DailyTaskView } from '../../core/models/task.models';

export const loadDailyTasks = createAction(
  '[Dashboard] Load Daily Tasks',
  props<{ date: string }>(),
);

export const loadDailyTasksSuccess = createAction(
  '[Dashboard] Load Daily Tasks Success',
  props<{ tasks: DailyTaskView[] }>(),
);

export const loadDailyTasksFailure = createAction(
  '[Dashboard] Load Daily Tasks Failure',
  props<{ error: string }>(),
);

export const logCompletion = createAction(
  '[Dashboard] Log Completion',
  props<{ taskId: string; date: string; status: number }>(),
);

export const logCompletionSuccess = createAction(
  '[Dashboard] Log Completion Success',
  props<{ date: string }>(),
);

export const logCompletionFailure = createAction(
  '[Dashboard] Log Completion Failure',
  props<{ error: string }>(),
);

export const undoCompletion = createAction(
  '[Dashboard] Undo Completion',
  props<{ completionId: string; date: string }>(),
);

export const undoCompletionSuccess = createAction(
  '[Dashboard] Undo Completion Success',
  props<{ date: string }>(),
);

export const undoCompletionFailure = createAction(
  '[Dashboard] Undo Completion Failure',
  props<{ error: string }>(),
);

export const setSelectedDate = createAction(
  '[Dashboard] Set Selected Date',
  props<{ date: string }>(),
);
