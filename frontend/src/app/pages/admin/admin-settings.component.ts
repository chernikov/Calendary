import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { NzRadioModule } from 'ng-zorro-antd/radio';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { AdminActions, selectAdminAiProvider, selectAdminConfigStatus } from '../../core/state/admin';
import { ImageGenerationProvider } from '../../core/models';

@Component({
  selector: 'app-admin-settings',
  standalone: true,
  imports: [FormsModule, NzRadioModule, NzAlertModule, NzTagModule],
  template: `
    <h2>Налаштування генерації</h2>
    <p style="color: rgba(0, 0, 0, 0.45); margin-bottom: 16px;">
      Провайдер AI-генерації зображень. Зміна застосовується одразу, без перезапуску сервера.
    </p>

    @if (provider(); as current) {
      <nz-radio-group [ngModel]="current" (ngModelChange)="onChange($event)">
        <label nz-radio-button nzValue="Mock">Mock (заглушка)</label>
        <label nz-radio-button nzValue="OpenAI">OpenAI</label>
        <label nz-radio-button nzValue="Gemini">Gemini</label>
      </nz-radio-group>
    }

    @if (justChanged) {
      <nz-alert
        nzType="success"
        nzMessage="Провайдера оновлено — застосовується одразу"
        style="margin-top: 16px; max-width: 420px;"
        nzShowIcon
      ></nz-alert>
    }

    <h2 style="margin-top: 32px;">Перевірка ключів</h2>
    <p style="color: rgba(0, 0, 0, 0.45); margin-bottom: 16px;">
      Чи налаштований кожен інтеграційний ключ на цьому середовищі. Самі значення ніде не показуються.
    </p>

    @if (configStatus(); as status) {
      <table style="border-collapse: collapse; max-width: 420px;">
        <tbody>
          <tr>
            <td style="padding: 6px 16px 6px 0;">OpenAI API-ключ</td>
            <td><nz-tag [nzColor]="color(status.openAiConfigured)">{{ label(status.openAiConfigured) }}</nz-tag></td>
          </tr>
          <tr>
            <td style="padding: 6px 16px 6px 0;">Gemini API-ключ</td>
            <td><nz-tag [nzColor]="color(status.geminiConfigured)">{{ label(status.geminiConfigured) }}</nz-tag></td>
          </tr>
          <tr>
            <td style="padding: 6px 16px 6px 0;">Google OAuth (Client ID + Secret)</td>
            <td><nz-tag [nzColor]="color(status.googleConfigured)">{{ label(status.googleConfigured) }}</nz-tag></td>
          </tr>
          <tr>
            <td style="padding: 6px 16px 6px 0;">Resend API-ключ</td>
            <td><nz-tag [nzColor]="color(status.resendConfigured)">{{ label(status.resendConfigured) }}</nz-tag></td>
          </tr>
          <tr>
            <td style="padding: 6px 16px 6px 0;">Monobank merchant-токен</td>
            <td><nz-tag [nzColor]="color(status.monobankConfigured)">{{ label(status.monobankConfigured) }}</nz-tag></td>
          </tr>
        </tbody>
      </table>
    }
  `,
})
export class AdminSettingsComponent implements OnInit {
  private readonly store = inject(Store);
  readonly provider = this.store.selectSignal(selectAdminAiProvider);
  readonly configStatus = this.store.selectSignal(selectAdminConfigStatus);
  justChanged = false;

  ngOnInit(): void {
    this.store.dispatch(AdminActions.loadAiProvider());
    this.store.dispatch(AdminActions.loadConfigStatus());
  }

  onChange(provider: ImageGenerationProvider): void {
    this.justChanged = false;
    this.store.dispatch(AdminActions.setAiProvider({ provider }));
    this.justChanged = true;
  }

  label(configured: boolean): string {
    return configured ? 'Налаштовано' : 'Не налаштовано';
  }

  color(configured: boolean): string {
    return configured ? 'green' : 'red';
  }
}
