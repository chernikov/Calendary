import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { FormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { AdminCategoryService } from '../../../../../../../services/admin/category.service';
import { Category } from '../../../../../../../models/category';
import { CreateFluxModel } from '../../../../../../../models/flux-model';


@Component({
    standalone: true,
    selector: 'app-create-flux-model-dialog',
    imports: [CommonModule, FormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatSelectModule],
    templateUrl: './create-flux-model-dialog.component.html',
    styleUrls: ['./create-flux-model-dialog.component.scss']
})
export class CreateFluxModelDialogComponent 
{
  model : CreateFluxModel = new CreateFluxModel();
  categories: Category[] = [];

  constructor(
    public dialogRef: MatDialogRef<CreateFluxModelDialogComponent>,
    private adminCategoryService: AdminCategoryService,
    @Inject(MAT_DIALOG_DATA) public dataFromParent: any)
  {
    this.loadCategories();
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


  onSave(): void {
    this.dialogRef.close(this.model);
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
