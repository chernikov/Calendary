import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { NzInputNumberModule } from 'ng-zorro-antd/input-number';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { AdminActions, selectAdminBasePrice } from '../../core/state/admin';

@Component({
    selector: 'app-admin-product',
    imports: [FormsModule, NzInputNumberModule, NzButtonModule, NzAlertModule],
    template: `
    <h2>Товар</h2>
    <p style="color: rgba(0, 0, 0, 0.45); margin-bottom: 16px;">
      Базова ціна фотокалендаря. Нові замовлення отримують цю ціну в момент створення — вона
      "заморожується" на замовленні і не перераховується заднім числом при подальшій зміні тут.
    </p>

    @if (basePrice() !== null) {
      <div style="display: flex; align-items: center; gap: 8px; max-width: 320px;">
        <nz-input-number [(ngModel)]="editedPrice" [nzMin]="1" [nzStep]="50" style="width: 160px;"></nz-input-number>
        <span>₴</span>
        <button nz-button nzType="primary" [disabled]="!editedPrice || editedPrice <= 0" (click)="save()">
          Зберегти
        </button>
      </div>
    }

    @if (justSaved) {
      <nz-alert
        nzType="success"
        nzMessage="Ціну оновлено — застосовується до нових замовлень"
        style="margin-top: 16px; max-width: 420px;"
        nzShowIcon
      ></nz-alert>
    }
  `
})
export class AdminProductComponent implements OnInit {
  private readonly store = inject(Store);
  readonly basePrice = this.store.selectSignal(selectAdminBasePrice);
  editedPrice: number | null = null;
  justSaved = false;

  ngOnInit(): void {
    this.store.dispatch(AdminActions.loadProductSettings());
  }

  constructor() {
    this.store.select(selectAdminBasePrice).subscribe((price) => {
      if (price !== null && this.editedPrice === null) {
        this.editedPrice = price;
      }
    });
  }

  save(): void {
    if (!this.editedPrice || this.editedPrice <= 0) return;
    this.justSaved = false;
    this.store.dispatch(AdminActions.setProductSettings({ basePrice: this.editedPrice }));
    this.justSaved = true;
  }
}
