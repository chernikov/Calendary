import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { FormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { AdminFluxModelService } from '../../../../../services/admin-flux-model.service';
import { AdminTestPromptService } from '../../../../../services/admin-test-prompot.service';
import { AdminFluxModel } from '../../../../../models/admin-flux-model';
import { AgeGenderDisplayPipe } from '../../../../pipes/age-gender-display';
import { CreateTestPrompt } from '../../../../../models/create-test-prompt';
@Component({
  selector: 'app-test-prompt-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule, MatDialogModule, MatInputModule, MatFormFieldModule, MatSelectModule, MatButtonModule, MatProgressSpinnerModule, AgeGenderDisplayPipe],
  templateUrl: './test-prompt-dialog.component.html',
  styleUrl: './test-prompt-dialog.component.scss'
})

export class TestPromptDialogComponent implements OnInit {
  models: AdminFluxModel[] = []; // Список моделей, завантажений з API
  selectedModel: AdminFluxModel | null = null; // Обрана модель
  promptText : string = '';

  constructor(
    private dialogRef: MatDialogRef<TestPromptDialogComponent>,
    private fluxModelService: AdminFluxModelService,
    private testPromptService: AdminTestPromptService,
    @Inject(MAT_DIALOG_DATA) public data: { 
      promptId: number, 
      ageGender: number
      promptText: string, 
     }
  ) {
    this.promptText = data.promptText;
  }

  loading: boolean = false;
  imageUrl: string | null = null;

  ngOnInit(): void {
    this.loadModels();
  }

  loadModels(): void {
    this.fluxModelService.getByAgeGender(this.data.ageGender).subscribe(
      (models) => {
        this.models = models;
      },
      (error) => {
        console.error('Failed to load models:', error);
      }
    );
  }

  runTest(): void {
    if (!this.selectedModel) return;

    this.loading = true;
    const newTestPrompt = new CreateTestPrompt() ;
    newTestPrompt.promptId = this.data.promptId;
    newTestPrompt.fluxModelId = this.selectedModel.id;
    newTestPrompt.text = this.promptText;

    // Створити TestPrompt
    this.testPromptService.create(newTestPrompt).subscribe(
      (testPrompt) => {
        // Після створення запускати
        this.testPromptService.runTestPrompt(testPrompt.id).subscribe(
          (result) => {
            this.loading = false;
            this.imageUrl = result.imageUrl!; // Відображаємо результат
          },
          (error) => {
            this.loading = false;
            console.error('Помилка при запуску тесту:', error);
          }
        );
      },
      (error) => {
        this.loading = false;
        console.error('Помилка при створенні TestPrompt:', error);
      }
    );
  }

  cancel(): void {
    this.dialogRef.close();
  }

  applyPrompt(): void {
    // Передаємо змінений текст промпту назад у компонент
    this.dialogRef.close(this.promptText);
  }
}



