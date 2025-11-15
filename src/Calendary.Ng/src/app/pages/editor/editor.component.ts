import { Component, OnInit } from '@angular/core';
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

@Component({
  selector: 'app-editor',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    ImageGalleryComponent,
    ImageCanvasComponent,
    PropertiesPanelComponent
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
    private snackBar: MatSnackBar
  ) {}

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
    this.snackBar.open('Збереження обробленого зображення буде реалізовано пізніше', 'OK', {
      duration: 3000
    });
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
