import { Component, Input, Output, EventEmitter, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSliderModule } from '@angular/material/slider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ImageCropperComponent, ImageCroppedEvent, LoadedImage } from 'ngx-image-cropper';
import { JobTask } from '../../../../../models/job-task';

@Component({
  selector: 'app-image-canvas',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    MatButtonModule,
    MatSliderModule,
    MatTooltipModule,
    ImageCropperComponent
  ],
  templateUrl: './image-canvas.component.html',
  styleUrl: './image-canvas.component.scss'
})
export class ImageCanvasComponent {
  @Input() selectedImage: JobTask | null = null;
  @Output() imageSaved = new EventEmitter<Blob>();

  imageChangedEvent: any = '';
  croppedImage: any = '';
  canvasRotation = 0;
  scale = 1;
  showCropper = false;
  transform: any = {};
  containWithinAspectRatio = false;
  isCropMode = false;

  zoomLevel = 100;
  showGrid = false;
  showRulers = false;

  get imageUrl(): string {
    if (!this.selectedImage) return '';
    return this.selectedImage.processedImageUrl || this.selectedImage.imageUrl || '';
  }

  loadImageFile(event: any): void {
    this.imageChangedEvent = event;
    this.isCropMode = true;
  }

  imageCropped(event: ImageCroppedEvent): void {
    this.croppedImage = event.blob;
  }

  imageLoaded(image: LoadedImage): void {
    this.showCropper = true;
  }

  cropperReady(): void {
    // Cropper ready
  }

  loadImageFailed(): void {
    console.error('Load failed');
  }

  rotateLeft(): void {
    this.canvasRotation--;
    this.flipAfterRotate();
  }

  rotateRight(): void {
    this.canvasRotation++;
    this.flipAfterRotate();
  }

  private flipAfterRotate(): void {
    const flippedH = this.transform.flipH;
    const flippedV = this.transform.flipV;
    this.transform = {
      ...this.transform,
      flipH: flippedV,
      flipV: flippedH
    };
  }

  flipHorizontal(): void {
    this.transform = {
      ...this.transform,
      flipH: !this.transform.flipH
    };
  }

  flipVertical(): void {
    this.transform = {
      ...this.transform,
      flipV: !this.transform.flipV
    };
  }

  resetImage(): void {
    this.scale = 1;
    this.canvasRotation = 0;
    this.transform = {};
  }

  updateZoom(value: number): void {
    this.zoomLevel = value;
    this.scale = value / 100;
  }

  zoomIn(): void {
    if (this.zoomLevel < 200) {
      this.updateZoom(this.zoomLevel + 10);
    }
  }

  zoomOut(): void {
    if (this.zoomLevel > 10) {
      this.updateZoom(this.zoomLevel - 10);
    }
  }

  toggleGrid(): void {
    this.showGrid = !this.showGrid;
  }

  toggleRulers(): void {
    this.showRulers = !this.showRulers;
  }

  toggleCropMode(): void {
    this.isCropMode = !this.isCropMode;
  }

  saveCroppedImage(): void {
    if (this.croppedImage) {
      this.imageSaved.emit(this.croppedImage);
    }
  }

  cancelCrop(): void {
    this.isCropMode = false;
    this.imageChangedEvent = '';
    this.croppedImage = '';
  }
}
