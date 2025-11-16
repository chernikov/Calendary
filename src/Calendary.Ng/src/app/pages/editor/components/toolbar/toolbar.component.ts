import { Component, OnInit, OnDestroy, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSliderModule } from '@angular/material/slider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { EditorStateService, EditorTool } from '../../services/editor-state.service';
import { Subscription } from 'rxjs';

@Component({
  standalone: true,
  selector: 'app-toolbar',
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    MatButtonModule,
    MatSliderModule,
    MatTooltipModule,
    MatButtonToggleModule,
  ],
  templateUrl: './toolbar.component.html',
  styleUrl: './toolbar.component.scss',
})
export class ToolbarComponent implements OnInit, OnDestroy {
  @Output() zoomChange = new EventEmitter<number>();
  @Output() fitToScreen = new EventEmitter<void>();
  @Output() actualSize = new EventEmitter<void>();
  @Output() addText = new EventEmitter<void>();
  @Output() addRectangle = new EventEmitter<void>();
  @Output() addCircle = new EventEmitter<void>();
  @Output() addLine = new EventEmitter<void>();
  @Output() preview = new EventEmitter<void>();

  zoomLevel: number = 100;
  gridEnabled: boolean = false;
  rulersEnabled: boolean = false;
  selectedTool: EditorTool = 'select';

  private subscription: Subscription | null = null;

  constructor(private editorStateService: EditorStateService) {}

  ngOnInit(): void {
    this.subscription = this.editorStateService.state$.subscribe((state) => {
      this.zoomLevel = state.zoom;
      this.gridEnabled = state.gridEnabled;
      this.rulersEnabled = state.rulersEnabled;
      this.selectedTool = state.selectedTool;
    });
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  onZoomChange(value: number): void {
    this.editorStateService.setZoom(value);
    this.zoomChange.emit(value);
  }

  zoomIn(): void {
    const newZoom = Math.min(this.zoomLevel + 10, 400);
    this.onZoomChange(newZoom);
  }

  zoomOut(): void {
    const newZoom = Math.max(this.zoomLevel - 10, 10);
    this.onZoomChange(newZoom);
  }

  onFitToScreen(): void {
    this.fitToScreen.emit();
  }

  onActualSize(): void {
    this.editorStateService.setZoom(100);
    this.actualSize.emit();
  }

  toggleGrid(): void {
    this.editorStateService.toggleGrid();
  }

  toggleRulers(): void {
    this.editorStateService.toggleRulers();
  }

  onAddText(): void {
    this.editorStateService.setTool('text');
    this.addText.emit();
  }

  onAddRectangle(): void {
    this.editorStateService.setTool('rectangle');
    this.addRectangle.emit();
  }

  onAddCircle(): void {
    this.editorStateService.setTool('circle');
    this.addCircle.emit();
  }

  onAddLine(): void {
    this.editorStateService.setTool('line');
    this.addLine.emit();
  }

  onPreview(): void {
    this.preview.emit();
  }
}
