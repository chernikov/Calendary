import { Component } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Category } from '../../../../models/category';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AdminCategoryService } from '../../../../services/admin/category.service';
import { CategoryDialogComponent } from './category-dialog/category-dialog.component';

@Component({
  selector: 'app-admin-category',
  standalone: true,
  imports: [CommonModule, FormsModule, MatTableModule, MatButtonModule, MatIconModule],
  templateUrl: './admin-category.component.html',
  styleUrls: ['./admin-category.component.scss']
})
export class AdminCategoryComponent {
  displayedColumns: string[] = ['id', 'name', 'isAlive', 'actions'];
  dataSource: Category[] = [];

  constructor(private adminCategoryService: AdminCategoryService, private dialog: MatDialog) {}

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.adminCategoryService.getAll().subscribe((data) => {
      this.dataSource = data;
    });
  }

  openDialog(category: Category = { id: 0, name: '', isAlive: true }): void {
    const dialogRef = this.dialog.open(CategoryDialogComponent, {
      width: '400px',
      data: category,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.loadCategories();
      }
    });
  }

  onDelete(id: number): void {
    this.adminCategoryService.delete(id).subscribe(() => {
      this.loadCategories();
    });
  }
}