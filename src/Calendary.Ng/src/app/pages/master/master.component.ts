import { Component } from '@angular/core';
import { PhotoUploadComponent } from './photo-upload/photo-upload.component';
import { CalendarDatesComponent } from './calendar-dates/calendar-dates.component';
import { CartButtonComponent } from './cart-button/cart-button.component';
import { GenerationResultsComponent } from './generation-results/generation-results.component';
import { GenerationStatusComponent } from './generation-status/generation-status.component';
import { CalendarMonthsComponent } from './calendar-months/calendar-months.component';
import { PromptResultsComponent } from './prompt-results/prompt-results.component';
import { PromptSelectionComponent } from './prompt-selection/prompt-selection.component';
import { FluxModelComponent } from './flux-model/flux-model.component';
import { FluxModel } from '../../../models/flux-model';
import { CommonModule } from '@angular/common';
import { FluxModelService } from '../../../services/flux-model.service';
import { JobTask } from '../../../models/job-task';

@Component({
  selector: 'app-master',
  standalone: true,
  imports: [
    CommonModule,
    FluxModelComponent,
    PhotoUploadComponent,
    CalendarDatesComponent,
    CalendarMonthsComponent,
    CartButtonComponent,
    GenerationResultsComponent,
    GenerationStatusComponent,
    PromptResultsComponent,
    PromptSelectionComponent,
  ],
  templateUrl: './master.component.html',
  styleUrl: './master.component.scss',
})
export class MasterComponent {

  tasks: JobTask[] = [];

  constructor(private fluxModelService: FluxModelService) { }

  fluxModel: FluxModel | null = null;

  onUpdateFluxModel($event: FluxModel) {
    this.fluxModel = $event;
    if (this.fluxModel.jobs && this.fluxModel.jobs.length > 0) {
      this.tasks = this.fluxModel.jobs[this.fluxModel.jobs.length - 1].tasks;
    }
  }

  onUpdateModelSelf() {
    this.fluxModelService.current().subscribe({
      next: (model) => {
        this.fluxModel = model;
        if (this.fluxModel.jobs && this.fluxModel.jobs.length > 0) {
          this.tasks = this.fluxModel.jobs[this.fluxModel.jobs.length - 1].tasks;
        }
        console.log('Поточний FluxModel:', model);
      },
      error: (err) => {
        console.error('Помилка завантаження поточного FluxModel:', err);
      }
    });
  }
  onUpdateTask(task: JobTask) {
    const index = this.tasks.findIndex(t => t.id === task.id);
  
    if (index !== -1) {
      // Якщо завдання з таким id знайдено, оновлюємо його
      this.tasks[index] = task;
      console.log(`Task with ID ${task.id} updated:`, task);
    } else {
      // Якщо завдання з таким id не знайдено, додаємо його
      this.tasks.push(task);
      console.log(`Task with ID ${task.id} added as new:`, task);
    }
  }
}
