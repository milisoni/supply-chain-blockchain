import { Routes } from '@angular/router';
import { authGuard, adminGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'login', loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./pages/register/register.component').then(m => m.RegisterComponent) },
  {
    path: '',
    loadComponent: () => import('./layout/layout.component').then(m => m.LayoutComponent),
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent) },
      { path: 'shipments', loadComponent: () => import('./pages/shipments/shipments.component').then(m => m.ShipmentsComponent) },
      { path: 'shipments/add', loadComponent: () => import('./pages/shipment-add/shipment-add.component').then(m => m.ShipmentAddComponent) },
      { path: 'shipments/track', loadComponent: () => import('./pages/shipment-track/shipment-track.component').then(m => m.ShipmentTrackComponent) },
      { path: 'shipments/status', loadComponent: () => import('./pages/shipment-status/shipment-status.component').then(m => m.ShipmentStatusComponent) },
      { path: 'payments', loadComponent: () => import('./pages/payments/payments.component').then(m => m.PaymentsComponent) },
      { path: 'transactions', loadComponent: () => import('./pages/transactions/transactions.component').then(m => m.TransactionsComponent) },
      { path: 'profile', loadComponent: () => import('./pages/profile/profile.component').then(m => m.ProfileComponent) },
      { path: 'admin/overview', loadComponent: () => import('./pages/admin-overview/admin-overview.component').then(m => m.AdminOverviewComponent), canActivate: [adminGuard] },
      { path: 'admin/users', loadComponent: () => import('./pages/admin-users/admin-users.component').then(m => m.AdminUsersComponent), canActivate: [adminGuard] }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];
