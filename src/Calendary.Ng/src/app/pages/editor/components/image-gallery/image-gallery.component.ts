import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { JobTask } from '../../../../../models/job-task';
import { Job } from '../../../../../models/job';

@Component({
  selector: 'app-image-gallery',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule, MatTooltipModule],
  templateUrl: './image-gallery.component.html',
  styleUrl: './image-gallery.component.scss'
})
export class ImageGalleryComponent {
  @Input() jobs: Job[] = [];
  @Output() imageSelected = new EventEmitter<JobTask>();
  @Output() imageDeleted = new EventEmitter<JobTask>();

  selectedImage: JobTask | null = null;

  get allImages(): JobTask[] {
    return this.jobs.flatMap(job => job.tasks || []).filter(task => task.imageUrl || task.processedImageUrl);
  }

  selectImage(image: JobTask): void {
    this.selectedImage = image;
    this.imageSelected.emit(image);
  }

  deleteImage(image: JobTask, event: Event): void {
    event.stopPropagation();
    this.imageDeleted.emit(image);
    if (this.selectedImage === image) {
      this.selectedImage = null;
    }
  }

  getImageUrl(task: JobTask): string {
    return task.processedImageUrl || task.imageUrl || '';
  }

  getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
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
}
