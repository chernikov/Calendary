import { Component, OnInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { FluxModel } from '../../../models/flux-model';
import { FluxModelService } from '../../../services/flux-model.service';
import { JobTask } from '../../../models/job-task';
import { Job } from '../../../models/job';
import { ImageGalleryComponent } from './components/image-gallery/image-gallery.component';
import { ImageCanvasComponent } from './components/image-canvas/image-canvas.component';
import { PropertiesPanelComponent } from './components/properties-panel/properties-panel.component';
import { GenerateModalComponent } from './components/generate-modal/generate-modal.component';
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
        ImageGalleryComponent,
        ImageCanvasComponent,
        PropertiesPanelComponent,
        SidebarComponent,
        HistoryComponent,
        ToolbarComponent
    ],
    templateUrl: './editor.component.html',
    styleUrl: './editor.component.scss'
})
export class EditorComponent implements OnInit {
  isLoading = true;
  loadError = '';
  userModels: FluxModel[] = [];
  activeModel: FluxModel | null = null;
  selectedImage: JobTask | null = null;

  constructor(
    private fluxModelService: FluxModelService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
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
    this.loadUserModels();
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
    // TODO: Implement actual deletion via API
    this.snackBar.open('Видалення зображення буде реалізовано пізніше', 'OK', {
      duration: 3000
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
}
