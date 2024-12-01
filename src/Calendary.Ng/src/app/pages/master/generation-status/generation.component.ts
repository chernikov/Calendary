import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { FluxModelService } from '../../../../services/flux-model.service';
import { FluxModel } from '../../../../models/flux-model';
import { TrainingService } from '../../../../services/training.service';

@Component({
  selector: 'app-generation',
  standalone: true,
  imports: [CommonModule, MatButtonModule],
  templateUrl: './generation.component.html',
  styleUrl: './generation.component.scss',
})
export class GenerationComponent {
  @Input() fluxModel: FluxModel | null = null;

  constructor(
    private fluxModelService: FluxModelService,
    private trainingService: TrainingService
  ) {}

  generateModel(): void {
    if (!this.fluxModel) {
      alert('FluxModel не знайдено');
      return;
    }

    this.fluxModelService.generate(this.fluxModel.id).subscribe({
      next: (response) => {
        console.log('Model generation triggered successfully', response);
        window.location.reload();
      },
      error: (error) => {
        console.error('Error during model generation', error);
      },
    });
  }

  updateStatus(trainingId: number): void {
    this.trainingService.getStatus(trainingId).subscribe({
      next: (status) => {
        console.log(`Status updated: ${status}`);
        const training = this.fluxModel!.trainings.find(
          (t: any) => t.id === trainingId
        );
        if (training) {
          training.status = status.status; // Оновлюємо статус локально
        }
      },
      error: (err) => console.error('Error updating status', err),
    });
  }

  cancelTraining(trainingId: number): void {
    this.trainingService.cancelTraining(trainingId).subscribe({
      next: () => {
        console.log(`Training #${trainingId} canceled.`);
        const training = this.fluxModel!.trainings.find(
          (t: any) => t.id === trainingId
        );
        if (training) {
          training.status = 'canceled'; // Оновлюємо статус локально
        }
      },
      error: (err) => console.error('Error canceling training', err),
    });
  }

  canGenerateTraining(): boolean {
    const result =
      this.fluxModel &&
      (this.fluxModel.trainings.length == 0 ||
        this.fluxModel.trainings.every(
          (t: any) =>
            t.status === 'succeeded' ||
            t.status === 'canceled' ||
            t.status === 'failed'
        ));
    return result ?? false;
  }
}
