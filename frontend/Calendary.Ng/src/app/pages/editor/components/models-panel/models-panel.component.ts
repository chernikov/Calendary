import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { FluxModel } from '../../../../../models/flux-model';
import { FluxModelService } from '../../../../../services/flux-model.service';
import { Router } from '@angular/router';
import { take } from 'rxjs';

@Component({
  standalone: true,
  selector: 'app-models-panel',
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatMenuModule,
    MatSnackBarModule
  ],
  templateUrl: './models-panel.component.html',
  styleUrl: './models-panel.component.scss'
})
export class ModelsPanelComponent implements OnInit {
  @Input() models: FluxModel[] = [];
  @Input() activeModel: FluxModel | null = null;
  @Input() isLoading: boolean = false;
  @Input() loadError: string = '';

  @Output() modelSelected = new EventEmitter<FluxModel>();
  @Output() modelDeleted = new EventEmitter<number>();
  @Output() modelRenamed = new EventEmitter<FluxModel>();
  @Output() modelSetActive = new EventEmitter<FluxModel>();
  @Output() modelsRefresh = new EventEmitter<void>();

  renamingModelId: number | null = null;
  newModelName: string = '';

  constructor(
    private fluxModelService: FluxModelService,
    private snackBar: MatSnackBar,
    private router: Router
  ) {}

  ngOnInit(): void {}

  selectModel(model: FluxModel): void {
    if (this.renamingModelId !== model.id) {
      this.modelSelected.emit(model);
    }
  }

  setActiveModel(model: FluxModel, event: Event): void {
    event.stopPropagation();
    this.modelSetActive.emit(model);
  }

  startRename(model: FluxModel, event: Event): void {
    event.stopPropagation();
    this.renamingModelId = model.id;
    this.newModelName = model.name || '';
  }

  cancelRename(): void {
    this.renamingModelId = null;
    this.newModelName = '';
  }

  saveRename(model: FluxModel, event: Event): void {
    event.stopPropagation();

    if (!this.newModelName.trim()) {
      this.snackBar.open('Назва моделі не може бути порожньою', 'OK', {
        duration: 3000
      });
      return;
    }

    this.fluxModelService.updateName(model.id, this.newModelName.trim())
      .pipe(take(1))
      .subscribe({
        next: (updatedModel) => {
          model.name = updatedModel.name;
          this.modelRenamed.emit(updatedModel);
          this.snackBar.open('Модель перейменовано', 'OK', {
            duration: 2000
          });
          this.renamingModelId = null;
          this.newModelName = '';
        },
        error: () => {
          this.snackBar.open('Не вдалося перейменувати модель', 'OK', {
            duration: 3000
          });
        }
      });
  }

  deleteModel(model: FluxModel, event: Event): void {
    event.stopPropagation();

    const confirmed = confirm(`Видалити модель "${model.name || 'FluxModel #' + model.id}"? Ця дія незворотна.`);
    if (!confirmed) {
      return;
    }

    this.fluxModelService.delete(model.id)
      .pipe(take(1))
      .subscribe({
        next: () => {
          this.modelDeleted.emit(model.id);
          this.snackBar.open('Модель видалено', 'OK', {
            duration: 2000
          });
        },
        error: () => {
          this.snackBar.open('Не вдалося видалити модель', 'OK', {
            duration: 3000
          });
        }
      });
  }

  createNewModel(): void {
    this.router.navigate(['/master']);
  }

  getModelName(model: FluxModel): string {
    return model.name || `FluxModel #${model.id}`;
  }

  getStatusIcon(status: string): string {
    switch (status?.toLowerCase()) {
      case 'ready':
      case 'completed':
        return 'check_circle';
      case 'training':
      case 'inprocess':
        return 'hourglass_empty';
      case 'failed':
        return 'error';
      case 'uploading':
        return 'upload';
      default:
        return 'fiber_manual_record';
    }
  }

  getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'ready':
      case 'completed':
        return 'status-ready';
      case 'training':
      case 'inprocess':
        return 'status-training';
      case 'failed':
        return 'status-failed';
      case 'uploading':
        return 'status-uploading';
      default:
        return 'status-unknown';
    }
  }

  formatDate(date: Date | string): string {
    if (!date) return '';
    const d = new Date(date);
    return d.toLocaleDateString('uk-UA', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }
}
