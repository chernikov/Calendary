import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { CreateSynthesis } from '../../../../../../../models/synthesis';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';

export interface CreateSynthesisDialogData {
  fluxModelId: number;
  trainingId: number;
}

@Component({
    selector: 'app-create-synthesis-dialog',
    imports: [CommonModule, MatDialogModule, MatFormFieldModule, FormsModule, ReactiveFormsModule, MatButtonModule, MatInputModule],
    templateUrl: './create-synthesis-dialog.component.html',
    styleUrls: ['./create-synthesis-dialog.component.scss']
})
export class CreateSynthesisDialogComponent {
  synthesisForm: FormGroup;

  constructor(
    public dialogRef: MatDialogRef<CreateSynthesisDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CreateSynthesisDialogData,
    private fb: FormBuilder
  ) {
    this.synthesisForm = this.fb.group({
      text: ['', Validators.required],
      seed: [null, null]
    });
  }

  onSave(): void {
    if (this.synthesisForm.valid) {
    const result: CreateSynthesis = {
        promptId: null,
        fluxModelId: this.data.fluxModelId,
        trainingId: this.data.trainingId,
        text: this.synthesisForm.value.text,
        seed: this.synthesisForm.value.seed
      };
      this.dialogRef.close(result);
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
