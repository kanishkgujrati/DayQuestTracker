import { createFeatureSelector, createSelector } from '@ngrx/store';
import { AuthState } from '../../core/models/auth.model';

export const selectAuthState = createFeatureSelector<AuthState>('auth');

export const selectCurrentUser = createSelector(selectAuthState, (state) => state.user);

export const selectAccessToken = createSelector(selectAuthState, (state) => state.accessToken);

export const selectIsLoggedIn = createSelector(
  selectAuthState,
  (state) => !!state.accessToken && !!state.user,
);

export const selectAuthLoading = createSelector(selectAuthState, (state) => state.isLoading);

export const selectAuthError = createSelector(selectAuthState, (state) => state.error);

export const selectUserLevel = createSelector(selectCurrentUser, (user) => user?.level ?? 1);

export const selectUserXP = createSelector(selectCurrentUser, (user) => user?.totalXP ?? 0);
