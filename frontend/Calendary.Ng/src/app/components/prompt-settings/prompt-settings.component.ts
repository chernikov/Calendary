import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { SettingService } from '../../../services/setting.service';
import { Setting } from '../../../models/setting';

@Component({
  standalone: true,
  selector: 'app-prompt-settings',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatSlideToggleModule],
  templateUrl: './prompt-settings.component.html',
  styleUrl: './prompt-settings.component.scss'
})
export class PromptSettingsComponent implements OnInit {
  promptForm: FormGroup;
  isLoading = false;
  saveMessage = '';

  constructor(
    private fb: FormBuilder,
    private settingService: SettingService
  ) {
    this.promptForm = this.fb.group({
      useImprovedPrompt: [false]
    });
  }

  ngOnInit(): void {
    this.loadSettings();
  }

  loadSettings(): void {
    this.isLoading = true;
    this.settingService.getSettings().subscribe({
      next: (settings: Setting) => {
        this.promptForm.patchValue({
          useImprovedPrompt: settings.useImprovedPrompt || false
        });
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Помилка завантаження налаштувань:', error);
        this.isLoading = false;
      }
    });
  }

  onToggleChange(): void {
    const settings = new Setting();
    settings.useImprovedPrompt = this.promptForm.value.useImprovedPrompt;

    this.saveMessage = '';
    this.settingService.saveSettings(settings).subscribe({
      next: () => {
        this.saveMessage = 'Налаштування збережено';
        setTimeout(() => {
          this.saveMessage = '';
        }, 3000);
      },
      error: (error) => {
        console.error('Помилка збереження налаштувань:', error);
        this.saveMessage = 'Помилка збереження налаштувань';
      }
    });
  }
}
