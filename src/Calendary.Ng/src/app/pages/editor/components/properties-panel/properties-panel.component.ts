import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { JobTask } from '../../../../../models/job-task';

@Component({
    selector: 'app-properties-panel',
    imports: [CommonModule, MatIconModule, MatDividerModule],
    templateUrl: './properties-panel.component.html',
    styleUrl: './properties-panel.component.scss'
})
export class PropertiesPanelComponent {
  @Input() selectedImage: JobTask | null = null;

  get imageUrl(): string {
    if (!this.selectedImage) return '';
    return this.selectedImage.processedImageUrl || this.selectedImage.imageUrl || '';
  }

  get formattedCreatedDate(): string {
    if (!this.selectedImage?.createdAt) return '-';
    return new Date(this.selectedImage.createdAt).toLocaleDateString('uk-UA', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  get formattedCompletedDate(): string {
    if (!this.selectedImage?.completedAt) return '-';
    return new Date(this.selectedImage.completedAt).toLocaleDateString('uk-UA', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  get statusBadgeClass(): string {
    if (!this.selectedImage) return 'status-unknown';
    const status = this.selectedImage.status.toLowerCase();

    switch (status) {
      case 'completed':
      case 'success':
        return 'status-success';
      case 'processing':
      case 'pending':
        return 'status-processing';
      case 'failed':
      case 'error':
        return 'status-error';
      default:
        return 'status-unknown';
    }
  }

  get retryText(): string {
    if (!this.selectedImage) return '-';
    return this.selectedImage.retryCount > 0
      ? `${this.selectedImage.retryCount} спроб`
      : 'Без повторів';
  }
}
