import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { Job } from '../../../../models/job';
import { FluxModel } from '../../../../models/flux-model';
import { PromptTheme } from '../../../../models/prompt-theme';
import { PromptThemeService } from '../../../../services/prompt-theme.service';
import { JobService } from '../../../../services/job.service';
import { JobTaskService } from '../../../../services/job-task.service';
import { JobTask } from '../../../../models/job-task';

@Component({
    standalone: true,
    selector: 'app-prompt-selection',
    imports: [CommonModule, FormsModule, MatCardModule, MatFormFieldModule, MatSelectModule, MatButtonModule],
    templateUrl: './prompt-selection.component.html',
    styleUrl: './prompt-selection.component.scss'
})
export class PromptSelectionComponent implements OnChanges, OnInit {
  @Input() fluxModel: FluxModel | null = null;
  @Output() onUpdate: EventEmitter<void> = new EventEmitter<void>();
  job?: Job;
  lastJob?: Job;

  promptThemes: PromptTheme[] = [];
  selectedTheme: PromptTheme | null = null;
  errorMessage: string | null = null;

  constructor(
    private promptThemeService: PromptThemeService,
    private jobService: JobService,
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
            window.location.reload(); // Перезавантаження сторінки
          },
          (error) => {
            console.error('Error generating Job:', error);
            this.errorMessage = 'Failed to generate Job. Please try again.';
          }
        );
    }
  }
}
