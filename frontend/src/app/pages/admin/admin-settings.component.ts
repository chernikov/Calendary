import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { NzRadioModule } from 'ng-zorro-antd/radio';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { AdminActions, selectAdminAiProvider } from '../../core/state/admin';
import { ImageGenerationProvider } from '../../core/models';

@Component({
  selector: 'app-admin-settings',
  standalone: true,
  imports: [FormsModule, NzRadioModule, NzAlertModule],
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
  `,
})
export class AdminSettingsComponent implements OnInit {
  private readonly store = inject(Store);
  readonly provider = this.store.selectSignal(selectAdminAiProvider);
  justChanged = false;

  ngOnInit(): void {
    this.store.dispatch(AdminActions.loadAiProvider());
  }

  onChange(provider: ImageGenerationProvider): void {
    this.justChanged = false;
    this.store.dispatch(AdminActions.setAiProvider({ provider }));
    this.justChanged = true;
  }
}
