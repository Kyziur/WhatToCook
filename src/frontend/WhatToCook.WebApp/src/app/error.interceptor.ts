import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = '';
      if (error.error instanceof ErrorEvent) {
        errorMessage = `Błąd: ${error.error.message}`;
      } else {
        errorMessage = `Kod błędu: ${error.status}\nWiadomość: ${error.message}`;
      }
      console.error(errorMessage);
      return throwError(() => errorMessage);
    })
  );
};