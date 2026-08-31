import { Component, OnInit, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { NzDescriptionsModule } from 'ng-zorro-antd/descriptions';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzSpinModule } from 'ng-zorro-antd/spin';
import { AdminActions, selectAdminBusy, selectAdminSelectedOrder } from '../../core/state/admin';
import { SheetDto, SheetStatus } from '../../core/models';

const MONTH_NAMES = [
  'Січень', 'Лютий', 'Березень', 'Квітень', 'Травень', 'Червень',
  'Липень', 'Серпень', 'Вересень', 'Жовтень', 'Листопад', 'Грудень',
];

const STATUS_COLORS: Record<SheetStatus, string> = {
  Pending: 'default',
  Generating: 'processing',
  Ready: 'green',
  Failed: 'red',
};

@Component({
  selector: 'app-admin-order-detail',
  standalone: true,
  imports: [DatePipe, RouterLink, NzDescriptionsModule, NzTagModule, NzButtonModule, NzIconModule, NzSpinModule],
  template: `
    <a routerLink="/admin/orders" style="display: inline-flex; align-items: center; gap: 4px; margin-bottom: 16px;">
      <span nz-icon nzType="left"></span> До списку замовлень
    </a>

    @if (busy() && !order()) {
      <nz-spin nzTip="Завантаження замовлення (може тривати довго — фото та аркуші містять великі зображення)…">
        <div style="height: 200px;"></div>
      </nz-spin>
    }

    @if (order(); as o) {
      <h2>Замовлення {{ o.id.slice(0, 8) }}</h2>

      <nz-descriptions [nzBordered]="true" [nzColumn]="2" style="margin-bottom: 24px;">
        <nz-descriptions-item nzTitle="Статус"><nz-tag>{{ o.status }}</nz-tag></nz-descriptions-item>
        <nz-descriptions-item nzTitle="Стиль">{{ o.styleCategory?.name || '—' }}</nz-descriptions-item>
        <nz-descriptions-item nzTitle="Ціна">{{ o.price }} ₴</nz-descriptions-item>
        <nz-descriptions-item nzTitle="Перегенерацій залишилось">{{ o.regenerationsRemaining }}</nz-descriptions-item>
        <nz-descriptions-item nzTitle="Створено">{{ o.createdAtUtc | date: 'short' }}</nz-descriptions-item>
        <nz-descriptions-item nzTitle="Діє до">{{ o.expiresAtUtc | date: 'short' }}</nz-descriptions-item>
        @if (o.payment) {
          <nz-descriptions-item nzTitle="Оплата">{{ o.payment.method }} — {{ o.payment.status }} ({{ o.payment.amount }} ₴)</nz-descriptions-item>
        }
        @if (o.delivery) {
          <nz-descriptions-item nzTitle="Доставка" [nzSpan]="2">
            {{ o.delivery.recipientName }}, {{ o.delivery.phone }}, {{ o.delivery.city }}, відділення {{ o.delivery.warehouseNumber }}
            @if (o.delivery.trackingNumber) { — ТТН {{ o.delivery.trackingNumber }} }
          </nz-descriptions-item>
        }
        @if (o.personalDates.length) {
          <nz-descriptions-item nzTitle="Особисті дати" [nzSpan]="2">
            @for (d of o.personalDates; track d.id) {
              <span style="margin-right: 12px;">{{ d.day }}.{{ d.month }} — {{ d.label }}</span>
            }
          </nz-descriptions-item>
        }
      </nz-descriptions>

      <h3>Фото</h3>
      <div style="display: flex; align-items: flex-start; gap: 16px; margin-bottom: 24px;">
        @if (o.photoUrl) {
          <img [src]="o.photoUrl" style="max-height: 220px; border-radius: 6px;" alt="Фото замовлення" />
        } @else {
          <div style="color: rgba(0,0,0,0.45);">Фото не завантажено</div>
        }
        <div>
          <input #fileInput type="file" accept="image/*" style="display: none;" (change)="onFileSelected($event)" />
          <button nz-button (click)="fileInput.click()">
            <span nz-icon nzType="picture"></span> Замінити фото
          </button>
        </div>
      </div>

      <h3>Аркуші календаря</h3>
      <div style="display: grid; grid-template-columns: repeat(auto-fill, minmax(130px, 1fr)); gap: 12px;">
        @for (sheet of o.sheets; track sheet.id) {
          <div style="text-align: center;">
            @if (sheet.imageUrl) {
              <div
                style="aspect-ratio: 3/4; background-size: cover; background-position: center; border-radius: 6px;"
                [style.background-image]="'url(' + sheet.imageUrl + ')'"
              ></div>
            } @else {
              <div style="aspect-ratio: 3/4; border-radius: 6px; background: #f0f0f0;"></div>
            }
            <div style="font-size: 12px; margin: 6px 0 4px;">
              {{ sheet.kind === 'Cover' ? 'Обкладинка' : monthName(sheet.index) }}
            </div>
            <nz-tag [nzColor]="statusColor(sheet.status)">{{ sheet.status }}</nz-tag>
            <div style="margin-top: 6px;">
              <button
                nz-button
                nzSize="small"
                [disabled]="sheet.status === 'Generating' || busy()"
                (click)="regenerate(o.id, sheet)"
              >
                <span nz-icon nzType="reload"></span> Перегенерувати
              </button>
            </div>
          </div>
        }
      </div>
    }
  `,
})
export class AdminOrderDetailComponent implements OnInit {
  private readonly store = inject(Store);
  private readonly orderId: string;

  readonly order = this.store.selectSignal(selectAdminSelectedOrder);
  readonly busy = this.store.selectSignal(selectAdminBusy);

  constructor(private readonly route: ActivatedRoute) {
    this.orderId = this.route.snapshot.paramMap.get('orderId')!;
  }

  ngOnInit(): void {
    this.store.dispatch(AdminActions.loadOrderDetail({ orderId: this.orderId }));
  }

  monthName(index: number): string {
    return MONTH_NAMES[index - 1] ?? '';
  }

  statusColor(status: SheetStatus): string {
    return STATUS_COLORS[status];
  }

  regenerate(orderId: string, sheet: SheetDto): void {
    this.store.dispatch(AdminActions.regenerateSheet({ orderId, sheetId: sheet.id }));
  }

  onFileSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = () => {
      this.store.dispatch(
        AdminActions.replacePhoto({ orderId: this.orderId, photoDataUrl: reader.result as string }),
      );
    };
    reader.readAsDataURL(file);
  }
}
