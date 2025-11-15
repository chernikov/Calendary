import { Component, OnDestroy, OnInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Subject, takeUntil, take } from 'rxjs';
import { FluxModel } from '../../../models/flux-model';
import { FluxModelService } from '../../../services/flux-model.service';
import { JobTask } from '../../../models/job-task';
import { Job } from '../../../models/job';
import { ImageGalleryComponent } from './components/image-gallery/image-gallery.component';
import { ImageCanvasComponent } from './components/image-canvas/image-canvas.component';
import { PropertiesPanelComponent } from './components/properties-panel/properties-panel.component';
import { GenerateModalComponent } from './components/generate-modal/generate-modal.component';
import { CalendarPreviewComponent } from './components/calendar-preview/calendar-preview.component';
import { CalendarBuilderService } from './services/calendar-builder.service';
import { MonthAssignment, MONTHS } from './models/calendar-assignment.model';
import { MonthSelectorComponent, MonthSelectorResult } from './components/month-selector/month-selector.component';
import { JobTaskService } from '../../../services/job-task.service';
import { SidebarComponent } from './components/sidebar/sidebar.component';
import { HistoryComponent } from './components/history/history.component';
import { ToolbarComponent } from './components/toolbar/toolbar.component';
import { EditorStateService } from './services/editor-state.service';

@Component({
    selector: 'app-editor',
    imports: [
        CommonModule,
        RouterModule,
        MatIconModule,
        MatButtonModule,
        MatProgressSpinnerModule,
        MatSnackBarModule,
        MatDialogModule,
        ImageGalleryComponent,
        ImageCanvasComponent,
        PropertiesPanelComponent,
        CalendarPreviewComponent,
        SidebarComponent,
        HistoryComponent,
        ToolbarComponent
    ],
    templateUrl: './editor.component.html',
    styleUrl: './editor.component.scss'
})
export class EditorComponent implements OnInit, OnDestroy {
  isLoading = true;
  loadError = '';
  userModels: FluxModel[] = [];
  activeModel: FluxModel | null = null;
  selectedImage: JobTask | null = null;
  assignments: MonthAssignment[] = [];
  duplicateImageIds: string[] = [];
  private destroy$ = new Subject<void>();

  constructor(
    private fluxModelService: FluxModelService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private calendarBuilder: CalendarBuilderService,
    private jobTaskService: JobTaskService,
    private editorStateService: EditorStateService
  ) {}

  @HostListener('window:keydown', ['$event'])
  handleKeyboardEvent(event: KeyboardEvent): void {
    // Handle Ctrl+Z (Undo)
    if (event.ctrlKey && event.key === 'z' && !event.shiftKey) {
      event.preventDefault();
      if (this.editorStateService.undo()) {
        this.snackBar.open('Дію скасовано', '', { duration: 2000 });
      }
    }

    // Handle Ctrl+Y (Redo) or Ctrl+Shift+Z
    if ((event.ctrlKey && event.key === 'y') || (event.ctrlKey && event.shiftKey && event.key === 'z')) {
      event.preventDefault();
      if (this.editorStateService.redo()) {
        this.snackBar.open('Дію повторено', '', { duration: 2000 });
      }
    }
  }

  ngOnInit(): void {
    this.subscribeToAssignments();
    this.loadUserModels();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadUserModels(): void {
    this.isLoading = true;
    this.fluxModelService.current().subscribe({
      next: (model) => {
        const hasModel = model && model.id;
        this.userModels = hasModel ? [model] : [];
        this.activeModel = hasModel ? model : null;
        this.isLoading = false;
      },
      error: () => {
        this.loadError = 'Не вдалося завантажити моделі користувача.';
        this.userModels = [];
        this.activeModel = null;
        this.isLoading = false;
      },
    });
  }

  selectModel(model: FluxModel): void {
    this.activeModel = model;
    this.selectedImage = null;
  }

  onImageSelected(image: JobTask): void {
    this.selectedImage = image;
  }

  onImageDeleted(image: JobTask): void {
    const confirmed = confirm('Видалити зображення? Це також прибере його з календаря.');
    if (!confirmed) {
      return;
    }

    this.jobTaskService.delete(image.id)
      .pipe(take(1))
      .subscribe({
        next: () => {
          this.calendarBuilder.removeAssignmentsByImageId(image.id.toString());
          this.removeImageFromActiveModel(image.id);

          if (this.selectedImage?.id === image.id) {
            this.selectedImage = null;
          }

          this.snackBar.open('Зображення видалено та прибране з календаря', 'OK', {
            duration: 3000
          });
        },
        error: () => {
          this.snackBar.open('Не вдалося видалити зображення. Спробуйте ще раз.', 'OK', {
            duration: 3500
          });
        }
      });
  }

  onImageSaved(blob: Blob): void {
    // TODO: Implement saving edited image to server
    this.editorStateService.markAsSaved();
    this.snackBar.open('Зображення збережено', 'OK', {
      duration: 3000
    });
  }

  onExportImage(): void {
    if (!this.selectedImage) {
      this.snackBar.open('Спочатку оберіть зображення', 'OK', {
        duration: 3000
      });
      return;
    }

    // Download the image
    const imageUrl = this.selectedImage.processedImageUrl || this.selectedImage.imageUrl;
    if (imageUrl) {
      const link = document.createElement('a');
      link.href = imageUrl;
      link.download = `image_${this.selectedImage.id}.${this.editorStateService.currentState.imageFormat.toLowerCase()}`;
      link.click();
      this.snackBar.open('Зображення експортовано', 'OK', {
        duration: 3000
      });
    }
  }

  openGenerateModal(): void {
    if (!this.activeModel) {
      this.snackBar.open('Спочатку оберіть модель', 'OK', {
        duration: 3000
      });
      return;
    }

    const dialogRef = this.dialog.open(GenerateModalComponent, {
      width: '600px',
      data: {
        fluxModelId: this.activeModel.id,
        fluxModelName: this.activeModel.name || `FluxModel #${this.activeModel.id}`
      }
    });

    dialogRef.afterClosed().subscribe((result: Job | undefined) => {
      if (result) {
        this.snackBar.open('Генерація розпочата! Зображення з\'являться після завершення.', 'OK', {
          duration: 5000
        });
        // Reload the model to get updated jobs
        this.loadUserModels();
      }
    });
  }

  uploadImage(): void {
    // TODO: Implement image upload
    this.snackBar.open('Завантаження зображень буде реалізовано пізніше', 'OK', {
      duration: 3000
    });
  }

  onAddToCalendar(image: JobTask): void {
    const dialogRef = this.dialog.open(MonthSelectorComponent, {
      width: '420px',
      data: {
        month: this.calendarBuilder.getAssignmentByImageId(image.id.toString())?.month,
        assignments: this.assignments,
        selectedImageId: image.id.toString()
      }
    });

    dialogRef.afterClosed().subscribe((result: MonthSelectorResult | undefined) => {
      if (result?.month) {
        this.assignImageToMonth(image, result.month);
      }
    });
  }

  onMonthSlotSelected(month: number): void {
    if (!this.allImages.length) {
      this.snackBar.open('Створіть або завантажте зображення, щоб заповнити календар', 'OK', {
        duration: 3000
      });
      return;
    }

    const dialogRef = this.dialog.open(MonthSelectorComponent, {
      width: '520px',
      data: {
        month,
        assignments: this.assignments,
        allowImageSelection: true,
        images: this.allImages.map((image) => ({
          id: image.id.toString(),
          url: this.getImageUrl(image)
        })),
        selectedImageId: this.selectedImage?.id?.toString()
      }
    });

    dialogRef.afterClosed().subscribe((result: MonthSelectorResult | undefined) => {
      if (result?.month && result?.imageId) {
        const image = this.allImages.find((item) => item.id.toString() === result.imageId);
        if (image) {
          this.assignImageToMonth(image, result.month);
          this.selectedImage = image;
        } else {
          this.snackBar.open('Не вдалося знайти вибране зображення', 'OK', { duration: 2500 });
        }
      }
    });
  }

  onMonthAssignmentCleared(month: number): void {
    this.calendarBuilder.removeAssignment(month);
    this.snackBar.open(`Призначення для ${this.getMonthName(month)} очищено`, 'OK', { duration: 2500 });
  }

  onClearAllAssignments(): void {
    if (!this.assignments.length) {
      return;
    }

    const confirmed = confirm('Очистити всі призначення місяців?');
    if (!confirmed) {
      return;
    }

    this.calendarBuilder.clear();
    this.snackBar.open('Всі призначення очищено', 'OK', { duration: 2500 });
  }

  onMonthSwap(payload: { from: number; to: number }): void {
    const { from, to } = payload;
    const fromAssignment = this.calendarBuilder.getAssignment(from);
    const toAssignment = this.calendarBuilder.getAssignment(to);

    if (!fromAssignment) {
      this.snackBar.open('Спочатку заповніть місяць, щоб перетягнути його.', 'OK', { duration: 2500 });
      return;
    }

    if (toAssignment) {
      this.calendarBuilder.assignImageToMonth(to, fromAssignment.imageId, fromAssignment.imageUrl);
      this.calendarBuilder.assignImageToMonth(from, toAssignment.imageId, toAssignment.imageUrl);
    } else {
      this.calendarBuilder.assignImageToMonth(to, fromAssignment.imageId, fromAssignment.imageUrl);
      this.calendarBuilder.removeAssignment(from);
    }
  }

  get allImages(): JobTask[] {
    return (this.activeModel?.jobs || [])
      .flatMap(job => job.tasks || [])
      .filter(task => task.imageUrl || task.processedImageUrl);
  }

  get isCalendarReady(): boolean {
    return this.assignments.length === 12 && this.duplicateImageIds.length === 0;
  }

  private removeImageFromActiveModel(imageId: number): void {
    if (!this.activeModel) {
      return;
    }

    const updatedJobs = (this.activeModel.jobs || []).map(job => ({
      ...job,
      tasks: (job.tasks || []).filter(task => task.id !== imageId)
    }));

    this.activeModel = { ...this.activeModel, jobs: updatedJobs };
    if (this.userModels.length) {
      this.userModels = this.userModels.map(model => model.id === this.activeModel!.id ? this.activeModel! : model);
    }
  }

  private assignImageToMonth(image: JobTask, month: number): void {
    const imageUrl = this.getImageUrl(image);
    if (!imageUrl) {
      this.snackBar.open('Не вдалося отримати посилання на зображення', 'OK', { duration: 2500 });
      return;
    }

    this.calendarBuilder.assignImageToMonth(month, image.id.toString(), imageUrl);
    this.snackBar.open(`Додано до ${this.getMonthName(month)}`, 'OK', { duration: 2500 });
  }

  private subscribeToAssignments(): void {
    this.calendarBuilder.assignments$
      .pipe(takeUntil(this.destroy$))
      .subscribe((assignments) => {
        this.assignments = assignments;
        this.duplicateImageIds = this.calendarBuilder.getDuplicateImageIds();
      });
  }

  private getImageUrl(task: JobTask): string {
    return task.processedImageUrl || task.imageUrl || '';
  }

  private getMonthName(month: number): string {
    return MONTHS.find((item) => item.value === month)?.label || `Місяць ${month}`;
  }
}
