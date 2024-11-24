import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
} from '@angular/core';
import { FluxModel } from '../../../../models/flux-model';
import { Job } from '../../../../models/job';
import { CommonModule } from '@angular/common';
import { PromptTheme } from '../../../../models/prompt-theme';
import { PromptThemeService } from '../../../../services/prompt-theme.service';
import { FormsModule } from '@angular/forms';
import { JobService } from '../../../../services/job.service';
import { JobTaskService } from '../../../../services/job-task.service';
import { JobTask } from '../../../../models/job-task';

@Component({
  selector: 'app-prompt-selection',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './prompt-selection.component.html',
  styleUrl: './prompt-selection.component.scss',
})
export class PromptSelectionComponent implements OnChanges, OnInit {
  @Input() fluxModel: FluxModel | null = null;
  @Output() onUpdate: EventEmitter<void> = new EventEmitter<void>();
  @Output() onUpdateTask: EventEmitter<JobTask> = new EventEmitter<JobTask>();
  job?: Job;
  lastJob?: Job;

  promptThemes: PromptTheme[] = [];
  selectedTheme: PromptTheme | null = null;
  errorMessage: string | null = null;

  constructor(
    private promptThemeService: PromptThemeService,
    private jobService: JobService,
    private jobTaskService: JobTaskService
  ) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['fluxModel'] && changes['fluxModel'].currentValue) {
      this.fluxModel = changes['fluxModel'].currentValue;
      if (this.fluxModel && this.fluxModel.jobs.length > 0) {
        this.job = this.fluxModel.jobs[0];
        this.lastJob = this.fluxModel.jobs[this.fluxModel.jobs.length - 1];
      }
    }
  }

  ngOnInit(): void {
    this.getPromptThemes();
  }

  getPromptThemes(): void {
    this.promptThemeService.getAll().subscribe(
      (themes) => {
        this.promptThemes = themes;
        this.errorMessage = null; // Очищення повідомлення про помилку
      },
      (error) => {
        console.error('Error fetching prompt themes:', error);
        this.errorMessage = 'Failed to load prompt themes.';
      }
    );
  }

  /**
   * Виклик сервісу для створення Job
   */
  generateJob(): void {
    if (this.selectedTheme && this.fluxModel) {
      this.jobService
        .createJob(this.fluxModel.id, this.selectedTheme.id)
        .subscribe(
          (job) => {
            console.log('Generated Job:', job);
            this.lastJob = job; // Зберігаємо створений Job
            this.errorMessage = null; // Очищення помилок
            this.onUpdate.emit(); // Відправляємо подію onUpdate
          },
          (error) => {
            console.error('Error generating Job:', error);
            this.errorMessage = 'Failed to generate Job. Please try again.';
          }
        );
    }
  }

  async processJob() {
    if (!this.lastJob) {
      return;
    }

    this.errorMessage = null;
    for (const task of this.lastJob.tasks) {
      try {
        const result = await this.jobTaskService.run(task.id).toPromise();
        console.log(`Task ${task.id} completed:`, result);
        this.onUpdateTask.emit(result);
      } catch (taskError) {
        console.error(`Error executing task ${task.id}:`, taskError);
        this.errorMessage = `Failed to execute task ${task.id}. Please try again.`;
        break; // Зупиняємо виконання наступних завдань у разі помилки
      }
    }
  }
}
