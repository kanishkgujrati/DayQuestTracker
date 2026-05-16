import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, switchMap } from 'rxjs/operators';
import { of } from 'rxjs';
import { TaskService } from '../../core/services/task.service';
import * as TaskActions from './task.actions';

@Injectable()
export class TaskEffects {
  private actions$ = inject(Actions);
  private taskService = inject(TaskService);

  loadTasks$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TaskActions.loadTasks),
      switchMap(({ categoryId }) =>
        this.taskService.getTasks(categoryId).pipe(
          map((tasks) => TaskActions.loadTasksSuccess({ tasks })),
          catchError((error) =>
            of(
              TaskActions.loadTasksFailure({
                error: error.error?.error || 'Failed to load tasks.',
              }),
            ),
          ),
        ),
      ),
    ),
  );

  createTask$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TaskActions.createTask),
      switchMap(({ request }) =>
        this.taskService.createTask(request).pipe(
          map((task) => TaskActions.createTaskSuccess({ task })),
          catchError((error) =>
            of(
              TaskActions.createTaskFailure({
                error:
                  error.error?.error || error.error?.errors?.Title?.[0] || 'Failed to create task.',
              }),
            ),
          ),
        ),
      ),
    ),
  );

  updateTask$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TaskActions.updateTask),
      switchMap(({ id, request }) =>
        this.taskService.updateTask(id, request).pipe(
          map((task) => TaskActions.updateTaskSuccess({ task })),
          catchError((error) =>
            of(
              TaskActions.updateTaskFailure({
                error: error.error?.error || 'Failed to update task.',
              }),
            ),
          ),
        ),
      ),
    ),
  );

  deleteTask$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TaskActions.deleteTask),
      switchMap(({ id }) =>
        this.taskService.deleteTask(id).pipe(
          map(() => TaskActions.deleteTaskSuccess({ id })),
          catchError((error) =>
            of(
              TaskActions.deleteTaskFailure({
                error: error.error?.error || 'Failed to delete task.',
              }),
            ),
          ),
        ),
      ),
    ),
  );
}
