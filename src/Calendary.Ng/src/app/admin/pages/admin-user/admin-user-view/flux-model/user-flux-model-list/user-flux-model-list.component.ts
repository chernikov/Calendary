import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { finalize } from 'rxjs/operators';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { FluxModel } from '../../../../../../../models/flux-model';
import { UserFluxModelService } from '../../../../../../../services/admin/user-flux-model.service';
import { Training } from '../../../../../../../models/training';
import { TrainingService } from '../../../../../../../services/admin/training.service';
import { UserPhotoGalleryComponent } from '../user-photo-gallery/user-photo-gallery.component';




@Component({
  selector: 'app-user-flux-model-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, MatTableModule, MatButtonModule, MatIconModule],
  templateUrl: './user-flux-model-list.component.html',
  styleUrls: ['./user-flux-model-list.component.scss']
})
export class UserFluxModelListComponent implements OnInit {
  @Input() userId!: number;
  fluxModels: FluxModel[] = [];
  displayedColumns: string[] = ['id', 'name', 'status', 'trainings', 'photos', 'createdAt', 'actions'];

  // Для редагування імені
  editingFluxModelId: number | null = null;
  tempName: string = '';

  constructor(private fluxModelService: UserFluxModelService,    
     private trainingService: TrainingService,
     private dialog: MatDialog) {}

  ngOnInit(): void {
    this.loadFluxModels();
  }

  loadFluxModels(): void {
    this.fluxModelService.getUserFluxModels(this.userId).subscribe({
      next: (models) => this.fluxModels = models,
      error: (err) => console.error('Помилка завантаження flux моделей', err)
    });
  }

  getTrainingInfo(model: FluxModel): Training | null{
    if (model.trainings && model.trainings.length > 0) {
      const training = model.trainings[0];
      return training;
    }
    return null;
  }

  deleteFluxModel(fluxModelId: number): void {
    if (!confirm('Ви впевнені, що хочете видалити flux модель?')) {
      return;
    }
    this.fluxModelService.delete(this.userId, fluxModelId)
      .pipe(finalize(() => this.loadFluxModels()))
      .subscribe({
        next: () => console.log(`Flux модель ${fluxModelId} видалено`),
        error: (err) => console.error('Помилка видалення flux моделі', err)
      });
  }

  deleteTraining(trainingId: number): void {
    if (!confirm('Ви впевнені, що хочете видалити тренування?')) {
      return;
    }
    this.trainingService.softDelete(trainingId)
      .pipe(finalize(() => this.loadFluxModels()))
      .subscribe({
        next: () => console.log(`Тренування ${trainingId} видалено`),
        error: (err) => console.error('Помилка видалення тренування', err)
      });
  }

  openPhotoGallery(fluxModelId: number): void {
    this.dialog.open(UserPhotoGalleryComponent, {
      width: '80%',
      data: { userId: this.userId, fluxModelId: fluxModelId }
    });
  }

  // Редагування імені
  onNameClick(model: FluxModel): void {
    this.editingFluxModelId = model.id;
    this.tempName = model.name;
  }

  saveName(model: FluxModel): void {
    const updatedModel: FluxModel = { ...model, name: this.tempName };
    this.fluxModelService.changeName(this.userId, updatedModel)
      .pipe(finalize(() => this.loadFluxModels()))
      .subscribe({
        next: () => {
          console.log(`Ім'я flux моделі ${model.id} змінено`);
          this.cancelEditing();
        },
        error: (err) => console.error('Помилка зміни імені flux моделі', err)
      });
  }

  cancelEditing(): void {
    this.editingFluxModelId = null;
    this.tempName = '';
  }
}