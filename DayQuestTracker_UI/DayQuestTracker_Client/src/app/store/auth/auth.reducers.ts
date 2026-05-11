import { createReducer, on } from '@ngrx/store';
import * as AuthActions from './auth.actions';
import { AuthState } from '../../core/models/auth.model';

export const initialState: AuthState = {
  user: null,
  accessToken: null,
  refreshToken: null,
  isLoading: false,
  error: null,
};

export const authReducer = createReducer(
  initialState,

  // Login
  on(AuthActions.login, (state) => ({
    ...state,
    isLoading: true,
    error: null,
  })),

  on(AuthActions.loginSuccess, (state, { response }) => ({
    ...state,
    isLoading: false,
    accessToken: response.accessToken,
    refreshToken: response.refreshToken,
    user: {
      userId: response.userId,
      username: response.username,
      level: response.level,
      totalXP: response.totalXP,
    },
    error: null,
  })),

  on(AuthActions.loginFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    error,
  })),

  // Register
  on(AuthActions.register, (state) => ({
    ...state,
    isLoading: true,
    error: null,
  })),

  on(AuthActions.registerSuccess, (state, { response }) => ({
    ...state,
    isLoading: false,
    accessToken: response.accessToken,
    refreshToken: response.refreshToken,
    user: {
      userId: response.userId,
      username: response.username,
      level: response.level,
      totalXP: response.totalXP,
    },
    error: null,
  })),

  on(AuthActions.registerFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    error,
  })),

  // Logout
  on(AuthActions.logout, () => initialState),

  // Initialize from localStorage
  on(AuthActions.initializeAuth, (state) => {
    const accessToken = localStorage.getItem('access_token');
    const refreshToken = localStorage.getItem('refresh_token');
    const userStr = localStorage.getItem('auth_user');
    const user = userStr ? JSON.parse(userStr) : null;

    return {
      ...state,
      accessToken,
      refreshToken,
      user,
    };
  }),
);
