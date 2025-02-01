import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminPromptThemeService } from '../../../../../services/admin/prompt-theme.service';
import { PromptTheme } from '../../../../../models/prompt-theme';

@Component({
  selector: 'app-prompt-theme-dialog',
  templateUrl: './prompt-theme-dialog.component.html',
  styleUrls: ['./prompt-theme-dialog.component.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatButtonModule,
     MatDialogModule, MatFormFieldModule, MatInputModule, MatSlideToggleModule]

})
export class PromptThemeDialogComponent {
  promptThemeForm: FormGroup;

  constructor(
    private dialogRef: MatDialogRef<PromptThemeDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: PromptTheme,
    private fb: FormBuilder,
    private adminPromptThemeService: AdminPromptThemeService
  ) {
    this.promptThemeForm = this.fb.group({
      name: [data?.name || '', [Validators.required, Validators.maxLength(50)]],
      isPublished: [data?.isPublished || false, [Validators.required]],
    });
  }

  onSave(): void {
    if (this.promptThemeForm.invalid) return;

    const formData = this.promptThemeForm.value;
    if (this.data?.id) {
      formData.id = this.data.id;
      this.adminPromptThemeService.update(formData).subscribe(() => {
        this.dialogRef.close(true);
      });
    } else {
      this.adminPromptThemeService.create(formData).subscribe(() => {
        this.dialogRef.close(true);
      });
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}