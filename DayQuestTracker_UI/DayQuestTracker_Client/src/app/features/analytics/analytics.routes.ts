import { Routes } from '@angular/router';

export const analyticsRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('../analytics/analytics.components').then((m) => m.AnalyticsComponent),
  },
];
