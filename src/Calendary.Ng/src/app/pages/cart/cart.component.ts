import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { DeliveryComponent } from '../../components/delivery/delivery.component';
import { CartService } from '../../../services/cart.service';
import { PaymentService } from '../../../services/payment.service';
import { Order } from '../../../models/order';
import { OrderItem } from '../../../models/order-item';
import { RouterModule } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmationModalComponent } from '../../components/confirmation-modal/confirmation-modal.component';
import { OrderSummaryModalComponent } from '../../components/order-summary-modal/order-summary-modal.component';
import { SummaryOrder } from '../../../models/summary-order';
import { PaymentRedirect } from '../../../models/payment.redirect';
@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, 
    DeliveryComponent, RouterModule],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.scss'
})
export class CartComponent implements OnInit {

  order: Order | null = null;
  delivery: any = {};
  previewImage: string | null = null;
  
  constructor(private cartService: CartService, 
    private paymentService: PaymentService,
    public dialog: MatDialog // Додаємо MatDialog для роботи з модальними вікнами
  ) {}

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


  proceedToPayment() {

    this.cartService.summary().subscribe({
      next: (summary) => {
        var { user } = summary;

        if (!user.isEmailConfirmed && !user.isPhoneNumberConfirmed)
        {
          this.showConfirmationWarning();
        } else {
          this.showOrderSummary(summary);
        }
      },
      error: (error) => {
        console.error('Failed to load order summary:', error);
      }
    });
  }

  showOrderSummary(order: SummaryOrder) {
    const dialogRef = this.dialog.open(OrderSummaryModalComponent, {
      width: '600px',
      data: order
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === 'confirm') {
        this.confirmPayment(); // Метод для підтвердження та переходу до оплати
      }
    });
  }

  // Метод для відображення модального вікна попередження
  showConfirmationWarning() {
    this.dialog.open(ConfirmationModalComponent, {
      width: '400px'
    });
  }

  confirmPayment() {
    this.paymentService.getPay().subscribe({
      next: (redirect) => {
        this.redirectToPayment(redirect);
      },
      error: (error) => {
        console.error('Failed to get payment redirect:', error);
      }
    });
  }
  
  redirectToPayment(redirect: PaymentRedirect) {
    window.open(redirect.paymentPage, '_blank');
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