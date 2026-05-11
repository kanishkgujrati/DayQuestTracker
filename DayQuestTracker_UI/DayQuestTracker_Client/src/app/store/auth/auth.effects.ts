import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Router } from '@angular/router';
import { catchError, map, switchMap, tap } from 'rxjs/operators';
import { of } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import * as AuthActions from './auth.actions';

@Injectable()
export class AuthEffects {
  constructor(
    private actions$: Actions,
    private authService: AuthService,
    private router: Router,
  ) {}

  login$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.login),
      switchMap(({ request }) =>
        this.authService.login(request).pipe(
          map((response) => AuthActions.loginSuccess({ response })),
          catchError((error) =>
            of(
              AuthActions.loginFailure({
                error: error.error?.message || error.error?.error || 'Invalid credentials.',
              }),
            ),
          ),
        ),
      ),
    ),
  );

  loginSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.loginSuccess),
        tap(({ response }) => {
          // Persist user to localStorage for page refresh
          localStorage.setItem(
            'auth_user',
            JSON.stringify({
              userId: response.userId,
              username: response.username,
              level: response.level,
              totalXP: response.totalXP,
            }),
          );
          this.router.navigate(['/dashboard']);
        }),
      ),
    { dispatch: false },
  );

  register$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.register),
      switchMap(({ request }) =>
        this.authService.register(request).pipe(
          map((response) => AuthActions.registerSuccess({ response })),
          catchError((error) =>
            of(
              AuthActions.registerFailure({
                error: error.error?.message || error.error?.error || 'Registration failed.',
              }),
            ),
          ),
        ),
      ),
    ),
  );

  registerSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.registerSuccess),
        tap(({ response }) => {
          localStorage.setItem(
            'auth_user',
            JSON.stringify({
              userId: response.userId,
              username: response.username,
              level: response.level,
              totalXP: response.totalXP,
            }),
          );
          this.router.navigate(['/dashboard']);
        }),
      ),
    { dispatch: false },
  );

  logout$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.logout),
        tap(() => {
          this.authService.logout();
          localStorage.removeItem('auth_user');
          this.router.navigate(['/auth/login']);
        }),
      ),
    { dispatch: false },
  );
}
