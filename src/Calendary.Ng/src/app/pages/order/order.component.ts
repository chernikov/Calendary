import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { SummaryOrder } from '../../../models/summary-order';
import { OrderService } from '../../../services/order.service';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { interval, Subscription } from 'rxjs';
import { takeWhile } from 'rxjs/operators';

@Component({
    standalone: true,
    selector: 'app-order',
    imports: [CommonModule, MatButtonModule, MatIconModule],
    templateUrl: './order.component.html',
    styleUrl: './order.component.scss'
})
export class OrderComponent implements OnInit, OnDestroy {
  order: SummaryOrder | null = null;
  errorMessage: string | null = null;
  previewImage: string | null = null;
  paymentMessage: string | null = null;
  paymentMessageType: 'success' | 'error' | 'info' | null = null;
  isPolling: boolean = false;
  pollingSubscription: Subscription | null = null;

  constructor(
    private orderService: OrderService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Отримуємо orderId з URL
    const orderId = Number(this.route.snapshot.paramMap.get('orderId'));

    // Перевіряємо чи користувач повернувся з платіжної сторінки
    const fromPayment = this.route.snapshot.queryParamMap.get('payment');

    this.fetchOrderDetails(orderId);

    // Якщо користувач повернувся з MonoBank, показуємо повідомлення та починаємо polling
    if (fromPayment === 'processing') {
      this.paymentMessage = 'Очікуємо підтвердження оплати від MonoBank...';
      this.paymentMessageType = 'info';
      this.startPaymentPolling(orderId);
    }
  }

  ngOnDestroy(): void {
    this.stopPaymentPolling();
  }

  fetchOrderDetails(orderId: number): void {
    this.orderService.getOrderById(orderId).subscribe({
      next: (order) => {
        this.order = order;
        this.checkPaymentStatus(order);
      },
      error: (error) => (this.errorMessage = error.error || 'Помилка завантаження замовлення')
    });
  }

  checkPaymentStatus(order: SummaryOrder): void {
    if (order.isPaid && this.isPolling) {
      this.paymentMessage = 'Оплата успішно отримана! Дякуємо за замовлення.';
      this.paymentMessageType = 'success';
      this.stopPaymentPolling();
    }
  }

  startPaymentPolling(orderId: number): void {
    this.isPolling = true;
    let pollCount = 0;
    const maxPolls = 60; // Максимум 60 разів (5 хвилин при інтервалі 5 секунд)

    this.pollingSubscription = interval(5000) // Кожні 5 секунд
      .pipe(takeWhile(() => pollCount < maxPolls && this.isPolling))
      .subscribe(() => {
        pollCount++;
        this.fetchOrderDetails(orderId);

        // Якщо досягнуто максимум опитувань без успіху
        if (pollCount >= maxPolls && this.isPolling) {
          this.paymentMessage = 'Не вдалося підтвердити оплату автоматично. Будь ласка, перевірте пізніше або зв\'яжіться з підтримкою.';
          this.paymentMessageType = 'error';
          this.stopPaymentPolling();
        }
      });
  }

  stopPaymentPolling(): void {
    this.isPolling = false;
    if (this.pollingSubscription) {
      this.pollingSubscription.unsubscribe();
      this.pollingSubscription = null;
    }
  }

  goBack(): void {
    this.router.navigate(['/profile']); // Перенаправляє на сторінку профілю
  }

  
  // Відкриття прев'ю
  openPreview(imagePath: string | null | undefined): void {
    if (imagePath) {
      this.previewImage = imagePath;
    }
  }

  // Закриття прев'ю
  closePreview(): void {
    this.previewImage = null;
  }
}
