import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzInputNumberLegacyModule } from 'ng-zorro-antd/input-number-legacy';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzSwitchModule } from 'ng-zorro-antd/switch';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { NzPopconfirmModule } from 'ng-zorro-antd/popconfirm';
import {
  AdminActions,
  selectAdminBusy,
  selectAdminError,
  selectAdminPromoCodes,
} from '../../core/state/admin';
import { SavePromoCodePayload } from '../../core/models';

const TYPES = [
  { value: 'Percent', label: 'Відсоток' },
  { value: 'FixedAmount', label: 'Фіксована сума' },
];

// nz-date-picker binds a Date, not the ISO string SavePromoCodePayload carries — this local shape
// mirrors the payload with those two fields swapped, converted at the form's edges (edit/save).
interface PromoCodeFormModel extends Omit<SavePromoCodePayload, 'validFromUtc' | 'validToUtc'> {
  validFromUtc: Date | null;
  validToUtc: Date | null;
}

@Component({
    selector: 'app-admin-promo-codes',
    imports: [
        FormsModule,
        NzTableModule,
        NzButtonModule,
        NzIconModule,
        NzInputModule,
        NzInputNumberLegacyModule,
        NzSelectModule,
        NzSwitchModule,
        NzDatePickerModule,
        NzAlertModule,
        NzPopconfirmModule,
    ],
    template: `
    <h2>Знижки</h2>
    <p style="color: rgba(0, 0, 0, 0.45); margin-bottom: 16px;">
      Промокоди для checkout — клієнт вводить код, знижка розраховується від базової ціни
      замовлення і заморожується на ньому в момент застосування.
    </p>

    @if (error(); as err) {
      <nz-alert nzType="error" [nzMessage]="err" nzShowIcon style="display: block; margin-bottom: 16px;"></nz-alert>
    }

    <button nz-button nzType="primary" (click)="startNew()" [disabled]="busy()" style="margin-bottom: 16px;">
      <span nz-icon nzType="plus"></span> Додати промокод
    </button>

    @if (form; as f) {
      <div style="background: #fafafa; border: 1px solid #f0f0f0; border-radius: 6px; padding: 16px; margin-bottom: 16px; max-width: 640px;">
        <h3 style="margin-top: 0;">{{ f.id ? 'Редагувати промокод' : 'Новий промокод' }}</h3>
        <div style="display: grid; gap: 8px;">
          <input nz-input placeholder="Код (напр. NEWYEAR10)" [(ngModel)]="f.code" style="text-transform: uppercase;" />
          <div style="display: flex; gap: 8px;">
            <nz-select [(ngModel)]="f.type" style="width: 180px;">
              @for (t of types; track t.value) {
                <nz-option [nzValue]="t.value" [nzLabel]="t.label"></nz-option>
              }
            </nz-select>
            <nz-input-number
              [(ngModel)]="f.value"
              [nzMin]="0"
              [nzMax]="f.type === 'Percent' ? 100 : null"
              [nzPrecision]="2"
              [nzPlaceHolder]="f.type === 'Percent' ? 'Відсоток' : 'Сума ₴'"
              style="width: 140px;"
            ></nz-input-number>
          </div>
          <div style="display: flex; gap: 8px; align-items: center;">
            <nz-date-picker [(ngModel)]="f.validFromUtc" nzPlaceHolder="Діє з" style="width: 160px;"></nz-date-picker>
            <nz-date-picker [(ngModel)]="f.validToUtc" nzPlaceHolder="Діє по" style="width: 160px;"></nz-date-picker>
          </div>
          <div style="display: flex; gap: 8px;">
            <nz-input-number [(ngModel)]="f.maxRedemptions" [nzMin]="1" nzPlaceHolder="Ліміт використань" style="width: 180px;"></nz-input-number>
            <nz-input-number [(ngModel)]="f.minOrderAmount" [nzMin]="0" nzPlaceHolder="Мін. сума замовлення ₴" style="width: 200px;"></nz-input-number>
          </div>
          <label style="display: flex; align-items: center; gap: 8px;">
            <nz-switch [(ngModel)]="f.isActive"></nz-switch> Активний
          </label>
          <div>
            <button nz-button nzType="primary" [disabled]="!f.code.trim() || !f.value || busy()" (click)="save()">
              Зберегти
            </button>
            <button nz-button style="margin-left: 8px;" (click)="form = null">Скасувати</button>
          </div>
        </div>
      </div>
    }

    <nz-table [nzData]="promoCodes()" [nzFrontPagination]="false" [nzShowPagination]="false" [nzLoading]="busy()">
      <thead>
        <tr>
          <th style="width: 140px;">Код</th>
          <th style="width: 120px;">Тип</th>
          <th style="width: 90px;">Значення</th>
          <th style="width: 110px;">Використано</th>
          <th style="width: 90px;">Активний</th>
          <th style="width: 110px;"></th>
        </tr>
      </thead>
      <tbody>
        @for (promo of promoCodes(); track promo.id) {
          <tr>
            <td>{{ promo.code }}</td>
            <td>{{ typeLabel(promo.type) }}</td>
            <td>{{ promo.value }}{{ promo.type === 'Percent' ? '%' : ' ₴' }}</td>
            <td>{{ promo.redemptionsUsed }}{{ promo.maxRedemptions ? ' / ' + promo.maxRedemptions : '' }}</td>
            <td>{{ promo.isActive ? 'Так' : 'Ні' }}</td>
            <td>
              <button nz-button nzSize="small" (click)="edit(promo.id)"><span nz-icon nzType="edit"></span></button>
              <button
                nz-button
                nzSize="small"
                nzDanger
                nz-popconfirm
                nzPopconfirmTitle="Видалити промокод?"
                (nzOnConfirm)="remove(promo.id)"
                style="margin-left: 4px;"
              >
                <span nz-icon nzType="delete"></span>
              </button>
            </td>
          </tr>
        }
      </tbody>
    </nz-table>
  `
})
export class AdminPromoCodesComponent implements OnInit {
  private readonly store = inject(Store);
  readonly promoCodes = this.store.selectSignal(selectAdminPromoCodes);
  readonly busy = this.store.selectSignal(selectAdminBusy);
  readonly error = this.store.selectSignal(selectAdminError);

  readonly types = TYPES;

  form: PromoCodeFormModel | null = null;

  ngOnInit(): void {
    this.store.dispatch(AdminActions.loadPromoCodes());
  }

  typeLabel(value: string): string {
    return this.types.find((t) => t.value === value)?.label ?? value;
  }

  startNew(): void {
    this.form = {
      code: '',
      type: 'Percent',
      value: 10,
      validFromUtc: null,
      validToUtc: null,
      maxRedemptions: null,
      minOrderAmount: null,
      isActive: true,
    };
  }

  edit(promoCodeId: string): void {
    const promo = this.promoCodes().find((p) => p.id === promoCodeId);
    if (!promo) return;
    this.form = {
      id: promo.id,
      code: promo.code,
      type: promo.type,
      value: promo.value,
      validFromUtc: promo.validFromUtc ? new Date(promo.validFromUtc) : null,
      validToUtc: promo.validToUtc ? new Date(promo.validToUtc) : null,
      maxRedemptions: promo.maxRedemptions,
      minOrderAmount: promo.minOrderAmount,
      isActive: promo.isActive,
    };
  }

  save(): void {
    if (!this.form) return;
    const { validFromUtc, validToUtc, ...rest } = this.form;
    this.store.dispatch(AdminActions.savePromoCode({
      promoCode: {
        ...rest,
        code: this.form.code.trim().toUpperCase(),
        validFromUtc: validFromUtc ? validFromUtc.toISOString() : null,
        validToUtc: validToUtc ? validToUtc.toISOString() : null,
      },
    }));
    this.form = null;
  }

  remove(promoCodeId: string): void {
    this.store.dispatch(AdminActions.deletePromoCode({ promoCodeId }));
  }
}
