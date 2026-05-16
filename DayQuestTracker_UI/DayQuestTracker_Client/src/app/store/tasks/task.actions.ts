import { createAction, props } from '@ngrx/store';
import {
  HabitTask,
  CreateHabitTaskRequest,
  UpdateHabitTaskRequest,
} from '../../core/models/task.models';

export const loadTasks = createAction('[Tasks] Load Tasks', props<{ categoryId?: string }>());

export const loadTasksSuccess = createAction(
  '[Tasks] Load Tasks Success',
  props<{ tasks: HabitTask[] }>(),
);

export const loadTasksFailure = createAction(
  '[Tasks] Load Tasks Failure',
  props<{ error: string }>(),
);

export const createTask = createAction(
  '[Tasks] Create Task',
  props<{ request: CreateHabitTaskRequest }>(),
);

export const createTaskSuccess = createAction(
  '[Tasks] Create Task Success',
  props<{ task: HabitTask }>(),
);

export const createTaskFailure = createAction(
  '[Tasks] Create Task Failure',
  props<{ error: string }>(),
);

export const updateTask = createAction(
  '[Tasks] Update Task',
  props<{ id: string; request: UpdateHabitTaskRequest }>(),
);

export const updateTaskSuccess = createAction(
  '[Tasks] Update Task Success',
  props<{ task: HabitTask }>(),
);

export const updateTaskFailure = createAction(
  '[Tasks] Update Task Failure',
  props<{ error: string }>(),
);

export const deleteTask = createAction('[Tasks] Delete Task', props<{ id: string }>());

export const deleteTaskSuccess = createAction(
  '[Tasks] Delete Task Success',
  props<{ id: string }>(),
);

export const deleteTaskFailure = createAction(
  '[Tasks] Delete Task Failure',
  props<{ error: string }>(),
);

export const clearTaskError = createAction('[Tasks] Clear Error');
