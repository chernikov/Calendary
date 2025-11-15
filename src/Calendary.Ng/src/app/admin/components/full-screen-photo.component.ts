import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-full-screen-photo',
    imports: [CommonModule],
    template: `
    <div class="full-screen-container" (click)="onBackdropClick($event)">
      <img
        [src]="data.imageUrl"
        alt="Full Screen Photo"
        class="full-screen-image"
        (click)="$event.stopPropagation()"
      />
    </div>
  `,
    styles: [
        `
      .full-screen-container {
        position: fixed; /* фіксуємо контейнер, щоб він завжди займав екран */
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background-color: rgba(0, 0, 0, 0.9);
        display: flex;
        align-items: center;
        justify-content: center;
        cursor: pointer;
        z-index: 1000; /* переконайтесь, що контейнер розташований над іншим вмістом */
      }

      .full-screen-image {
        max-width: 90%;
        max-height: 90%;
        object-fit: contain;
        box-shadow: 0 0 10px rgba(255, 255, 255, 0.5);
      }
    `,
    ]
})
export class FullScreenPhotoComponent {
  constructor(
    public dialogRef: MatDialogRef<FullScreenPhotoComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { imageUrl: string }
  ) {}

  onBackdropClick(event: MouseEvent): void {
    // Закриваємо діалог при кліку на фон
    this.dialogRef.close();
  }
}
