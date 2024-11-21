import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ActivatedRoute, Router } from '@angular/router';
import { MatOptionModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { Prompt } from '../../../../../models/prompt';
import { AdminPromptService } from '../../../../../services/admin-prompt.service';
import { AdminPromptThemeService } from '../../../../../services/admin-prompt-theme.service';
import { PromptTheme } from '../../../../../models/prompt-theme';

@Component({
  selector: 'app-edit-prompt',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatOptionModule, MatSelectModule],
  templateUrl: './edit-prompt.component.html',
  styleUrl: './edit-prompt.component.scss'
})
export class EditPromptComponent implements OnInit {
  promptForm!: FormGroup;
  promptId!: number;
  isEditMode = false;
  themes: PromptTheme[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private adminPromptService: AdminPromptService,
    private adminPromptThemeService: AdminPromptThemeService
  ) {}

  ngOnInit(): void {
    this.promptId = +this.route.snapshot.paramMap.get('id')!;
    this.isEditMode = !!this.promptId;

    this.initializeForm();

    if (this.isEditMode) {
      this.loadPrompt();
    }
    this.loadThemes();
  }

  initializeForm(): void {
    this.promptForm = this.fb.group({
      id: [null],
      themeId: [null, Validators.required],
      gender: [0, Validators.required],
      text: ['', [Validators.required]]
    });
  }

  loadPrompt(): void {
    this.adminPromptService.getById(this.promptId).subscribe(
      (prompt) => {
        this.promptForm.patchValue(prompt);
      },
      (error) => {
        console.error('Failed to load prompt:', error);
      }
    );
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
