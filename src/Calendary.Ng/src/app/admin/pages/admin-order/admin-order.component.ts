import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { AdminOrderService } from '../../../../services/admin-order.service';
import { AdminOrder } from '../../../../models/admin-order';
import { OrderStatusDialogComponent } from '../../components/order-status-dialog/order-status-dialog.component';


@Component({
  selector: 'app-admin-order',
  standalone: true,
  imports:[CommonModule, MatDialogModule, MatTableModule, MatPaginatorModule, MatIconModule, MatButtonModule, MatSelectModule],
  templateUrl: './admin-order.component.html',
  styleUrls: ['./admin-order.component.scss'],
})
export class AdminOrderComponent implements OnInit {
  orders: AdminOrder[] = [];
  totalOrders = 0;
  pageSize = 10;
  currentPage = 1;

  displayedColumns: string[] = ['id', 'user', 'status', 'date', 'items', 'actions'];

  constructor(private adminOrderService: AdminOrderService, 
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.adminOrderService.getOrders(this.currentPage, this.pageSize).subscribe(
      (data) => {
        this.orders = data.orders;
        this.totalOrders = data.total;
      },
      (error) => {
        console.error('Error loading orders:', error);
      }
    );
  }

  onPageChange(event: any): void {
    this.currentPage = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadOrders();
  }

  openStatusDialog(order: any): void {
    const dialogRef = this.dialog.open(OrderStatusDialogComponent, {
      width: '400px',
      data: { status: order.status },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.updateOrderStatus(order.id, result);
      }
    });
  }

  updateOrderStatus(orderId: number, newStatus: string): void {
    this.adminOrderService.updateOrderStatus(orderId, newStatus).subscribe(
      () => {
        console.log('Статус замовлення оновлено');
        this.loadOrders(); // Перезавантажуємо список замовлень
      },
      (error) => {
        console.error('Помилка при оновленні статусу:', error);
      }
    );
  }
}
