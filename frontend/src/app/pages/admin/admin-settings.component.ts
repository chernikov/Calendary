import { Component, OnInit, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { NzRadioModule } from 'ng-zorro-antd/radio';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { NzSwitchModule } from 'ng-zorro-antd/switch';
import {
  AdminActions,
  selectAdminAiProvider,
  selectAdminBackupStatus,
  selectAdminConfigStatus,
  selectAdminRealIntegrationsOnStaging,
} from '../../core/state/admin';
import { ImageGenerationProvider } from '../../core/models';

@Component({
    selector: 'app-admin-settings',
    imports: [FormsModule, NzRadioModule, NzAlertModule, NzTagModule, NzSwitchModule, DatePipe],
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

    <h2 style="margin-top: 32px;">Реальні інтеграції на стейджингу</h2>
    <p style="color: rgba(0, 0, 0, 0.45); margin-bottom: 8px; max-width: 520px;">
      За замовчуванням стейджинг завжди імітує SMS (код 0000) і ТТН Нової Пошти (фейковий номер) —
      навіть якщо ключі налаштовані. Цей перемикач вмикає реальні, платні виклики для тестування.
      На проді жодного ефекту не має — там завжди реально, незалежно від цього перемикача.
    </p>

    @if (configStatus(); as status) {
      <p style="color: rgba(0, 0, 0, 0.45); margin-bottom: 16px;">
        SMS.Club ключ: <nz-tag [nzColor]="color(status.smsClubConfigured)">{{ label(status.smsClubConfigured) }}</nz-tag>
        Nova Poshta відправник: <nz-tag [nzColor]="color(status.novaPoshtaSenderConfigured)">{{ label(status.novaPoshtaSenderConfigured) }}</nz-tag>
      </p>
    }

    @if (realIntegrationsOnStaging(); as enabled) {
      <nz-switch [ngModel]="enabled" (ngModelChange)="onToggleRealIntegrations($event)"></nz-switch>
      <span style="margin-left: 8px;">{{ enabled ? 'Реальні SMS/ТТН увімкнено' : 'Реальні SMS/ТТН вимкнено (за замовчуванням)' }}</span>
      @if (enabled) {
        <nz-alert
          nzType="warning"
          nzMessage="Увага: кожен запит на верифікацію телефону надішле реальну платну SMS, а кожна відправка замовлення створить реальну накладну в Новій Пошті."
          style="margin-top: 12px; max-width: 520px;"
          nzShowIcon
        ></nz-alert>
      }
    }

    <h2 style="margin-top: 32px;">Бекапи</h2>
    <p style="color: rgba(0, 0, 0, 0.45); margin-bottom: 16px;">
      Read-only перегляд снепшотів restic (deploy/backup.sh). Запуск бекапу звідси не передбачений
      — лише через systemd-таймер на дроплеті.
    </p>

    @if (backupStatus(); as status) {
      @if (!status.configured) {
        <p class="text-muted">Не налаштовано (немає DO_SPACES_KEY/RESTIC_PASSWORD у .env).</p>
      } @else if (status.snapshots.length === 0) {
        <p class="text-muted">Налаштовано, але жодного снепшота ще не знайдено.</p>
      } @else {
        <table style="border-collapse: collapse; max-width: 480px;">
          <tbody>
            @for (s of status.snapshots; track s.timeUtc) {
              <tr>
                <td style="padding: 6px 16px 6px 0;">{{ s.timeUtc | date: 'dd.MM.yyyy HH:mm' }}</td>
                <td style="padding: 6px 0; color: rgba(0, 0, 0, 0.45);">{{ s.tags.join(', ') }}</td>
              </tr>
            }
          </tbody>
        </table>
      }
    }
  `
})
export class AdminSettingsComponent implements OnInit {
  private readonly store = inject(Store);
  readonly provider = this.store.selectSignal(selectAdminAiProvider);
  readonly configStatus = this.store.selectSignal(selectAdminConfigStatus);
  readonly backupStatus = this.store.selectSignal(selectAdminBackupStatus);
  readonly realIntegrationsOnStaging = this.store.selectSignal(selectAdminRealIntegrationsOnStaging);
  justChanged = false;

  ngOnInit(): void {
    this.store.dispatch(AdminActions.loadAiProvider());
    this.store.dispatch(AdminActions.loadConfigStatus());
    this.store.dispatch(AdminActions.loadBackupStatus());
    this.store.dispatch(AdminActions.loadRealIntegrationsOnStaging());
  }

  onToggleRealIntegrations(enabled: boolean): void {
    this.store.dispatch(AdminActions.setRealIntegrationsOnStaging({ enabled }));
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
