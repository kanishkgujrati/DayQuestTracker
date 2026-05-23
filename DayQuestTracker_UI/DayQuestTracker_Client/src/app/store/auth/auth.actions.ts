import { createAction, props } from '@ngrx/store';
import {
  AuthResponse,
  AuthUser,
  LoginRequest,
  RegisterRequest,
} from '../../core/models/auth.models';

// Login
export const login = createAction('[Auth] Login', props<{ request: LoginRequest }>());

export const loginSuccess = createAction(
  '[Auth] Login Success',
  props<{ response: AuthResponse }>(),
);

export const loginFailure = createAction('[Auth] Login Failure', props<{ error: string }>());

// Register
export const register = createAction('[Auth] Register', props<{ request: RegisterRequest }>());

export const registerSuccess = createAction(
  '[Auth] Register Success',
  props<{ response: AuthResponse }>(),
);

export const registerFailure = createAction('[Auth] Register Failure', props<{ error: string }>());

// Logout
export const logout = createAction('[Auth] Logout');

// Initialize auth from localStorage on app start
export const initializeAuth = createAction('[Auth] Initialize');

export const updateCurrentUser = createAction(
  '[Auth] Update Current User',
  props<{ user: AuthUser }>(),
);
