import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-cart-summary',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './cart-summary.component.html',
  styleUrl: './cart-summary.component.scss',
})
export class CartSummaryComponent {
  @Input() subtotal = 0;
  @Input() totalItems = 0;
  @Input() isLoading = false;
  @Input() shippingCost = 0;

  @Output() checkout = new EventEmitter<void>();
  @Output() continueShopping = new EventEmitter<void>();

  get total(): number {
    return this.subtotal + this.shippingCost;
  }
}
