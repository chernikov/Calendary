import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { SummaryOrder } from '../../../models/summary-order';
import { OrderService } from '../../../services/order.service';

@Component({
  selector: 'app-order',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './order.component.html',
  styleUrl: './order.component.scss'
})
export class OrderComponent implements OnInit {
  order: SummaryOrder | null = null;
  errorMessage: string | null = null;

  constructor(
    private orderService: OrderService,
    private route: ActivatedRoute
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
}