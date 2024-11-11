import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, finalize, timeout } from 'rxjs/operators';
import { BlockUiService } from '../services/block.ui.service';


@Injectable()
export class BlockUiInterceptor implements HttpInterceptor {

  constructor(private blockUiService: BlockUiService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    this.blockUiService.show(); // показуємо блок з loading.gif

    // Встановлюємо таймер для приховування block UI, якщо відповідь не отримана протягом 5 секунд
    const timer = setTimeout(() => {
      this.blockUiService.hide();
    }, 5000);

    return next.handle(req).pipe(
      finalize(() => {
        clearTimeout(timer); // Відміняємо таймер, якщо запит завершився раніше
        this.blockUiService.hide(); // приховуємо блок, коли запит завершено
      })
    );
  }
}