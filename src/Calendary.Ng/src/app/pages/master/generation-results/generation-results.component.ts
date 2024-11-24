import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { Job } from '../../../../models/job';
import { JobService } from '../../../../services/job.service';
import { CommonModule } from '@angular/common';
import { FluxModel } from '../../../../models/flux-model';

@Component({
  selector: 'app-generation-results',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './generation-results.component.html',
  styleUrl: './generation-results.component.scss'
})
export class GenerationResultsComponent implements OnChanges {

  @Input() fluxModel: FluxModel | null = null;

  @Output() onUpdate: EventEmitter<void> = new EventEmitter<void>();
  
  job?: Job; // Збереження створеного Job
  errorMessage: string | null = null;

  constructor(private jobService: JobService) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['fluxModel'] && changes['fluxModel'].currentValue) {
      this.fluxModel = changes['fluxModel'].currentValue;
      if (this.fluxModel && this.fluxModel.jobs.length > 0) 
      {
        this.job = this.fluxModel.jobs[0];
      }
    }
  }


  /**
   * Створення Job
   */
  createJob(): void {
    if (!this.fluxModel) {
      this.errorMessage = "Треба flux model";
      return;
    }
    this.errorMessage = null; // Скидаємо помилку
    this.jobService.createDefaultJob(this.fluxModel.id).subscribe(
      (job) => {
        console.log('Job created:', job);
        this.job = job;
        this.onUpdate.emit();
      },
      (error) => {
        console.error('Error creating Job:', error);
        this.errorMessage = 'Помилка при створенні Job.';
      }
    );
  }

  /**
   * Запуск Job
   */
  runJob(): void {
    if (this.job) {
      this.errorMessage = null; // Скидаємо помилку
      this.jobService.runJob(this.job.id).subscribe(
        (response) => {
          console.log('Job run successfully:', response);
          alert('Job виконано успішно.');
          this.onUpdate.emit();
        },
        (error) => {
          console.error('Error running Job:', error);
          this.errorMessage = 'Помилка при виконанні Job.';
        }
      );
    } else {
      this.errorMessage = 'Спершу потрібно створити Job.';
    }
  }
}