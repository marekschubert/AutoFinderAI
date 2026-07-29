import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'chat' },
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login)
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/register/register').then((m) => m.Register)
  },
  {
    path: 'chat',
    canActivate: [authGuard],
    loadComponent: () => import('./features/chat/chat-page/chat-page').then((m) => m.ChatPage)
  },
  {
    path: 'chat/:sessionId',
    canActivate: [authGuard],
    loadComponent: () => import('./features/chat/chat-page/chat-page').then((m) => m.ChatPage)
  },
  { path: '**', redirectTo: 'chat' }
];
