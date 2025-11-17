import { CommonModule, DatePipe } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Router } from '@angular/router';
import { TemplateDetail } from '../../../../../models/template';

export interface TemplatePreviewDialogData {
  template: TemplateDetail;
}

@Component({
  selector: 'app-template-preview-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatDividerModule,
    MatTooltipModule,
    DatePipe,
  ],
  templateUrl: './template-preview-dialog.component.html',
  styleUrl: './template-preview-dialog.component.scss',
})
export class TemplatePreviewDialogComponent {
  activeImageIndex = 0;
  private readonly defaultFeatures = [
    'Готовий до друку макет 300 DPI',
    'Повністю редаговані шари',
    'Сумісність з редактором Calendary',
  ];
  private readonly defaultRecommendations = [
    'Настільні та настінні фотокалендарі',
    'Подарунок для друзів або клієнтів',
    'Студійні та сімейні фотосесії',
  ];

  constructor(
    private dialogRef: MatDialogRef<TemplatePreviewDialogComponent>,
    private router: Router,
    @Inject(MAT_DIALOG_DATA) public data: TemplatePreviewDialogData
  ) {}

  get galleryImages(): string[] {
    if (this.data.template.galleryImages?.length) {
      return this.data.template.galleryImages;
    }
    return [this.data.template.previewImageUrl];
  }

  get activeImage(): string {
    return this.galleryImages[this.activeImageIndex] ?? this.data.template.previewImageUrl;
  }

  get features(): string[] {
    return this.data.template.features?.length ? this.data.template.features : this.defaultFeatures;
  }

  get recommendedUses(): string[] {
    return this.data.template.recommendedUses?.length
      ? this.data.template.recommendedUses
      : this.defaultRecommendations;
  }

  get tags(): string[] {
    return this.data.template.tags ?? [];
  }

  selectImage(index: number): void {
    this.activeImageIndex = index;
  }

  stepGallery(direction: number): void {
    const total = this.galleryImages.length;
    if (total <= 1) {
      return;
    }
    this.activeImageIndex = (this.activeImageIndex + direction + total) % total;
  }

  get hasMultipleImages(): boolean {
    return this.galleryImages.length > 1;
  }

  goToEditor(): void {
    this.dialogRef.close();
    this.router.navigate(['/editor'], { queryParams: { templateId: this.data.template.id } });
  }
}
