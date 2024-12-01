import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatError, MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ActivatedRoute, Router } from '@angular/router';
import { MatOptionModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { Prompt } from '../../../../../models/prompt';
import { AdminPromptService } from '../../../../../services/admin-prompt.service';
import { AdminPromptThemeService } from '../../../../../services/admin-prompt-theme.service';
import { PromptTheme } from '../../../../../models/prompt-theme';
import { PromptSeed } from '../../../../../models/promt-seed';
import { Category } from '../../../../../models/category';
import { AdminCategoryService } from '../../../../../services/admin-category.service';

@Component({
  selector: 'app-edit-prompt',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule,
     MatFormFieldModule, MatInputModule, MatButtonModule, MatOptionModule, MatSelectModule,
     MatError, MatIconModule
    ],
  templateUrl: './edit-prompt.component.html',
  styleUrl: './edit-prompt.component.scss'
})
export class EditPromptComponent implements OnInit {
  promptForm!: FormGroup;
  promptId!: number;
  isEditMode = false;
  themes: PromptTheme[] = [];
  categories : Category[] = [];
  isTextChanged: boolean = false; // Прапорець для відображення попередження
  newSeed: number | null = null;
  assignedSeeds: number[] = []; // Список призначених Seed

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private adminPromptService: AdminPromptService,
    private adminPromptThemeService: AdminPromptThemeService,
    private adminCategoryService : AdminCategoryService
  ) {}

  ngOnInit(): void {
   
    this.promptId = +this.route.snapshot.paramMap.get('id')!;
    this.isEditMode = !!this.promptId;

    this.initializeForm();

    if (this.isEditMode) {
      this.loadPrompt();
    }
    this.loadThemes();
    this.loadCategories();
  }

  initializeForm(): void {
    this.promptForm = this.fb.group({
      id: [null],
      themeId: [null, Validators.required],
      categoryId: [0, Validators.required],
      text: ['', [Validators.required]],
      newSeed: [null] 
    });
  }

  loadPrompt(): void {
    this.adminPromptService.getById(this.promptId).subscribe(
      (prompt) => {
        this.promptForm.patchValue(prompt);
        this.promptForm.get('text')?.valueChanges.subscribe(() => {
          this.isTextChanged = true;
        });
        this.assignedSeeds = prompt.seeds.map((p) => p.seed);
      },
      (error) => {
        console.error('Failed to load prompt:', error);
      }
    );
  }

  assignSeed(): void { 
    const newSeed = this.promptForm.get('newSeed')?.value; // Отримуємо значення з форми
    if (newSeed !== null && newSeed !== undefined) 
    {
      const promptSeed = new PromptSeed();
      promptSeed.seed = newSeed;
      promptSeed.promptId = this.promptId;
      this.adminPromptService.assignSeed(promptSeed).subscribe(() => 
        {
          this.assignedSeeds.push(newSeed);
          this.newSeed = null; // Очищуємо поле після призначення
        });
    }
  }

  disassignSeed(seed: number): void {
    const promptSeed = new PromptSeed();
    promptSeed.seed = seed;
    promptSeed.promptId = this.promptId;
    this.adminPromptService.deassignSeed(promptSeed).subscribe(() => {
      this.assignedSeeds = this.assignedSeeds.filter((s) => s !== seed);
    });
  }

  // Завантажуємо теми через promptThemeService
  loadThemes(): void {
    this.adminPromptThemeService.getAll().subscribe(
      (themes) => {
        this.themes = themes;
      },
      error => {
        console.error('Failed to load prompt themes:', error);
      }
    );
  }

  // Завантажуємо теми через promptThemeService
  loadCategories(): void {
    this.adminCategoryService.getAll().subscribe(
      (categories) => {
        this.categories = categories;
      },
      error => {
        console.error('Failed to load categories:', error);
      }
    );
  }


  onSubmit(): void {
    if (this.promptForm.invalid) {
      return;
    }

    const prompt: Prompt = this.promptForm.value;

    if (this.isEditMode) {
      this.adminPromptService.update(prompt).subscribe(
        () => {
          this.router.navigate(['/admin/prompts']);
        },
        (error) => {
          console.error('Failed to update prompt:', error);
        }
      );
    } else {
      this.adminPromptService.create(prompt).subscribe(
        () => {
          this.router.navigate(['/admin/prompts']);
        },
        (error) => {
          console.error('Failed to create prompt:', error);
        }
      );
    }
  }

  onCancel(): void {
    this.router.navigate(['/admin/prompts']);
  }
}
