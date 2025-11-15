import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { JobTask } from '../../../../../models/job-task';
import { Job } from '../../../../../models/job';
import { MonthAssignment, MONTHS } from '../../models/calendar-assignment.model';

@Component({
    selector: 'app-image-gallery',
    imports: [CommonModule, MatIconModule, MatButtonModule, MatTooltipModule],
    templateUrl: './image-gallery.component.html',
    styleUrl: './image-gallery.component.scss'
})
export class ImageGalleryComponent {
  @Input() jobs: Job[] = [];
  @Input() assignments: MonthAssignment[] = [];
  @Output() imageSelected = new EventEmitter<JobTask>();
  @Output() imageDeleted = new EventEmitter<JobTask>();
  @Output() addToCalendar = new EventEmitter<JobTask>();

  selectedImage: JobTask | null = null;

  get allImages(): JobTask[] {
    return this.jobs.flatMap(job => job.tasks || []).filter(task => task.imageUrl || task.processedImageUrl);
  }

  selectImage(image: JobTask): void {
    this.selectedImage = image;
    this.imageSelected.emit(image);
  }

  requestAddToCalendar(image: JobTask, event: Event): void {
    event.stopPropagation();
    this.addToCalendar.emit(image);
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

  getAssignedMonth(image: JobTask): number | null {
    const assignment = this.assignments.find((a) => a.imageId === image.id.toString());
    return assignment?.month ?? null;
  }

  getMonthLabel(month: number | null): string | null {
    if (!month) {
      return null;
    }
    return MONTHS.find((item) => item.value === month)?.label || null;
  }
}
