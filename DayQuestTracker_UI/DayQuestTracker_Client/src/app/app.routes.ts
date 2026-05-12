import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { LayoutComponent } from './shared/components/layout/layout.component';

export const routes: Routes = [
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then((m) => m.authRoutes),
  },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        canActivate: [authGuard],
        loadChildren: () =>
          import('./features/dashboard/dashboard.routes').then((m) => m.dashboardRoutes),
      },
      {
        path: 'categories',
        canActivate: [authGuard],
        loadChildren: () =>
          import('./features/categories/categories.routes').then((m) => m.categoriesRoutes),
      },
      {
        path: 'tasks',
        canActivate: [authGuard],
        loadChildren: () => import('./features/tasks/tasks.routes').then((m) => m.tasksRoutes),
      },
      {
        path: 'analytics',
        canActivate: [authGuard],
        loadChildren: () =>
          import('./features/analytics/analytics.routes').then((m) => m.analyticsRoutes),
      },
      {
        path: 'profile',
        canActivate: [authGuard],
        loadChildren: () =>
          import('./features/profile/profile.routes').then((m) => m.profileRoutes),
      },
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
    ],
  },
  {
    path: '**',
    redirectTo: 'dashboard',
  },
];
