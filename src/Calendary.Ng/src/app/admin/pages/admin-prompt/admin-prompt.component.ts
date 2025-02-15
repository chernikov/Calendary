import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatOptionModule } from '@angular/material/core';
import { FormsModule } from '@angular/forms';
import { MatSelectModule } from '@angular/material/select';
import { Prompt } from '../../../../models/prompt';
import { AdminPromptService } from '../../../../services/admin/prompt.service';
import { SynthesisDialogComponent } from './synthesis-dialog/synthesis-dialog.component';
import { AdminPromptThemeService } from '../../../../services/admin/prompt-theme.service';
import { PromptTheme } from '../../../../models/prompt-theme';
import { AdminCategoryService } from '../../../../services/admin/category.service';
import { Category } from '../../../../models/category';
@Component({
  selector: 'app-admin-prompt',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatTableModule,
    MatButtonModule,
    MatFormFieldModule,
    MatSelectModule,
    MatOptionModule],
  templateUrl: './admin-prompt.component.html',
  styleUrl: './admin-prompt.component.scss',
})
export class AdminPromptComponent {
  displayedColumns: string[] = ['id', 'theme', 'age-gender', 'text', 'actions'];
  dataSource = new MatTableDataSource<Prompt>();
  themes: PromptTheme[] = []; // Завантажені теми
  categories: Category[] = [];

  filterThemeId : number | null = null;
  filterCategoryId : number | null = null;

  constructor(
    private adminPromptService: AdminPromptService,
    private adminPromptThemeService: AdminPromptThemeService,
    private adminCategoryService: AdminCategoryService,
    private dialog: MatDialog,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadThemes();
    this.loadPrompts();
    this.loadCategories();

  }

  loadThemes(): void {
    this.adminPromptThemeService.getAll().subscribe(
      (themes) => {
        this.themes = themes;
      },
      (error) => {
        console.error('Помилка завантаження тем:', error);
      }
    );
  }

  loadPrompts(): void 
  {
    const themeId = this.filterThemeId || null;
    const categoryId = this.filterCategoryId || null;
    this.adminPromptService.getAll(themeId, categoryId).subscribe(
      (data) => {
        this.dataSource.data = data;
      },
      (error) => {
        console.error('Failed to load prompts:', error);
      }
    );
  }

  loadCategories(): void 
  {
    this.adminCategoryService.getAll().subscribe(
      (data) => {
        this.categories = data;
      },
      (error) => {
        console.error('Failed to load categories:', error);
      }
    );
  }

  applyFilters() {
    this.loadPrompts();
  }


  createNew(): void {
    this.router.navigate(['/admin/prompts/create']);
  }

  editPrompt(id: number): void {
    this.router.navigate(['/admin/prompts/edit', id]);
  }

  deletePrompt(id: number): void {
    if (confirm('Are you sure you want to delete this prompt?')) {
      this.adminPromptService.delete(id).subscribe(
        () => {
          this.loadPrompts(); // Reload the list after deletion
        },
        (error) => {
          console.error('Failed to delete prompt:', error);
        }
      );
    }
  }

  openTestDialog(prompt: Prompt): void {
    const dialogRef = this.dialog.open(SynthesisDialogComponent, {
      panelClass: 'custom-dialog-container', 
      width: '800px', // Задаємо ширину
      height: '800px', // Задаємо висоту
      data: {
        prompt: prompt
      },
    });

    dialogRef.afterClosed().subscribe((result) => {
        this.loadPrompts();
    });
  }

  goToHistory(promptId: number): void {
    this.router.navigate([`/admin/prompts/${promptId}/history`]);
  }
}
