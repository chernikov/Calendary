import { Component, OnInit } from '@angular/core';
import { OrderService } from '../../../services/order.service';
import { Order } from '../../../models/order';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';

@Component({
    standalone: true,
    selector: 'app-profile-orders',
    imports: [CommonModule, MatTableModule, MatPaginatorModule, MatButtonModule, RouterModule],
    templateUrl: './profile-orders.component.html',
    styleUrls: ['./profile-orders.component.scss']
})
export class ProfileOrdersComponent implements OnInit {

  statusMap: { [key: string]: string } = {
    Creating: 'Створюється',
    Paid: 'Оплачено',
    InPrinting: 'Передано на друк',
    InDelivery: 'Доставляється',
    Delivered: 'Доставлено',
    Declined: 'Відмінено',
    Done: 'Виконано',
  };

  orders: Order[] = [];
  totalOrders = 0;
  pageSize = 10; // Фіксований розмір сторінки, який встановлено на бекенді
  currentPage = 1;

  displayedColumns: string[] = ['id', 'date', 'sum', 'status'];

  constructor(private orderService: OrderService) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.orderService.getUserOrders(this.currentPage).subscribe(
      (data) => {
        this.orders = data.orders;
        this.totalOrders = data.total;
      },
      (error) => {
        console.error('Error loading orders:', error);
      }
    );
  }
  textSum = (order: Order): number => order.items.reduce((total, item) => total + item.price * item.quantity, 0);

  textStatus = (order: Order): string =>  this.statusMap[order.status] || 'Невідомий статус';

  onPageChange(event: any): void {
    this.currentPage = event.pageIndex + 1;
    this.loadOrders();
  }
}
