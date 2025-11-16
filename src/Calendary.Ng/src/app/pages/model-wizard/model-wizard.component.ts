import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatStepperModule } from '@angular/material/stepper';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { Subject, takeUntil } from 'rxjs';

import { ModelCreationService, ModelCreationState } from '../../../services/model-creation.service';
import { ModelStatus, ModelStatusHelper } from '../../../models/model-status.enum';
import { FluxModel } from '../../../models/flux-model';

/**
 * Wizard компонент для створення моделі
 * Замінює старий master flow з правильною архітектурою
 */
@Component({
  selector: 'app-model-wizard',
  standalone: true,
  imports: [
    CommonModule,
    MatStepperModule,
    MatButtonModule,
    MatProgressBarModule,
    MatCardModule,
    MatIconModule
  ],
  templateUrl: './model-wizard.component.html',
  styleUrls: ['./model-wizard.component.scss']
})
export class ModelWizardComponent implements OnInit, OnDestroy {
  private readonly modelCreationService = inject(ModelCreationService);
  private readonly router = inject(Router);
  private readonly destroy$ = new Subject<void>();

  // State
  state: ModelCreationState = {
    model: null,
    status: ModelStatus.Created,
    progress: 0,
    error: null,
    isLoading: false
  };

  // Stepper state
  currentStep = 0;
  selectedCategoryId: number | null = null;
  uploadedPhotos: File[] = [];

  // Enum для використання в template
  ModelStatus = ModelStatus;

  // Helper для відображення статусу
  ModelStatusHelper = ModelStatusHelper;

  ngOnInit(): void {
    // Підписка на зміни стану моделі
    this.modelCreationService.state$
      .pipe(takeUntil(this.destroy$))
      .subscribe(state => {
        this.state = state;
        this.updateStepFromStatus(state.status);
      });

    // Перевірка чи є поточна модель
    this.loadCurrentModel();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Завантаження поточної моделі якщо вона є
   */
  private loadCurrentModel(): void {
    this.modelCreationService.getCurrentModel().subscribe({
      next: model => {
        if (model) {
          this.selectedCategoryId = model.categoryId;
          this.updateStepFromStatus(model.status as ModelStatus);
        }
      },
      error: () => {
        // Немає поточної моделі - це нормально для нових користувачів
        this.currentStep = 0;
      }
    });
  }

  /**
   * Оновлення поточного кроку на основі статусу
   */
  private updateStepFromStatus(status: ModelStatus): void {
    const stepMap: Record<ModelStatus, number> = {
      [ModelStatus.Created]: 0,
      [ModelStatus.Uploading]: 1,
      [ModelStatus.AwaitingPayment]: 2,
      [ModelStatus.Preparing]: 3,
      [ModelStatus.Training]: 3,
      [ModelStatus.Trained]: 4,
      [ModelStatus.ExamplesGenerated]: 4,
      [ModelStatus.GeneratingImages]: 5,
      [ModelStatus.ImagesSelected]: 6,
      [ModelStatus.DatesAdded]: 6,
      [ModelStatus.Ready]: 7,
      [ModelStatus.Failed]: this.currentStep,
      [ModelStatus.Archived]: 0
    };

    const newStep = stepMap[status];
    if (newStep !== undefined && newStep > this.currentStep) {
      this.currentStep = newStep;
    }
  }

  /**
   * Крок 1: Вибір категорії та створення моделі
   */
  onCategorySelected(categoryId: number): void {
    this.selectedCategoryId = categoryId;

    this.modelCreationService.createModel({
      categoryId,
      name: `Модель ${new Date().toLocaleDateString()}`
    }).subscribe({
      next: () => {
        this.currentStep = 1;
      },
      error: error => {
        console.error('Помилка створення моделі:', error);
      }
    });
  }

  /**
   * Крок 2: Завантаження фото
   */
  onPhotosSelected(photos: File[]): void {
    if (!this.state.model) {
      console.error('Модель не створена');
      return;
    }

    this.uploadedPhotos = photos;

    this.modelCreationService.uploadPhotos(this.state.model.id, photos).subscribe({
      next: () => {
        this.currentStep = 2;
      },
      error: error => {
        console.error('Помилка завантаження фото:', error);
      }
    });
  }

  /**
   * Крок 3: Оплата завершена
   */
  onPaymentCompleted(): void {
    if (!this.state.model) return;

    this.modelCreationService.startTraining(this.state.model.id).subscribe({
      next: () => {
        this.currentStep = 3;
      },
      error: error => {
        console.error('Помилка початку навчання:', error);
      }
    });
  }

  /**
   * Крок 4: Навчання завершено, генерація прикладів
   */
  onTrainingCompleted(): void {
    if (!this.state.model) return;

    this.modelCreationService.generateExamples(this.state.model.id).subscribe({
      next: () => {
        this.currentStep = 4;
      },
      error: error => {
        console.error('Помилка генерації прикладів:', error);
      }
    });
  }

  /**
   * Крок 5: Вибір теми та генерація зображень
   */
  onThemeSelected(promptThemeId?: number): void {
    if (!this.state.model) return;

    this.modelCreationService.generateImages(this.state.model.id, promptThemeId).subscribe({
      next: () => {
        this.currentStep = 5;
      },
      error: error => {
        console.error('Помилка генерації зображень:', error);
      }
    });
  }

  /**
   * Крок 6: Зображення згенеровані, переходимо до редактора
   */
  onImagesGenerated(): void {
    if (!this.state.model) return;

    // Перехід до редактора календаря
    this.router.navigate(['/editor'], {
      queryParams: { modelId: this.state.model.id }
    });
  }

  /**
   * Скасування процесу
   */
  onCancel(): void {
    this.modelCreationService.clearState();
    this.router.navigate(['/']);
  }

  /**
   * Перезапуск процесу після помилки
   */
  onRetry(): void {
    if (!this.state.model) return;

    const status = this.state.status;

    // В залежності від статусу повторюємо відповідну операцію
    if (status === ModelStatus.Uploading) {
      this.onPhotosSelected(this.uploadedPhotos);
    } else if (status === ModelStatus.Training || status === ModelStatus.Preparing) {
      this.onPaymentCompleted();
    } else if (status === ModelStatus.GeneratingImages) {
      this.onThemeSelected();
    }
  }

  /**
   * Перевірка чи можна перейти до наступного кроку
   */
  canProceed(): boolean {
    if (this.state.isLoading) return false;
    if (this.state.error) return false;

    switch (this.currentStep) {
      case 0:
        return this.selectedCategoryId !== null;
      case 1:
        return this.uploadedPhotos.length >= 12;
      case 2:
        return this.state.model?.isPaid === true;
      case 3:
        return this.state.status === ModelStatus.Trained;
      case 4:
        return this.state.status === ModelStatus.ExamplesGenerated;
      case 5:
        return this.state.status === ModelStatus.ImagesSelected;
      default:
        return false;
    }
  }

  /**
   * Отримання назви поточного кроку
   */
  getStepTitle(): string {
    const titles = [
      'Виберіть категорію',
      'Завантажте фото',
      'Оплата',
      'Навчання моделі',
      'Перегляд прикладів',
      'Генерація зображень',
      'Створення календаря',
      'Готово'
    ];
    return titles[this.currentStep] || '';
  }

  /**
   * Отримання опису поточного кроку
   */
  getStepDescription(): string {
    const descriptions = [
      'Оберіть категорію для вашої моделі (людина, тварина, об\'єкт)',
      'Завантажте мінімум 12 фотографій для навчання моделі',
      'Оплатіть навчання моделі для продовження',
      'Модель навчається на ваших фотографіях. Це може зайняти 10-20 хвилин',
      'Переглянете згенеровані приклади та оберіть тему',
      'Генерація зображень для календаря',
      'Розподіліть зображення по місяцям та додайте дати',
      'Ваш календар готовий!'
    ];
    return descriptions[this.currentStep] || '';
  }
}
