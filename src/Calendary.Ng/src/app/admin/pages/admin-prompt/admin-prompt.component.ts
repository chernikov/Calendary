import { Component } from '@angular/core';
import { Prompt } from '../../../../models/prompt';
import { AdminPromptService } from '../../../../services/admin-prompt.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-admin-prompt',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule],
  templateUrl: './admin-prompt.component.html',
  styleUrl: './admin-prompt.component.scss'
})
export class AdminPromptComponent {
  displayedColumns: string[] = ['id', 'theme', 'gender', 'text', 'actions'];
  dataSource = new MatTableDataSource<Prompt>();

  themeId: number | null = null;

  constructor(private adminPromptService: AdminPromptService, private router: Router) {}

  ngOnInit(): void {
    this.loadPrompts();
  }

  loadPrompts(): void {
    this.adminPromptService.getAll(this.themeId).subscribe(
      (data) => {
        this.dataSource.data = data;
      },
      (error) => {
        console.error('Failed to load prompts:', error);
      }
    );
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
}
