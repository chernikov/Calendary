import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatInputModule } from '@angular/material/input';
import { Category } from '../../../../../models/category';
import { AdminCategoryService } from '../../../../../services/admin/category.service';

@Component({
  selector: 'app-category-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatButtonModule,MatInputModule, MatCheckboxModule],
  templateUrl: './category-dialog.component.html',
  styleUrl: './category-dialog.component.scss'
})
export class CategoryDialogComponent {
  categoryForm: FormGroup;

  constructor(
    private dialogRef: MatDialogRef<CategoryDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Category,
    private fb: FormBuilder,
    private adminCategoryService: AdminCategoryService
  ) {
    this.categoryForm = this.fb.group({
      name: [data?.name || '', [Validators.required, Validators.maxLength(50)]],
      isAlive: [data?.isAlive || false],
    });
  }

  onSave(): void {
    if (this.categoryForm.invalid) return;

    const formData = this.categoryForm.value;

    if (this.data?.id) {
      formData.id = this.data.id;
      this.adminCategoryService.update(formData).subscribe(() => {
        this.dialogRef.close(true);
      });
    } else {
      this.adminCategoryService.create(formData).subscribe(() => {
        this.dialogRef.close(true);
      });
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}