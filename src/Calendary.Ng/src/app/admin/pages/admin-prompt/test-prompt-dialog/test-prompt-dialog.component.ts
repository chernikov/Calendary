import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { FormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatDividerModule } from '@angular/material/divider';
import { AdminFluxModelService } from '../../../../../services/admin-flux-model.service';
import { AdminTestPromptService } from '../../../../../services/admin-test-prompot.service';
import { AdminFluxModel } from '../../../../../models/admin-flux-model';
import { AgeGenderDisplayPipe } from '../../../../pipes/age-gender-display';
import { CreateTestPrompt } from '../../../../../models/create-test-prompt';
import { AdminPromptService } from '../../../../../services/admin-prompt.service';
import { Prompt } from '../../../../../models/prompt';
import { PromptSeed } from '../../../../../models/promt-seed';
@Component({
  selector: 'app-test-prompt-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule,
    MatDialogModule, MatInputModule, MatFormFieldModule, MatSelectModule, 
    MatButtonModule, MatProgressSpinnerModule, MatDividerModule,
    AgeGenderDisplayPipe],
  templateUrl: './test-prompt-dialog.component.html',
  styleUrl: './test-prompt-dialog.component.scss'
})

export class TestPromptDialogComponent implements OnInit {
  models: AdminFluxModel[] = []; // Список моделей, завантажений з API
  selectedModel: AdminFluxModel | null = null; // Обрана модель
  promptText : string = '';
  seeds: number[] = [];
  seed : number | null = null;
  outputSeed : number | null = null;


  constructor(
    private dialogRef: MatDialogRef<TestPromptDialogComponent>,
    private fluxModelService: AdminFluxModelService,
    private testPromptService: AdminTestPromptService,
    private promptService: AdminPromptService,
    @Inject(MAT_DIALOG_DATA) public data: { 
      prompt: Prompt
     }
  ) {
    this.initValues(data);
  }

  loading: boolean = false;
  imageUrl: string | null = null;

  ngOnInit(): void {
    this.loadModels();
  }

  loadModels(): void {
    this.fluxModelService.getByAgeGender(this.data.prompt.ageGender).subscribe(
      (models) => {
        this.models = models;
      },
      (error) => {
        console.error('Failed to load models:', error);
      }
    );
  }

  initValues(data : { prompt: Prompt }) : void {
    this.promptText = data.prompt.text;
    this.seeds = data.prompt.seeds.map(p => p.seed);
  }

  runTest(): void {
    if (!this.selectedModel) return;

    this.loading = true;
    const newTestPrompt = new CreateTestPrompt() ;
    newTestPrompt.promptId = this.data.prompt.id;
    newTestPrompt.fluxModelId = this.selectedModel.id;
    newTestPrompt.text = this.promptText;
    newTestPrompt.seed = this.seed;

    // Створити TestPrompt
    this.testPromptService.create(newTestPrompt).subscribe(
      (testPrompt) => {
        // Після створення запускати
        this.testPromptService.runTestPrompt(testPrompt.id).subscribe(
          (result) => {
            this.loading = false;
            this.imageUrl = result.imageUrl!; // Відображаємо результат
            this.outputSeed = result.outputSeed;
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
    debugger;
    this.saveUpdatedPrompt(this.promptText, null);
    this.dialogRef.close(this.promptText);
  }


  
  applyPromptWithSeed(): void {
    debugger;
    this.saveUpdatedPrompt(this.promptText, this.outputSeed ?? this.seed);
    this.dialogRef.close(this.promptText);
  }

  saveUpdatedPrompt(updatedText: string, seed : number | null): void {
    const newPrompt = { ...this.data.prompt };
    newPrompt.text = updatedText;

    this.promptService.update(newPrompt).subscribe(
      () => {
        console.log('Промпт успішно збережений');
        if (seed) {
          this.saveSeed(seed);
          this.loadPrompt();
        }
      },
      (error) => {
        console.error('Помилка при збереженні промпту:', error);
      }
    );
  }

  saveSeed(seed : number) : void
  {
    const newPromptSeed = new PromptSeed();
          newPromptSeed.promptId = this.data.prompt.id;
          newPromptSeed.seed = seed;
          this.promptService.assignSeed(newPromptSeed).subscribe(
            () => {
              console.log('Seed успішно збережений');
            },
            (error) => {
              console.error('Помилка при збереженні seed:', error);
            }
    );
  }

  loadPrompt() {
    this.promptService.getById(this.data.prompt.id).subscribe(
      (prompt) => {
        this.data.prompt = prompt;
        this.initValues(this.data);
      },
      (error) => {
        console.error('Помилка завантаження промпту:', error);
      }
    );
  }
}



