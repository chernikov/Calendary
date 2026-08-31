import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { FormsModule } from '@angular/forms';
import { AdminActions, selectAdminBusy, selectAdminOrders } from '../../core/state/admin';
import { OrderStatus } from '../../core/models';

const STATUS_OPTIONS: OrderStatus[] = [
  'Created', 'PhotoUploaded', 'DetailsSubmitted', 'Generating', 'CoverReady', 'CoverConfirmed',
  'ReviewReady', 'AwaitingPayment', 'Paid', 'Printing', 'Shipped', 'Delivered', 'Cancelled', 'GenerationFailed',
];

const STATUS_COLORS: Record<OrderStatus, string> = {
  Created: 'default', PhotoUploaded: 'default', DetailsSubmitted: 'default',
  Generating: 'processing', CoverReady: 'processing', CoverConfirmed: 'processing', ReviewReady: 'processing',
  AwaitingPayment: 'gold', Paid: 'blue', Printing: 'blue', Shipped: 'blue',
  Delivered: 'green', Cancelled: 'red', GenerationFailed: 'red',
};

@Component({
  selector: 'app-admin-orders',
  standalone: true,
  imports: [RouterLink, DatePipe, FormsModule, NzTableModule, NzTagModule, NzSelectModule],
  template: `
    <h2>Замовлення</h2>

    <div style="margin-bottom: 16px; max-width: 260px;">
      <nz-select [(ngModel)]="statusFilter" nzAllowClear nzPlaceHolder="Фільтр за статусом" (ngModelChange)="onFilterChange()">
        @for (s of statusOptions; track s) {
          <nz-option [nzValue]="s" [nzLabel]="s"></nz-option>
        }
      </nz-select>
    </div>

    <nz-table
      [nzData]="orders()?.items ?? []"
      [nzFrontPagination]="false"
      [nzTotal]="orders()?.totalCount ?? 0"
      [nzPageIndex]="page()"
      [nzPageSize]="pageSize"
      [nzLoading]="busy()"
      (nzPageIndexChange)="onPageChange($event)"
    >
      <thead>
        <tr>
          <th>ID</th>
          <th>Клієнт</th>
          <th>Статус</th>
          <th>Ціна</th>
          <th>Створено</th>
        </tr>
      </thead>
      <tbody>
        @for (o of orders()?.items ?? []; track o.id) {
          <tr [routerLink]="['/admin', 'orders', o.id]" style="cursor: pointer;">
            <td>{{ o.id.slice(0, 8) }}</td>
            <td>{{ o.userDisplayName || o.userEmail }}</td>
            <td><nz-tag [nzColor]="statusColor(o.status)">{{ o.status }}</nz-tag></td>
            <td>{{ o.price }} ₴</td>
            <td>{{ o.createdAtUtc | date: 'short' }}</td>
          </tr>
        }
      </tbody>
    </nz-table>
  `,
})
export class AdminOrdersComponent implements OnInit {
  private readonly store = inject(Store);
  readonly orders = this.store.selectSignal(selectAdminOrders);
  readonly busy = this.store.selectSignal(selectAdminBusy);

  readonly statusOptions = STATUS_OPTIONS;
  readonly pageSize = 20;
  readonly page = signal(1);
  statusFilter: OrderStatus | null = null;

  ngOnInit(): void {
    this.load();
  }

  onPageChange(page: number): void {
    this.page.set(page);
    this.load();
  }

  onFilterChange(): void {
    this.page.set(1);
    this.load();
  }

  statusColor(status: OrderStatus): string {
    return STATUS_COLORS[status];
  }

  private load(): void {
    this.store.dispatch(
      AdminActions.loadOrders({ page: this.page(), pageSize: this.pageSize, status: this.statusFilter ?? undefined }),
    );
  }
}
