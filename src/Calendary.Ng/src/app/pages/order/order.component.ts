import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { SummaryOrder } from '../../../models/summary-order';
import { OrderService } from '../../../services/order.service';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
    selector: 'app-order',
    imports: [CommonModule, MatButtonModule, MatIconModule],
    templateUrl: './order.component.html',
    styleUrl: './order.component.scss'
})
export class OrderComponent implements OnInit {
  order: SummaryOrder | null = null;
  errorMessage: string | null = null;
  previewImage: string | null = null;
  constructor(
    private orderService: OrderService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Отримуємо orderId з URL
    const orderId = Number(this.route.snapshot.paramMap.get('orderId'));
    this.fetchOrderDetails(orderId);
  }

  fetchOrderDetails(orderId: number): void {
    this.orderService.getOrderById(orderId).subscribe({
      next: (order) => (this.order = order),
      error: (error) => (this.errorMessage = error.error || 'Помилка завантаження замовлення')
    });
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