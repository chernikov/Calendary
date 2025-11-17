import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { RouterModule } from '@angular/router';
import { Order } from '../../../../models/order';
import { EmptyCartComponent } from '../../cart/empty-cart/empty-cart.component';

@Component({
  selector: 'app-order-summary',
  standalone: true,
  imports: [CommonModule, RouterModule, EmptyCartComponent],
  templateUrl: './order-summary.component.html',
  styleUrl: './order-summary.component.scss',
})
export class OrderSummaryComponent {
  @Input() order: Order | null = null;
  @Input() deliveryCost = 0;
  @Input() isSubmitting = false;

  @Output() confirm = new EventEmitter<void>();
  @Output() backToCart = new EventEmitter<void>();

  get subtotal(): number {
    if (!this.order) {
      return 0;
    }
    return this.order.items.reduce((sum: number, item) => sum + item.price * item.quantity, 0);
  }

  get totalItems(): number {
    if (!this.order) {
      return 0;
    }
    return this.order.items.reduce((sum: number, item) => sum + item.quantity, 0);
  }
}