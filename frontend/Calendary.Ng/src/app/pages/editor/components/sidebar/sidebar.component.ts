import {
  Component,
  Output,
  EventEmitter,
  OnInit,
  OnDestroy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import {
  EditorStateService,
  EditorTool,
} from '../../services/editor-state.service';
import { Subscription } from 'rxjs';

interface ToolButton {
  id: EditorTool;
  icon: string;
  label: string;
  tooltip: string;
}

@Component({
  standalone: true,
  selector: 'app-sidebar',
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatDividerModule,
  ],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
})
export class SidebarComponent implements OnInit, OnDestroy {
  @Output() generateImage = new EventEmitter<void>();
  @Output() uploadImage = new EventEmitter<void>();
  @Output() saveImage = new EventEmitter<void>();
  @Output() exportImage = new EventEmitter<void>();

  selectedTool: EditorTool = 'select';
  private subscription: Subscription | null = null;

  tools: ToolButton[] = [
    {
      id: 'select',
      icon: 'mouse',
      label: 'Вибір',
      tooltip: 'Інструмент вибору',
    },
    {
      id: 'crop',
      icon: 'crop',
      label: 'Обрізка',
      tooltip: 'Обрізати зображення',
    },
    {
      id: 'rotate',
      icon: 'rotate_right',
      label: 'Поворот',
      tooltip: 'Повернути зображення',
    },
    {
      id: 'resize',
      icon: 'photo_size_select_large',
      label: 'Розмір',
      tooltip: 'Змінити розмір',
    },
  ];

  filterTools: ToolButton[] = [
    {
      id: 'brightness',
      icon: 'brightness_6',
      label: 'Яскравість',
      tooltip: 'Налаштувати яскравість',
    },
    {
      id: 'contrast',
      icon: 'contrast',
      label: 'Контраст',
      tooltip: 'Налаштувати контраст',
    },
    {
      id: 'saturation',
      icon: 'palette',
      label: 'Насиченість',
      tooltip: 'Налаштувати насиченість',
    },
  ];

  constructor(private editorStateService: EditorStateService) {}

  ngOnInit(): void {
    this.subscription = this.editorStateService.state$.subscribe((state) => {
      this.selectedTool = state.selectedTool;
    });
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  selectTool(tool: EditorTool): void {
    this.editorStateService.setTool(tool);
  }

  onGenerateImage(): void {
    this.generateImage.emit();
  }

  onUploadImage(): void {
    this.uploadImage.emit();
  }

  onSaveImage(): void {
    this.saveImage.emit();
  }

  onExportImage(): void {
    this.exportImage.emit();
  }
}
