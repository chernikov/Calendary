import { Injectable } from '@angular/core';
import { HttpEvent, HttpInterceptor, HttpHandler, HttpRequest, HttpErrorResponse } from '@angular/common/http';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  constructor(private snackBar: MatSnackBar) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        // Перевіряємо, чи це помилка HTTP
        if (error.status >= 500) {
          // Відображаємо повідомлення через MatSnackBar
          this.snackBar.open('Сталася помилка під час запиту', 'Закрити', {
            duration: 3000,
            horizontalPosition: 'center',
            verticalPosition: 'bottom',
          });
        }

        if (error.status == 405) {
          // Відображаємо повідомлення через MatSnackBar
          this.snackBar.open('Ви не авторизовані', 'Закрити', {
            duration: 3000,
            horizontalPosition: 'center',
            verticalPosition: 'bottom',
          });
        }
        if (error.status == 404) {
          // Відображаємо повідомлення через MatSnackBar
          this.snackBar.open('Нема такої сторінки', 'Закрити', {
            duration: 3000,
            horizontalPosition: 'center',
            verticalPosition: 'bottom',
          });
        }
        // Повертаємо помилку для обробки у компоненті або сервісі
        return throwError(error);
      })
    );
  }
}
