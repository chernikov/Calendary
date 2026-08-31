import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { NzPopconfirmModule } from 'ng-zorro-antd/popconfirm';
import {
  AdminActions,
  selectAdminBusy,
  selectAdminError,
  selectAdminImageStyles,
} from '../../core/state/admin';
import { SaveImageStylePayload } from '../../core/models';

@Component({
  selector: 'app-admin-styles',
  standalone: true,
  imports: [FormsModule, NzTableModule, NzButtonModule, NzIconModule, NzInputModule, NzAlertModule, NzPopconfirmModule],
  template: `
    <h2>Стилі зображень</h2>
    <p style="color: rgba(0, 0, 0, 0.45); margin-bottom: 16px;">
      Стиль накладається на обраний образ. Текст пишеться англійською: він додається до інструкції
      для AI-генератора.
    </p>

    @if (error(); as err) {
      <nz-alert nzType="error" [nzMessage]="err" nzShowIcon style="display: block; margin-bottom: 16px;"></nz-alert>
    }

    <button nz-button nzType="primary" (click)="startNew()" [disabled]="busy()" style="margin-bottom: 16px;">
      <span nz-icon nzType="plus"></span> Додати стиль
    </button>

    @if (form; as f) {
      <div style="background: #fafafa; border: 1px solid #f0f0f0; border-radius: 6px; padding: 16px; margin-bottom: 16px; max-width: 640px;">
        <h3 style="margin-top: 0;">{{ f.id ? 'Редагувати стиль' : 'Новий стиль' }}</h3>
        <div style="display: grid; gap: 8px;">
          <input nz-input placeholder="Назва (укр)" [(ngModel)]="f.name" />
          <textarea nz-input rows="2" placeholder="Текст стилю англійською" [(ngModel)]="f.text"></textarea>
          <input nz-input type="number" placeholder="Порядок" [(ngModel)]="f.sortOrder" style="max-width: 120px;" />
          <div>
            <button nz-button nzType="primary" [disabled]="!f.name.trim() || !f.text.trim() || busy()" (click)="save()">
              Зберегти
            </button>
            <button nz-button style="margin-left: 8px;" (click)="form = null">Скасувати</button>
          </div>
        </div>
      </div>
    }

    <nz-table [nzData]="styles()" [nzFrontPagination]="false" [nzShowPagination]="false" [nzLoading]="busy()">
      <thead>
        <tr>
          <th style="width: 180px;">Назва</th>
          <th>Текст стилю (EN)</th>
          <th style="width: 90px;">Порядок</th>
          <th style="width: 110px;"></th>
        </tr>
      </thead>
      <tbody>
        @for (style of styles(); track style.id) {
          <tr>
            <td>{{ style.name }}</td>
            <td style="white-space: pre-wrap;">{{ style.text }}</td>
            <td>{{ style.sortOrder }}</td>
            <td>
              <button nz-button nzSize="small" (click)="edit(style.id)"><span nz-icon nzType="edit"></span></button>
              <button
                nz-button
                nzSize="small"
                nzDanger
                nz-popconfirm
                nzPopconfirmTitle="Видалити стиль?"
                (nzOnConfirm)="remove(style.id)"
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
export class AdminStylesComponent implements OnInit {
  private readonly store = inject(Store);
  readonly styles = this.store.selectSignal(selectAdminImageStyles);
  readonly busy = this.store.selectSignal(selectAdminBusy);
  readonly error = this.store.selectSignal(selectAdminError);

  form: SaveImageStylePayload | null = null;

  ngOnInit(): void {
    this.store.dispatch(AdminActions.loadImageStyles());
  }

  startNew(): void {
    this.form = { name: '', text: '', sortOrder: (this.styles().length ?? 0) + 1 };
  }

  edit(styleId: string): void {
    const style = this.styles().find((s) => s.id === styleId);
    if (!style) return;
    this.form = { id: style.id, name: style.name, text: style.text, sortOrder: style.sortOrder };
  }

  save(): void {
    if (!this.form) return;
    this.store.dispatch(AdminActions.saveImageStyle({ style: this.form }));
    this.form = null;
  }

  remove(styleId: string): void {
    this.store.dispatch(AdminActions.deleteImageStyle({ styleId }));
  }
}
