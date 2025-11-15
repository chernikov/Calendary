import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { PromptTheme } from '../../../../../models/prompt-theme';
import { PromptThemeService } from '../../../../../services/prompt-theme.service';
import { JobService } from '../../../../../services/job.service';
import { Job } from '../../../../../models/job';

export interface GenerateModalData {
  fluxModelId: number;
  fluxModelName: string;
}

@Component({
  selector: 'app-generate-modal',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatCheckboxModule
  ],
  templateUrl: './generate-modal.component.html',
  styleUrl: './generate-modal.component.scss'
})
export class GenerateModalComponent implements OnInit {
  generateForm: FormGroup;
  isLoading = false;
  isGenerating = false;
  error = '';
  promptThemes: PromptTheme[] = [];
  useDefaultGeneration = false;

  constructor(
    private fb: FormBuilder,
    private promptThemeService: PromptThemeService,
    private jobService: JobService,
    private dialogRef: MatDialogRef<GenerateModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: GenerateModalData
  ) {
    this.generateForm = this.fb.group({
      promptThemeId: [null, Validators.required],
      useDefault: [false]
    });
  }

  ngOnInit(): void {
    this.loadPromptThemes();
  }

  loadPromptThemes(): void {
    this.isLoading = true;
    this.promptThemeService.getAll().subscribe({
      next: (themes) => {
        this.promptThemes = themes.filter(t => t.isPublished);
        this.isLoading = false;
      },
      error: () => {
        this.error = 'Не вдалося завантажити теми промптів';
        this.isLoading = false;
      }
    });
  }

  onDefaultToggle(checked: boolean): void {
    this.useDefaultGeneration = checked;
    if (checked) {
      this.generateForm.get('promptThemeId')?.clearValidators();
      this.generateForm.get('promptThemeId')?.setValue(null);
    } else {
      this.generateForm.get('promptThemeId')?.setValidators(Validators.required);
    }
    this.generateForm.get('promptThemeId')?.updateValueAndValidity();
  }

  onGenerate(): void {
    if (!this.useDefaultGeneration && this.generateForm.invalid) {
      return;
    }

    this.isGenerating = true;
    this.error = '';

    const generateObservable = this.useDefaultGeneration
      ? this.jobService.createDefaultJob(this.data.fluxModelId)
      : this.jobService.createJob(this.data.fluxModelId, this.generateForm.value.promptThemeId);

    generateObservable.subscribe({
      next: (job: Job) => {
        // Add job to processing queue
        this.jobService.messageJob(job.id).subscribe({
          next: () => {
            this.isGenerating = false;
            this.dialogRef.close(job);
          },
          error: (err) => {
            this.error = 'Не вдалося додати задачу в чергу обробки';
            this.isGenerating = false;
          }
        });
      },
      error: () => {
        this.error = 'Не вдалося створити задачу генерації';
        this.isGenerating = false;
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
