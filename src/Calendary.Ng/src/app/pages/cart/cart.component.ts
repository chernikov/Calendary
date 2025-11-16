import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { CartItemComponent } from '../../components/cart/cart-item/cart-item.component';
import { CartSummaryComponent } from '../../components/cart/cart-summary/cart-summary.component';
import { EmptyCartComponent } from '../../components/cart/empty-cart/empty-cart.component';
import { Order } from '../../../models/order';
import { OrderItem } from '../../../models/order-item';
import { OrderService } from '../../../services/order.service';
import { CartStore } from '../../store/cart.store';

@Component({
    standalone: true,
    selector: 'app-cart',
    imports: [CommonModule, FormsModule, RouterModule, CartItemComponent, CartSummaryComponent, EmptyCartComponent],
    templateUrl: './cart.component.html',
    styleUrl: './cart.component.scss'
})
export class CartComponent implements OnInit, OnDestroy {

  order: Order | null = null;
  previewImage: string | null = null;
  commentValue = '';
  isLoading = false;
  loadError: string | null = null;
  updatingItemId: number | null = null;
  removingItemId: number | null = null;

  private subscriptions = new Subscription();

  constructor(
    private readonly cartStore: CartStore,
    private readonly orderService: OrderService,
    private readonly router: Router
  ) {}

  ngOnInit() {
    this.subscriptions.add(this.cartStore.order$.subscribe((order) => {
      this.order = order;
      this.commentValue = order?.comment ?? '';
    }));

    this.subscriptions.add(this.cartStore.loading$.subscribe((loading) => {
      this.isLoading = loading;
    }));

    this.refreshCart();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  refreshCart(): void {
    this.loadError = null;
    this.cartStore.refreshCart().subscribe({
      error: () => {
        this.loadError = 'Не вдалося завантажити кошик. Спробуйте ще раз.';
      },
    });
  }

  get subtotal(): number {
    if (!this.order) {
      return 0;
    }
    return this.order.items.reduce((sum, item) => sum + item.price * item.quantity, 0);
  }

  get totalItems(): number {
    if (!this.order) {
      return 0;
    }
    return this.order.items.reduce((sum, item) => sum + item.quantity, 0);
  }

  onQuantityChange(item: OrderItem, quantity: number): void {
    if (quantity <= 0 || quantity === item.quantity) {
      return;
    }
    this.updatingItemId = item.id;
    this.cartStore.updateItemQuantity(item.id, quantity).subscribe({
      next: () => (this.updatingItemId = null),
      error: (error) => {
        console.error('Failed to update item quantity', error);
        this.updatingItemId = null;
      },
    });
  }

  onRemoveItem(item: OrderItem): void {
    this.removingItemId = item.id;
    this.cartStore.removeItem(item.id).subscribe({
      next: () => (this.removingItemId = null),
      error: (error) => {
        console.error('Failed to remove cart item', error);
        this.removingItemId = null;
      },
    });
  }

  goToCheckout(): void {
    this.router.navigate(['/checkout']);
  }

  continueShopping(): void {
    this.router.navigate(['/catalog']);
  }

  hasItems(): boolean {
    return !!this.order && this.order.items.length > 0;
  }

  openPreview(imagePath: string | null | undefined): void {
    if (imagePath) {
      this.previewImage = imagePath;
    }
  }

  closePreview(): void {
    this.previewImage = null;
  }

  onCommentChange(): void {
    if (!this.order) {
      return;
    }
    if (this.commentValue === (this.order.comment ?? '')) {
      return;
    }
    this.orderService.updateComment(this.order.id, this.commentValue).subscribe({
      next: () => {
        if (this.order) {
          this.order.comment = this.commentValue;
        }
      },
      error: (err) => console.error('Помилка при збереженні коментаря', err),
    });
  }

  trackByItem(_: number, item: OrderItem): number {
    return item.id;
  }
}
