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
import { AdminFluxModelService } from '../../../../../services/admin/flux-model.service';
import { AdminSynthesisService } from '../../../../../services/admin/synthesis.service';
import { AdminFluxModel } from '../../../../../models/admin-flux-model';
import { CreateSynthesis } from '../../../../../models/create-synthesis';
import { AdminPromptService } from '../../../../../services/admin/prompt.service';
import { Prompt } from '../../../../../models/prompt';
import { PromptSeed } from '../../../../../models/promt-seed';
@Component({
  selector: 'app-synthesis-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule,
    MatDialogModule, MatInputModule, MatFormFieldModule, MatSelectModule, 
    MatButtonModule, MatProgressSpinnerModule, MatDividerModule],
  templateUrl: './synthesis-dialog.component.html',
  styleUrl: './synthesis-dialog.component.scss'
})

export class SynthesisDialogComponent implements OnInit {
  models: AdminFluxModel[] = []; // Список моделей, завантажений з API
  selectedModel: AdminFluxModel | null = null; // Обрана модель
  promptText : string = '';
  seeds: number[] = [];
  seed : number | null = null;
  outputSeed : number | null = null;


  constructor(
    private dialogRef: MatDialogRef<SynthesisDialogComponent>,
    private fluxModelService: AdminFluxModelService,
    private synthesisService: AdminSynthesisService,
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
    this.fluxModelService.getByCategoryId(this.data.prompt.categoryId).subscribe(
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
    const newSynthesis = new CreateSynthesis() ;
    newSynthesis.promptId = this.data.prompt.id;
    newSynthesis.fluxModelId = this.selectedModel.id;
    newSynthesis.text = this.promptText;
    newSynthesis.seed = this.seed;

    // Створити Synthesis
    this.synthesisService.create(newSynthesis).subscribe(
      (synthesis) => {
        // Після створення запускати
        this.synthesisService.runSynthesis(synthesis.id).subscribe(
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
        console.error('Помилка при створенні Synthesis:', error);
      }
    );
  }

  cancel(): void {
    this.dialogRef.close();
  }

  applyPrompt(): void {
    this.saveUpdatedPrompt(this.promptText, null);
    this.dialogRef.close(this.promptText);
  }


  
  applyPromptWithSeed(): void {
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



