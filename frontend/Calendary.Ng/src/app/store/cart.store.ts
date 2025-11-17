import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, catchError, finalize, map, of, tap, throwError } from 'rxjs';
import { Order } from '../../models/order';
import { OrderItem } from '../../models/order-item';
import { CartService } from '../../services/cart.service';

@Injectable({
  providedIn: 'root',
})
export class CartStore {
  private readonly orderSubject = new BehaviorSubject<Order | null>(null);
  readonly order$ = this.orderSubject.asObservable();

  private readonly loadingSubject = new BehaviorSubject<boolean>(false);
  readonly loading$ = this.loadingSubject.asObservable();

  private readonly totalItemsSubject = new BehaviorSubject<number>(0);
  readonly totalItems$ = this.totalItemsSubject.asObservable();

  constructor(private readonly cartService: CartService) {}

  refreshCart(): Observable<Order | null> {
    this.loadingSubject.next(true);
    return this.cartService.getCart().pipe(
      tap((order) => this.setOrder(order)),
      map((order) => order),
      catchError((error: HttpErrorResponse) => {
        if (error.status === 404) {
          this.setOrder(null);
          return of(null);
        }
        return throwError(() => error);
      }),
      finalize(() => this.loadingSubject.next(false))
    );
  }

  syncCartCount(): Observable<number> {
    return this.cartService.itemsInCart().pipe(
      tap((count) => this.totalItemsSubject.next(count))
    );
  }

  updateItemQuantity(itemId: number, quantity: number): Observable<void> {
    const currentOrder = this.orderSubject.value;
    if (!currentOrder) {
      return throwError(() => new Error('Cart is empty'));
    }
    const targetItem = currentOrder.items.find((item) => item.id === itemId);
    if (!targetItem) {
      return throwError(() => new Error('Item not found'));
    }

    const payload: OrderItem = { ...targetItem, quantity };

    return this.cartService.updateItem(payload).pipe(
      tap(() => {
        const updatedOrder: Order = {
          ...currentOrder,
          items: currentOrder.items.map((item) =>
            item.id === itemId ? { ...item, quantity } : item
          ),
        };
        this.setOrder(updatedOrder.items.length ? updatedOrder : null);
      }),
      map(() => void 0)
    );
  }

  removeItem(itemId: number): Observable<void> {
    return this.cartService.deleteItem(itemId).pipe(
      tap(() => {
        const currentOrder = this.orderSubject.value;
        if (!currentOrder) {
          return;
        }
        const remainingItems = currentOrder.items.filter((item) => item.id !== itemId);
        const nextOrder = remainingItems.length
          ? { ...currentOrder, items: remainingItems }
          : null;
        this.setOrder(nextOrder);
      }),
      map(() => void 0)
    );
  }

  getTotalPrice(): number {
    const order = this.orderSubject.value;
    if (!order) {
      return 0;
    }
    return order.items.reduce((sum, item) => sum + item.price * item.quantity, 0);
  }

  getTotalItems(): number {
    return this.totalItemsSubject.value;
  }

  getOrderSnapshot(): Order | null {
    return this.orderSubject.value;
  }

  private setOrder(order: Order | null): void {
    this.orderSubject.next(order);
    const totalItems = order
      ? order.items.reduce((sum, item) => sum + item.quantity, 0)
      : 0;
    this.totalItemsSubject.next(totalItems);
  }
}
