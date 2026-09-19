import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Store } from '@ngrx/store';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { AdminActions, selectAdminBusy, selectAdminUsers } from '../../core/state/admin';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [DatePipe, NzTableModule, NzTagModule],
  template: `
    <h2>Користувачі</h2>

    <nz-table
      [nzData]="users()?.items ?? []"
      [nzFrontPagination]="false"
      [nzTotal]="users()?.totalCount ?? 0"
      [nzPageIndex]="page()"
      [nzPageSize]="pageSize"
      [nzLoading]="busy()"
      (nzPageIndexChange)="onPageChange($event)"
    >
      <thead>
        <tr>
          <th>Email</th>
          <th>Ім'я</th>
          <th>Роль</th>
          <th>Автентифікація</th>
          <th>Email підтверджено</th>
          <th>Замовлень</th>
          <th>Зареєстровано</th>
        </tr>
      </thead>
      <tbody>
        @for (u of users()?.items ?? []; track u.id) {
          <tr>
            <td>{{ u.email }}</td>
            <td>{{ u.displayName }}</td>
            <td><nz-tag [nzColor]="u.role === 'Admin' ? 'blue' : 'default'">{{ u.role }}</nz-tag></td>
            <td>{{ u.authProvider }}</td>
            <td>{{ u.emailConfirmed ? 'Так' : 'Ні' }}</td>
            <td>{{ u.orderCount }}</td>
            <td>{{ u.createdAtUtc | date: 'short' }}</td>
          </tr>
        }
      </tbody>
    </nz-table>
  `,
})
export class AdminUsersComponent implements OnInit {
  private readonly store = inject(Store);
  readonly users = this.store.selectSignal(selectAdminUsers);
  readonly busy = this.store.selectSignal(selectAdminBusy);

  readonly pageSize = 20;
  readonly page = signal(1);

  ngOnInit(): void {
    this.load();
  }

  onPageChange(page: number): void {
    this.page.set(page);
    this.load();
  }

  private load(): void {
    this.store.dispatch(AdminActions.loadUsers({ page: this.page(), pageSize: this.pageSize }));
  }
}
