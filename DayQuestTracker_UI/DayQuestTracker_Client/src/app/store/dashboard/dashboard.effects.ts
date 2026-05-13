import { inject, Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, switchMap, tap } from 'rxjs/operators';
import { of } from 'rxjs';
import { TaskService } from '../../core/services/task.service';
import { CompletionStatus } from '../../core/models/task.model';
import * as DashboardActions from './dashboard.actions';

@Injectable()
export class DashboardEffects {

  private actions$ = inject(Actions);
  private taskService = inject(TaskService);

  loadDailyTasks$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DashboardActions.loadDailyTasks),
      switchMap(({ date }) =>
        this.taskService.getDailyTasks(date).pipe(
          map((tasks) => DashboardActions.loadDailyTasksSuccess({ tasks })),
          catchError((error) =>
            of(
              DashboardActions.loadDailyTasksFailure({
                error: error.error?.error || 'Failed to load tasks.',
              }),
            ),
          ),
        ),
      ),
    ),
  );

  logCompletion$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DashboardActions.logCompletion),
      switchMap(({ taskId, date, status }) =>
        this.taskService
          .logCompletion({
            taskId,
            completionDate: date,
            status,
          })
          .pipe(
            map(() => DashboardActions.logCompletionSuccess({ date })),
            catchError((error) =>
              of(
                DashboardActions.logCompletionFailure({
                  error: error.error?.error || 'Failed to log completion.',
                }),
              ),
            ),
          ),
      ),
    ),
  );

  // Reload tasks after successful completion or undo
  reloadAfterCompletion$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DashboardActions.logCompletionSuccess, DashboardActions.undoCompletionSuccess),
      map(({ date }) => DashboardActions.loadDailyTasks({ date })),
    ),
  );

  undoCompletion$ = createEffect(() =>
    this.actions$.pipe(
      ofType(DashboardActions.undoCompletion),
      switchMap(({ completionId, date }) =>
        this.taskService.undoCompletion(completionId).pipe(
          map(() => DashboardActions.undoCompletionSuccess({ date })),
          catchError((error) =>
            of(
              DashboardActions.undoCompletionFailure({
                error: error.error?.error || 'Failed to undo completion.',
              }),
            ),
          ),
        ),
      ),
    ),
  );
}
