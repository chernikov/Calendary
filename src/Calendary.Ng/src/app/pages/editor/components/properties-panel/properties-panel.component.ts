import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatSliderModule } from '@angular/material/slider';
import { MatSelectModule } from '@angular/material/select';
import { FormsModule } from '@angular/forms';
import { JobTask } from '../../../../../models/job-task';
import { EditorStateService } from '../../services/editor-state.service';
import { Subscription } from 'rxjs';

@Component({
    selector: 'app-properties-panel',
    imports: [CommonModule, MatIconModule, MatDividerModule, MatSliderModule, MatSelectModule, FormsModule],
    templateUrl: './properties-panel.component.html',
    styleUrl: './properties-panel.component.scss'
})
export class PropertiesPanelComponent implements OnInit, OnDestroy {
  @Input() selectedImage: JobTask | null = null;

  imageFormat = 'PNG';
  imageQuality = 95;
  imageWidth = 0;
  imageHeight = 0;

  formats = ['PNG', 'JPG', 'WebP'];

  private subscription: Subscription | null = null;

  constructor(private editorStateService: EditorStateService) {}

  ngOnInit(): void {
    this.subscription = this.editorStateService.state$.subscribe(state => {
      this.imageFormat = state.imageFormat;
      this.imageQuality = state.imageQuality;
      this.imageWidth = state.imageWidth;
      this.imageHeight = state.imageHeight;
    });
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  get imageUrl(): string {
    if (!this.selectedImage) return '';
    return this.selectedImage.processedImageUrl || this.selectedImage.imageUrl || '';
  }

  onFormatChange(format: string): void {
    this.editorStateService.setImageFormat(format);
  }

  onQualityChange(quality: number | null): void {
    if (quality !== null) {
      this.editorStateService.setImageQuality(quality);
    }
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
