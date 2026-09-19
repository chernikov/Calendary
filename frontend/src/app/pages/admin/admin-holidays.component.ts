import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzInputNumberModule } from 'ng-zorro-antd/input-number';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { NzPopconfirmModule } from 'ng-zorro-antd/popconfirm';
import {
  AdminActions,
  selectAdminBusy,
  selectAdminError,
  selectAdminHolidays,
} from '../../core/state/admin';
import { SaveHolidayPayload } from '../../core/models';

const COUNTRIES = [
  { value: 'Ukraine', label: 'Україна' },
  { value: 'Usa', label: 'США' },
  { value: 'Poland', label: 'Польща' },
  { value: 'Germany', label: 'Німеччина' },
  { value: 'Czechia', label: 'Чехія' },
];

@Component({
  selector: 'app-admin-holidays',
  standalone: true,
  imports: [
    FormsModule,
    NzTableModule,
    NzButtonModule,
    NzIconModule,
    NzInputModule,
    NzInputNumberModule,
    NzSelectModule,
    NzAlertModule,
    NzPopconfirmModule,
  ],
  template: `
    <h2>Свята</h2>
    <p style="color: rgba(0, 0, 0, 0.45); margin-bottom: 16px;">
      Державні свята за країною і роком — клієнт обирає, які позначати в календарі, на кроці
      персональних дат.
    </p>

    @if (error(); as err) {
      <nz-alert nzType="error" [nzMessage]="err" nzShowIcon style="display: block; margin-bottom: 16px;"></nz-alert>
    }

    <button nz-button nzType="primary" (click)="startNew()" [disabled]="busy()" style="margin-bottom: 16px;">
      <span nz-icon nzType="plus"></span> Додати свято
    </button>

    @if (form; as f) {
      <div style="background: #fafafa; border: 1px solid #f0f0f0; border-radius: 6px; padding: 16px; margin-bottom: 16px; max-width: 640px;">
        <h3 style="margin-top: 0;">{{ f.id ? 'Редагувати свято' : 'Нове свято' }}</h3>
        <div style="display: grid; gap: 8px;">
          <nz-select [(ngModel)]="f.country" nzPlaceHolder="Країна">
            @for (c of countries; track c.value) {
              <nz-option [nzValue]="c.value" [nzLabel]="c.label"></nz-option>
            }
          </nz-select>
          <input nz-input placeholder="Назва свята" [(ngModel)]="f.name" />
          <div style="display: flex; gap: 8px;">
            <nz-input-number [(ngModel)]="f.year" [nzMin]="2024" [nzMax]="2100" nzPlaceHolder="Рік" style="width: 100px;"></nz-input-number>
            <nz-input-number [(ngModel)]="f.month" [nzMin]="1" [nzMax]="12" nzPlaceHolder="Місяць" style="width: 100px;"></nz-input-number>
            <nz-input-number [(ngModel)]="f.day" [nzMin]="1" [nzMax]="31" nzPlaceHolder="День" style="width: 100px;"></nz-input-number>
          </div>
          <div>
            <button nz-button nzType="primary" [disabled]="!f.country || !f.name.trim() || busy()" (click)="save()">
              Зберегти
            </button>
            <button nz-button style="margin-left: 8px;" (click)="form = null">Скасувати</button>
          </div>
        </div>
      </div>
    }

    <nz-table [nzData]="holidays()" [nzFrontPagination]="false" [nzShowPagination]="false" [nzLoading]="busy()">
      <thead>
        <tr>
          <th style="width: 140px;">Країна</th>
          <th style="width: 80px;">Рік</th>
          <th style="width: 90px;">Дата</th>
          <th>Назва</th>
          <th style="width: 110px;"></th>
        </tr>
      </thead>
      <tbody>
        @for (holiday of holidays(); track holiday.id) {
          <tr>
            <td>{{ countryLabel(holiday.country) }}</td>
            <td>{{ holiday.year }}</td>
            <td>{{ pad(holiday.day) }}.{{ pad(holiday.month) }}</td>
            <td>{{ holiday.name }}</td>
            <td>
              <button nz-button nzSize="small" (click)="edit(holiday.id)"><span nz-icon nzType="edit"></span></button>
              <button
                nz-button
                nzSize="small"
                nzDanger
                nz-popconfirm
                nzPopconfirmTitle="Видалити свято?"
                (nzOnConfirm)="remove(holiday.id)"
                style="margin-left: 4px;"
              >
                <span nz-icon nzType="delete"></span>
              </button>
            </td>
          </tr>
        }
      </tbody>
    </nz-table>
  `,
})
export class AdminHolidaysComponent implements OnInit {
  private readonly store = inject(Store);
  readonly holidays = this.store.selectSignal(selectAdminHolidays);
  readonly busy = this.store.selectSignal(selectAdminBusy);
  readonly error = this.store.selectSignal(selectAdminError);

  readonly countries = COUNTRIES;

  form: SaveHolidayPayload | null = null;

  ngOnInit(): void {
    this.store.dispatch(AdminActions.loadHolidays());
  }

  countryLabel(value: string): string {
    return this.countries.find((c) => c.value === value)?.label ?? value;
  }

  pad(n: number): string {
    return n.toString().padStart(2, '0');
  }

  startNew(): void {
    this.form = { country: 'Ukraine', year: 2027, month: 1, day: 1, name: '' };
  }

  edit(holidayId: string): void {
    const holiday = this.holidays().find((h) => h.id === holidayId);
    if (!holiday) return;
    this.form = {
      id: holiday.id,
      country: holiday.country,
      year: holiday.year,
      month: holiday.month,
      day: holiday.day,
      name: holiday.name,
    };
  }

  save(): void {
    if (!this.form) return;
    this.store.dispatch(AdminActions.saveHoliday({ holiday: this.form }));
    this.form = null;
  }

  remove(holidayId: string): void {
    this.store.dispatch(AdminActions.deleteHoliday({ holidayId }));
  }
}
