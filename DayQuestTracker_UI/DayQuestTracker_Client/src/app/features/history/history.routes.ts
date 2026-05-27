import { Routes } from '@angular/router';

export const historyRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./history.component').then((m) => m.HistoryComponent),
  },
];
