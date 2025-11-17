import { Component } from '@angular/core';
import { AdminPromptThemeService } from '../../../../services/admin/prompt-theme.service';
import { MatDialog } from '@angular/material/dialog';
import { PromptThemeDialogComponent } from './prompt-theme-dialog/prompt-theme-dialog.component';
import {  MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { Prompt } from '../../../../models/prompt';
import { PromptTheme } from '../../../../models/prompt-theme';

@Component({
    standalone: true,
    selector: 'app-admin-prompt-theme',
    imports: [CommonModule, FormsModule, MatIconModule, MatTableModule, MatButtonModule],
    templateUrl: './admin-prompt-theme.component.html',
    styleUrl: './admin-prompt-theme.component.scss'
})
export class AdminPromptThemeComponent {
  displayedColumns: string[] = ['id', 'name', 'isPublished', 'actions'];
  dataSource: any[] = [];

  constructor(private adminPromptThemeService: AdminPromptThemeService, private dialog: MatDialog) {}

  ngOnInit(): void {
    this.loadPromptThemes();
  }

  loadPromptThemes(): void {
    this.adminPromptThemeService.getAll().subscribe((data) => {
      this.dataSource = data;
    });
  }

  openDialog(theme: any = null): void {
    if (!theme) {
      theme = new PromptTheme();
    }
    const dialogRef = this.dialog.open(PromptThemeDialogComponent, {
      width: '400px',
      data: theme,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.loadPromptThemes();
      }
    });
  }

  onDelete(id: number): void {
    this.adminPromptThemeService.delete(id).subscribe(() => {
      this.loadPromptThemes();
    });
  }
}
