import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatSliderModule } from '@angular/material/slider';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { FormsModule } from '@angular/forms';
import { JobTask } from '../../../../../models/job-task';
import { EditorStateService } from '../../services/editor-state.service';
import { Subscription } from 'rxjs';
import { CanvasOverlayService } from '../../services/canvas-overlay.service';
import { CanvasElement, TextCanvasElement } from '../../models/canvas-overlay.model';
import { ColorPickerComponent } from '../../../components/ui/color-picker/color-picker.component';

@Component({
    standalone: true,
    selector: 'app-properties-panel',
    imports: [
        CommonModule,
        MatIconModule,
        MatDividerModule,
        MatSliderModule,
        MatSelectModule,
        MatButtonModule,
        MatInputModule,
        MatButtonToggleModule,
        FormsModule,
        ColorPickerComponent
    ],
    templateUrl: './properties-panel.component.html',
    styleUrl: './properties-panel.component.scss'
})
export class PropertiesPanelComponent implements OnInit, OnDestroy {
  @Input() selectedImage: JobTask | null = null;

  imageFormat = 'PNG';
  imageQuality = 95;
  imageWidth = 0;
  imageHeight = 0;
  overlayElements: CanvasElement[] = [];
  selectedOverlay: CanvasElement | null = null;

  formats = ['PNG', 'JPG', 'WebP'];
  fontFamilies = ['Inter, sans-serif', 'Merriweather, serif', 'Roboto, sans-serif'];

  private subscriptions = new Subscription();

  constructor(
    private editorStateService: EditorStateService,
    private overlayService: CanvasOverlayService
  ) {}

  ngOnInit(): void {
    this.subscriptions.add(this.editorStateService.state$.subscribe(state => {
      this.imageFormat = state.imageFormat;
      this.imageQuality = state.imageQuality;
      this.imageWidth = state.imageWidth;
      this.imageHeight = state.imageHeight;
    }));

    this.subscriptions.add(
      this.overlayService.elements$.subscribe((elements) => (this.overlayElements = elements))
    );

    this.subscriptions.add(
      this.overlayService.selectedElement$.subscribe((element) => (this.selectedOverlay = element))
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  get imageUrl(): string {
    if (!this.selectedImage) return '';
    return this.selectedImage.processedImageUrl || this.selectedImage.imageUrl || '';
  }

  onFormatChange(format: string): void {
    this.editorStateService.setImageFormat(format);
  }

  onQualityChange(quality: number): void {
    this.editorStateService.setImageQuality(quality);
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

  selectOverlay(element: CanvasElement): void {
    this.overlayService.selectElement(element.id);
  }

  removeOverlay(element: CanvasElement, event?: Event): void {
    event?.stopPropagation();
    this.overlayService.removeElement(element.id);
  }

  updateOverlay(changes: Partial<CanvasElement>): void {
    if (!this.selectedOverlay) {
      return;
    }
    this.overlayService.updateElement(this.selectedOverlay.id, changes);
  }

  updateTextOverlay(changes: Partial<TextCanvasElement>): void {
    if (!this.isTextElement(this.selectedOverlay)) {
      return;
    }
    this.overlayService.updateElement(this.selectedOverlay.id, changes);
  }

  isTextElement(element: CanvasElement | null): element is TextCanvasElement {
    return !!element && element.type === 'text';
  }
}
