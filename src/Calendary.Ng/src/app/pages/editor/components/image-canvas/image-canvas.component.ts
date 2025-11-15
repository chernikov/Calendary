import { Component, Input, Output, EventEmitter, ViewChild, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSliderModule } from '@angular/material/slider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ImageCropperComponent, ImageCroppedEvent, LoadedImage } from 'ngx-image-cropper';
import { JobTask } from '../../../../../models/job-task';
import { EditorStateService } from '../../services/editor-state.service';
import { Subscription } from 'rxjs';

@Component({
    selector: 'app-image-canvas',
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
export class ImageCanvasComponent implements OnInit, OnDestroy {
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

  private subscription: Subscription | null = null;

  constructor(private editorStateService: EditorStateService) {}

  ngOnInit(): void {
    this.subscription = this.editorStateService.state$.subscribe(state => {
      this.zoomLevel = state.zoom;
      this.scale = state.zoom / 100;
      this.showGrid = state.gridEnabled;
      this.showRulers = state.rulersEnabled;

      // Handle tool changes
      if (state.selectedTool === 'crop' && !this.isCropMode && this.selectedImage) {
        // Auto-switch to crop mode when crop tool is selected
        this.isCropMode = true;
      }
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
    this.editorStateService.addAction('rotate', { direction: 'left', angle: -90 }, 'Повернуто ліворуч');
  }

  rotateRight(): void {
    this.canvasRotation++;
    this.flipAfterRotate();
    this.editorStateService.addAction('rotate', { direction: 'right', angle: 90 }, 'Повернуто праворуч');
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
    this.editorStateService.setZoom(value);
  }

  zoomIn(): void {
    if (this.zoomLevel < 200) {
      this.editorStateService.setZoom(this.zoomLevel + 10);
    }
  }

  zoomOut(): void {
    if (this.zoomLevel > 10) {
      this.editorStateService.setZoom(this.zoomLevel - 10);
    }
  }

  toggleGrid(): void {
    this.editorStateService.toggleGrid();
  }

  toggleRulers(): void {
    this.editorStateService.toggleRulers();
  }

  toggleCropMode(): void {
    this.isCropMode = !this.isCropMode;
  }

  saveCroppedImage(): void {
    if (this.croppedImage) {
      this.editorStateService.addAction('crop', { blob: this.croppedImage }, 'Зображення обрізано');
      this.imageSaved.emit(this.croppedImage);
      this.isCropMode = false;
    }
  }

  cancelCrop(): void {
    this.isCropMode = false;
    this.imageChangedEvent = '';
    this.croppedImage = '';
  }
}
