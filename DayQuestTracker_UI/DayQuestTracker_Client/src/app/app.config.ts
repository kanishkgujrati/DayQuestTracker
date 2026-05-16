import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { isDevMode } from '@angular/core';
import { authReducer } from './store/auth/auth.reducers';
import { AuthEffects } from './store/auth/auth.effects';
import { DashboardEffects } from './store/dashboard/dashboard.effects';
import { dashboardReducer } from './store/dashboard/dashboard.reducer';
import { categoryReducer } from './store/category/category.reducer';
import { CategoryEffects } from './store/category/category.effects';
import { taskReducer } from './store/tasks/task.reducer';
import { TaskEffects } from './store/tasks/task.effects';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideStore({
      auth: authReducer,
      dashboard: dashboardReducer,
      category: categoryReducer,
      task: taskReducer,
    }),
    provideEffects(AuthEffects, DashboardEffects, CategoryEffects, TaskEffects),
    provideStoreDevtools({
      maxAge: 25,
      logOnly: !isDevMode(),
    }),
  ],
};
