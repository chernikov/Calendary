import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/landing/landing.component').then((m) => m.LandingComponent),
  },
  {
    path: 'start',
    loadComponent: () => import('./pages/start/start.component').then((m) => m.StartComponent),
  },
  {
    path: 'order/:orderId/upload',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/upload/upload.component').then((m) => m.UploadComponent),
  },
  {
    path: 'order/:orderId/style',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/style-dates/style-dates.component').then((m) => m.StyleDatesComponent),
  },
  {
    path: 'order/:orderId/generating',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/generating/generating.component').then((m) => m.GeneratingComponent),
  },
  {
    path: 'order/:orderId/cover',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/cover/cover.component').then((m) => m.CoverComponent),
  },
  {
    path: 'order/:orderId/months/:month',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/month/month.component').then((m) => m.MonthComponent),
  },
  {
    path: 'order/:orderId/review',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/review/review.component').then((m) => m.ReviewComponent),
  },
  {
    path: 'order/:orderId/checkout',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/checkout/checkout.component').then((m) => m.CheckoutComponent),
  },
  {
    path: 'order/:orderId/status',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/status/status.component').then((m) => m.StatusComponent),
  },
  { path: '**', redirectTo: '' },
];
