import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthStore } from '../auth/auth.store';
import { NotificationService } from './notification.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authStore = inject(AuthStore);
  const router = inject(Router);
  const notifications = inject(NotificationService);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse) {
        if (error.status === 401) {
          authStore.logout();
          router.navigate(['/login']);
        } else if (error.status === 0) {
          notifications.error('Nie można połączyć się z serwerem.');
        } else {
          const message =
            error.error?.detail ?? error.error?.title ?? error.message ?? 'Wystąpił błąd.';
          notifications.error(message);
        }
      }

      return throwError(() => error);
    })
  );
};
