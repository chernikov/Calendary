import { Component, Inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatSliderModule } from '@angular/material/slider';
import { MatRadioModule } from '@angular/material/radio';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { PromptTheme } from '../../../../../models/prompt-theme';
import { PromptThemeService } from '../../../../../services/prompt-theme.service';
import { JobService } from '../../../../../services/job.service';
import { Job } from '../../../../../models/job';
import { ImageGenerationSignalRService, ProgressUpdate } from '../../services/image-generation-signalr.service';
import { Observable } from 'rxjs';
import { SettingService } from '../../../../../services/setting.service';

export interface GenerateModalData {
  fluxModelId: number;
  fluxModelName: string;
}

export type GenerationMode = 'default' | 'theme' | 'custom';

@Component({
    standalone: true,
    selector: 'app-generate-modal',
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatDialogModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatButtonModule,
        MatProgressSpinnerModule,
        MatProgressBarModule,
        MatIconModule,
        MatCheckboxModule,
        MatExpansionModule,
        MatSliderModule,
        MatRadioModule,
        MatTooltipModule
    ],
    templateUrl: './generate-modal.component.html',
    styleUrls: ['./generate-modal.component.scss', './generate-modal-advanced.component.scss']
})
export class GenerateModalComponent implements OnInit, OnDestroy {
  generateForm: FormGroup;
  isLoading = false;
  isGenerating = false;
  error = '';
  promptThemes: PromptTheme[] = [];
  generationMode: GenerationMode = 'theme';
  showAdvancedOptions = false;
  progress$: Observable<ProgressUpdate | null>;
  currentJobId: number | null = null;
  useImprovedPrompt = false;

  // Image dimension presets
  imageSizePresets = [
    { label: 'Квадрат 1:1', width: 1024, height: 1024 },
    { label: 'Портрет 2:3', width: 768, height: 1152 },
    { label: 'Пейзаж 3:2', width: 1152, height: 768 },
    { label: 'Широкий 16:9', width: 1344, height: 768 },
    { label: 'Ультра-широкий 21:9', width: 1536, height: 640 }
  ];

  constructor(
    private fb: FormBuilder,
    private promptThemeService: PromptThemeService,
    private jobService: JobService,
    private signalRService: ImageGenerationSignalRService,
    private settingService: SettingService,
    private dialogRef: MatDialogRef<GenerateModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: GenerateModalData
  ) {
    this.progress$ = this.signalRService.progress$;
    this.generateForm = this.fb.group({
      // Generation mode
      mode: ['theme'],

      // Theme-based generation
      promptThemeId: [null],

      // Custom prompt generation
      customPrompt: [''],

      // Advanced options
      seed: [null],
      numImages: [4, [Validators.min(1), Validators.max(12)]],
      width: [1024, [Validators.min(512), Validators.max(2048)]],
      height: [1024, [Validators.min(512), Validators.max(2048)]],
      guidanceScale: [7.5, [Validators.min(1), Validators.max(20)]],
      inferenceSteps: [50, [Validators.min(1), Validators.max(150)]]
    });

    // Update validators based on mode
    this.generateForm.get('mode')?.valueChanges.subscribe((mode: GenerationMode) => {
      this.generationMode = mode;
      this.updateValidators(mode);
    });
  }

  async ngOnInit(): Promise<void> {
    this.loadPromptThemes();
    this.loadUserSettings();
    this.updateValidators(this.generationMode);

    // Підключитися до SignalR при ініціалізації
    try {
      await this.signalRService.connect();
    } catch (err) {
      console.error('Failed to connect to SignalR:', err);
      // Продовжуємо роботу без real-time оновлень
    }
  }

  async ngOnDestroy(): Promise<void> {
    // Відключитися від SignalR при знищенні компонента
    await this.signalRService.disconnect();
  }

  loadUserSettings(): void {
    this.settingService.getSettings().subscribe({
      next: (settings) => {
        this.useImprovedPrompt = settings.useImprovedPrompt || false;
      },
      error: (err) => {
        console.error('Помилка завантаження налаштувань користувача:', err);
        // Default to false if settings can't be loaded
        this.useImprovedPrompt = false;
      }
    });
  }

  loadPromptThemes(): void {
    this.isLoading = true;
    this.promptThemeService.getAll().subscribe({
      next: (themes) => {
        this.promptThemes = themes.filter(t => t.isPublished);
        this.isLoading = false;
      },
      error: () => {
        this.error = 'Не вдалося завантажити теми промптів';
        this.isLoading = false;
      }
    });
  }

  updateValidators(mode: GenerationMode): void {
    const promptThemeIdControl = this.generateForm.get('promptThemeId');
    const customPromptControl = this.generateForm.get('customPrompt');

    // Clear all validators first
    promptThemeIdControl?.clearValidators();
    customPromptControl?.clearValidators();

    // Set validators based on mode
    if (mode === 'theme') {
      promptThemeIdControl?.setValidators([Validators.required]);
    } else if (mode === 'custom') {
      customPromptControl?.setValidators([Validators.required, Validators.minLength(3)]);
    }

    // Update validity
    promptThemeIdControl?.updateValueAndValidity();
    customPromptControl?.updateValueAndValidity();
  }

  applyImageSizePreset(preset: { width: number; height: number }): void {
    this.generateForm.patchValue({
      width: preset.width,
      height: preset.height
    });
  }

  generateRandomSeed(): void {
    const randomSeed = Math.floor(Math.random() * 2147483647);
    this.generateForm.patchValue({ seed: randomSeed });
  }

  clearSeed(): void {
    this.generateForm.patchValue({ seed: null });
  }

  async onGenerate(): Promise<void> {
    if (this.generationMode !== 'default' && this.generateForm.invalid) {
      // Mark all fields as touched to show validation errors
      Object.keys(this.generateForm.controls).forEach(key => {
        this.generateForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.isGenerating = true;
    this.error = '';
    this.signalRService.resetProgress();

    // Note: Currently the backend only supports default and theme-based generation
    // Custom prompts and advanced options are prepared for future backend support
    let generateObservable;

    if (this.generationMode === 'default') {
      generateObservable = this.jobService.createDefaultJob(this.data.fluxModelId);
    } else if (this.generationMode === 'theme') {
      const promptThemeId = this.generateForm.value.promptThemeId;
      generateObservable = this.jobService.createJob(this.data.fluxModelId, promptThemeId);
    } else {
      // Custom mode - for now, we'll use default generation
      // TODO: Implement custom prompt API endpoint
      this.error = 'Генерація з власним промптом буде додана в наступній версії';
      this.isGenerating = false;
      return;
    }

    generateObservable.subscribe({
      next: async (job: Job) => {
        this.currentJobId = job.id;

        // Підписатися на прогрес для кожної задачі в Job
        // Для простоти підписуємося на перше завдання
        // TODO: Відстежувати прогрес всіх завдань
        if (job.tasks && job.tasks.length > 0) {
          const firstTaskId = job.tasks[0].id;
          try {
            await this.signalRService.joinTaskGroup(firstTaskId.toString());
          } catch (err) {
            console.error('Failed to join task group:', err);
          }
        }

        // Add job to processing queue
        this.jobService.messageJob(job.id).subscribe({
          next: () => {
            // Не закриваємо модал одразу - чекаємо на завершення прогресу
            // this.isGenerating = false;
            // this.dialogRef.close(job);
          },
          error: (err) => {
            this.error = 'Не вдалося додати задачу в чергу обробки';
            this.isGenerating = false;
          }
        });

        // Підписатися на оновлення прогресу
        const progressSub = this.progress$.subscribe(progress => {
          if (progress && progress.progress === 100) {
            // Генерація завершена
            setTimeout(() => {
              this.isGenerating = false;
              this.dialogRef.close(job);
            }, 1000); // Невелика затримка для відображення 100%
          }

          if (progress && progress.error) {
            this.error = progress.error;
            this.isGenerating = false;
          }
        });
      },
      error: () => {
        this.error = 'Не вдалося створити задачу генерації';
        this.isGenerating = false;
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  get isFormValid(): boolean {
    if (this.generationMode === 'default') {
      return true;
    }
    return this.generateForm.valid;
  }
}
