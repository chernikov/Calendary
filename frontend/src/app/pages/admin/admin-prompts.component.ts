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
  selectAdminPromptThemes,
} from '../../core/state/admin';
import { SavePromptPayload, SavePromptThemePayload } from '../../core/models';

@Component({
  selector: 'app-admin-prompts',
  standalone: true,
  imports: [FormsModule, NzTableModule, NzButtonModule, NzIconModule, NzInputModule, NzAlertModule, NzPopconfirmModule],
  template: `
    <h2>Бібліотека промптів</h2>
    <p style="color: rgba(0, 0, 0, 0.45); margin-bottom: 16px;">
      Теми — це папки образів. Текст промпта пишеться англійською: він підставляється в середину
      стандартної інструкції для AI-генератора.
    </p>

    @if (error(); as err) {
      <nz-alert nzType="error" [nzMessage]="err" nzShowIcon style="display: block; margin-bottom: 16px;"></nz-alert>
    }

    <button nz-button nzType="primary" (click)="startNewTheme()" [disabled]="busy()" style="margin-bottom: 16px;">
      <span nz-icon nzType="plus"></span> Додати тему
    </button>

    @if (themeForm; as form) {
      <div style="background: #fafafa; border: 1px solid #f0f0f0; border-radius: 6px; padding: 16px; margin-bottom: 16px; max-width: 640px;">
        <h3 style="margin-top: 0;">{{ form.id ? 'Редагувати тему' : 'Нова тема' }}</h3>
        <div style="display: grid; gap: 8px;">
          <input nz-input placeholder="Назва" [(ngModel)]="form.name" />
          <input nz-input placeholder="Опис" [(ngModel)]="form.description" />
          <input nz-input type="number" placeholder="Порядок" [(ngModel)]="form.sortOrder" style="max-width: 120px;" />
          <div>
            <button nz-button nzType="primary" [disabled]="!form.name.trim() || busy()" (click)="saveTheme()">Зберегти</button>
            <button nz-button style="margin-left: 8px;" (click)="themeForm = null">Скасувати</button>
          </div>
        </div>
      </div>
    }

    @for (theme of themes(); track theme.id) {
      <div style="border: 1px solid #f0f0f0; border-radius: 6px; padding: 16px; margin-bottom: 16px;">
        <div style="display: flex; align-items: baseline; gap: 12px; margin-bottom: 8px;">
          <h3 style="margin: 0;">{{ theme.name }}</h3>
          <span style="color: rgba(0, 0, 0, 0.45);">{{ theme.description }}</span>
          <span style="flex: 1;"></span>
          <button nz-button nzSize="small" (click)="editTheme(theme.id)"><span nz-icon nzType="edit"></span></button>
          <button
            nz-button
            nzSize="small"
            nzDanger
            nz-popconfirm
            nzPopconfirmTitle="Видалити тему разом з її промптами?"
            (nzOnConfirm)="deleteTheme(theme.id)"
          >
            <span nz-icon nzType="delete"></span>
          </button>
        </div>

        <nz-table [nzData]="theme.prompts" [nzFrontPagination]="false" [nzShowPagination]="false" nzSize="small">
          <thead>
            <tr>
              <th style="width: 180px;">Образ</th>
              <th>Текст промпта (EN)</th>
              <th style="width: 90px;">Порядок</th>
              <th style="width: 110px;"></th>
            </tr>
          </thead>
          <tbody>
            @for (prompt of theme.prompts; track prompt.id) {
              <tr>
                <td>{{ prompt.name }}</td>
                <td style="white-space: pre-wrap;">{{ prompt.text }}</td>
                <td>{{ prompt.sortOrder }}</td>
                <td>
                  <button nz-button nzSize="small" (click)="editPrompt(theme.id, prompt.id)">
                    <span nz-icon nzType="edit"></span>
                  </button>
                  <button
                    nz-button
                    nzSize="small"
                    nzDanger
                    nz-popconfirm
                    nzPopconfirmTitle="Видалити промпт?"
                    (nzOnConfirm)="deletePrompt(prompt.id)"
                    style="margin-left: 4px;"
                  >
                    <span nz-icon nzType="delete"></span>
                  </button>
                </td>
              </tr>
            }
          </tbody>
        </nz-table>

        @if (promptForm && promptForm.promptThemeId === theme.id) {
          <div style="background: #fafafa; border: 1px solid #f0f0f0; border-radius: 6px; padding: 12px; margin-top: 8px;">
            <div style="display: grid; gap: 8px;">
              <input nz-input placeholder="Назва образу (укр)" [(ngModel)]="promptForm.name" />
              <textarea nz-input rows="3" placeholder="Текст промпта англійською" [(ngModel)]="promptForm.text"></textarea>
              <input nz-input placeholder="Короткий опис для клієнта (укр, 5–10 слів)" [(ngModel)]="promptForm.description" />
              <input nz-input placeholder="URL превʼю-зображення (необовʼязково)" [(ngModel)]="promptForm.previewImageUrl" />
              <input nz-input type="number" placeholder="Порядок" [(ngModel)]="promptForm.sortOrder" style="max-width: 120px;" />
              <div>
                <button
                  nz-button
                  nzType="primary"
                  [disabled]="!promptForm.name.trim() || !promptForm.text.trim() || busy()"
                  (click)="savePrompt()"
                >
                  Зберегти
                </button>
                <button nz-button style="margin-left: 8px;" (click)="promptForm = null">Скасувати</button>
              </div>
            </div>
          </div>
        } @else {
          <button nz-button nzSize="small" style="margin-top: 8px;" (click)="startNewPrompt(theme.id)">
            <span nz-icon nzType="plus"></span> Додати образ
          </button>
        }
      </div>
    }
  `,
})
export class AdminPromptsComponent implements OnInit {
  private readonly store = inject(Store);
  readonly themes = this.store.selectSignal(selectAdminPromptThemes);
  readonly busy = this.store.selectSignal(selectAdminBusy);
  readonly error = this.store.selectSignal(selectAdminError);

  themeForm: SavePromptThemePayload | null = null;
  promptForm: SavePromptPayload | null = null;

  ngOnInit(): void {
    this.store.dispatch(AdminActions.loadPromptThemes());
  }

  startNewTheme(): void {
    this.themeForm = { name: '', description: '', sortOrder: (this.themes().length ?? 0) + 1 };
  }

  editTheme(themeId: string): void {
    const theme = this.themes().find((t) => t.id === themeId);
    if (!theme) return;
    this.themeForm = { id: theme.id, name: theme.name, description: theme.description, sortOrder: theme.sortOrder };
  }

  saveTheme(): void {
    if (!this.themeForm) return;
    this.store.dispatch(AdminActions.savePromptTheme({ theme: this.themeForm }));
    this.themeForm = null;
  }

  deleteTheme(themeId: string): void {
    this.store.dispatch(AdminActions.deletePromptTheme({ themeId }));
  }

  startNewPrompt(themeId: string): void {
    const theme = this.themes().find((t) => t.id === themeId);
    this.promptForm = {
      promptThemeId: themeId,
      name: '',
      text: '',
      description: '',
      previewImageUrl: null,
      sortOrder: (theme?.prompts.length ?? 0) + 1,
    };
  }

  editPrompt(themeId: string, promptId: string): void {
    const prompt = this.themes()
      .find((t) => t.id === themeId)
      ?.prompts.find((p) => p.id === promptId);
    if (!prompt) return;
    this.promptForm = {
      id: prompt.id,
      promptThemeId: prompt.promptThemeId,
      name: prompt.name,
      text: prompt.text,
      description: prompt.description,
      previewImageUrl: prompt.previewImageUrl,
      sortOrder: prompt.sortOrder,
    };
  }

  savePrompt(): void {
    if (!this.promptForm) return;
    this.store.dispatch(AdminActions.savePrompt({ prompt: this.promptForm }));
    this.promptForm = null;
  }

  deletePrompt(promptId: string): void {
    this.store.dispatch(AdminActions.deletePrompt({ promptId }));
  }
}
