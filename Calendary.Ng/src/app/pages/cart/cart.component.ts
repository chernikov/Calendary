import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { DeliveryComponent } from '../../components/delivery/delivery.component';
import { CartService } from '../../../services/cart.service';
import { Order } from '../../../models/order';
import { OrderItem } from '../../../models/order-item';
@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, 
    FormsModule, 
    ReactiveFormsModule, 
    DeliveryComponent],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.scss'
})
export class CartComponent implements OnInit {

  order: Order | null = null;
  delivery: any = {};
  isEmailValid = false;
  isPhoneValid = false;

  constructor(private cartService: CartService) {}

  ngOnInit() {
    this.getCart();
  }

  getCart() {
    this.cartService.getCart().subscribe(order => {
      this.order = order;
    });
  }

  calculateTotal(): number {
    if (!this.order) 
      return 0;

    return this.order.items.reduce((total, item) => total + (item.price * item.quantity), 0);
  }

  removeItem(itemId: number) {
    if (!this.order) 
      return;

    this.cartService.deleteItem(itemId).subscribe({
      next: () => {
        this.getCart();
      }
    });
  }

  onUpdateItem(item: OrderItem) {
    this.cartService.updateItem(item).subscribe({
      next: () => {
        this.getCart();
      }
    });
  }

  updateDelivery(deliveryInfo: any) {
    this.delivery = deliveryInfo;
  }

  updateValidationStatus(status: { isEmailValid: boolean; isPhoneValid: boolean }) {
    this.isEmailValid = status.isEmailValid;
    this.isPhoneValid = status.isPhoneValid;
  }

  get isFormValid(): boolean {
    return this.isEmailValid && this.isPhoneValid;
  }

  proceedToPayment() {
    if (this.isFormValid) {
      // Логіка для переходу до оплати
      console.log('Перехід до оплати');
    }
  }
}