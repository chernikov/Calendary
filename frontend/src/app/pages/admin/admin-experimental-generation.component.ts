import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzRadioModule } from 'ng-zorro-antd/radio';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { NzSpinModule } from 'ng-zorro-antd/spin';
import { NzIconModule } from 'ng-zorro-antd/icon';
import {
  AdminActions,
  selectAdminBusy,
  selectAdminConfigStatus,
  selectAdminExperimentalGenerationResult,
} from '../../core/state/admin';
import { ExperimentalSheetKind, ImageGenerationProvider } from '../../core/models';

const MONTH_NAMES = [
  'Січень', 'Лютий', 'Березень', 'Квітень', 'Травень', 'Червень',
  'Липень', 'Серпень', 'Вересень', 'Жовтень', 'Листопад', 'Грудень',
];

@Component({
    selector: 'app-admin-experimental-generation',
    imports: [FormsModule, NzButtonModule, NzInputModule, NzRadioModule, NzSelectModule, NzAlertModule, NzSpinModule, NzIconModule],
    template: `
    <h2>Тестова генерація</h2>
    <p style="color: rgba(0, 0, 0, 0.45); margin-bottom: 16px; max-width: 640px;">
      Прямий виклик реального AI-провайдера для перевірки якості промптів — той самий шлях, що й
      генерація в замовленні, але без збереження результату в бібліотеку промптів/стилів чи
      будь-яке замовлення. Кожен запуск — окрема, нічим не пов'язана спроба.
    </p>

    @if (configStatus(); as status) {
      @if (!status.openAiConfigured && !status.geminiConfigured) {
        <nz-alert
          nzType="warning"
          nzMessage="Жоден AI-провайдер не налаштований на цьому середовищі — тестова генерація неможлива."
          style="max-width: 640px; margin-bottom: 16px;"
          nzShowIcon
        ></nz-alert>
      }
    }

    <div style="display: flex; flex-direction: column; gap: 16px; max-width: 640px;">
      <div>
        <label style="display: block; margin-bottom: 4px;">Референс-фото</label>
        <input #fileInput type="file" accept="image/*" style="display: none;" (change)="onFileSelected($event)" />
        <div style="display: flex; align-items: center; gap: 12px;">
          <button nz-button (click)="fileInput.click()">
            <span nz-icon nzType="picture"></span> {{ photoPreviewUrl ? 'Змінити фото' : 'Обрати фото' }}
          </button>
          @if (photoPreviewUrl) {
            <img [src]="photoPreviewUrl" alt="Референс-фото" style="height: 64px; border-radius: 6px;" />
          }
        </div>
      </div>

      <div>
        <label style="display: block; margin-bottom: 4px;">Сцена (англійською, як у Prompt.Text)</label>
        <textarea nz-input [(ngModel)]="sceneText" rows="2" placeholder="e.g. an astronaut floating in space"></textarea>
      </div>

      <div>
        <label style="display: block; margin-bottom: 4px;">Стиль (англійською, як у ImageStyle.Text)</label>
        <textarea nz-input [(ngModel)]="styleText" rows="2" placeholder="e.g. watercolor painting, soft pastel colors"></textarea>
      </div>

      <div>
        <label style="display: block; margin-bottom: 4px;">Тип аркуша</label>
        <nz-radio-group [(ngModel)]="kind">
          <label nz-radio-button nzValue="Cover">Обкладинка</label>
          <label nz-radio-button nzValue="Month">Місяць</label>
        </nz-radio-group>
        @if (kind === 'Month') {
          <nz-select [(ngModel)]="month" style="width: 160px; margin-left: 12px;" nzPlaceHolder="Місяць">
            @for (name of monthNames; track name; let i = $index) {
              <nz-option [nzValue]="i + 1" [nzLabel]="name"></nz-option>
            }
          </nz-select>
        }
      </div>

      @if (configStatus(); as status) {
        <div>
          <label style="display: block; margin-bottom: 4px;">Провайдер</label>
          <nz-radio-group [(ngModel)]="provider">
            @if (status.openAiConfigured) {
              <label nz-radio-button nzValue="OpenAI">OpenAI</label>
            }
            @if (status.geminiConfigured) {
              <label nz-radio-button nzValue="Gemini">Gemini</label>
            }
          </nz-radio-group>
        </div>
      }

      <div>
        <button nz-button nzType="primary" [disabled]="!canGenerate() || busy()" (click)="generate()">
          {{ result() ? 'Згенерувати ще раз' : 'Згенерувати' }}
        </button>
      </div>
    </div>

    <div style="margin-top: 24px; max-width: 640px;">
      @if (busy()) {
        <nz-spin nzTip="Генерується..."></nz-spin>
      }

      @if (!busy() && result(); as r) {
        @if (r.success && r.imageDataUrl) {
          <img [src]="r.imageDataUrl" alt="Результат генерації" style="max-width: 100%; border-radius: 8px;" />
          @if (r.estimatedCostUsd !== null) {
            <p style="color: rgba(0, 0, 0, 0.45); margin-top: 8px;">
              Орієнтовна вартість: \${{ r.estimatedCostUsd }}
            </p>
          }
        } @else {
          <nz-alert nzType="error" [nzMessage]="r.error ?? 'Генерація не вдалася.'" nzShowIcon></nz-alert>
        }
      }
    </div>
  `
})
export class AdminExperimentalGenerationComponent {
  private readonly store = inject(Store);
  readonly configStatus = this.store.selectSignal(selectAdminConfigStatus);
  readonly busy = this.store.selectSignal(selectAdminBusy);
  readonly result = this.store.selectSignal(selectAdminExperimentalGenerationResult);
  readonly monthNames = MONTH_NAMES;

  sceneText = '';
  styleText = '';
  kind: ExperimentalSheetKind = 'Cover';
  month: number | null = null;
  provider: ImageGenerationProvider | null = null;
  photo: File | null = null;
  photoPreviewUrl: string | null = null;

  constructor() {
    this.store.dispatch(AdminActions.loadConfigStatus());
  }

  onFileSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    this.photo = file;
    if (this.photoPreviewUrl) {
      URL.revokeObjectURL(this.photoPreviewUrl);
    }
    this.photoPreviewUrl = URL.createObjectURL(file);
  }

  canGenerate(): boolean {
    return !!(
      this.photo &&
      this.sceneText.trim() &&
      this.styleText.trim() &&
      this.provider &&
      (this.kind === 'Cover' || (this.month !== null && this.month >= 1 && this.month <= 12))
    );
  }

  generate(): void {
    if (!this.canGenerate() || !this.photo || !this.provider) return;
    this.store.dispatch(AdminActions.generateExperimentalImage({
      sceneText: this.sceneText.trim(),
      styleText: this.styleText.trim(),
      kind: this.kind,
      month: this.kind === 'Month' ? this.month : null,
      provider: this.provider,
      photo: this.photo,
    }));
  }
}
