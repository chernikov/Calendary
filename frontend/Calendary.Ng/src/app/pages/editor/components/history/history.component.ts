import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatTooltipModule } from '@angular/material/tooltip';
import {
  EditorStateService,
  EditorAction,
} from '../../services/editor-state.service';
import { Subscription } from 'rxjs';

@Component({
  standalone: true,
  selector: 'app-history',
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatListModule,
    MatTooltipModule,
  ],
  templateUrl: './history.component.html',
  styleUrl: './history.component.scss',
})
export class HistoryComponent implements OnInit, OnDestroy {
  history: EditorAction[] = [];
  currentIndex: number = -1;
  canUndo: boolean = false;
  canRedo: boolean = false;

  private subscription: Subscription | null = null;

  constructor(private editorStateService: EditorStateService) {}

  ngOnInit(): void {
    this.subscription = this.editorStateService.state$.subscribe((state) => {
      this.history = state.history;
      this.currentIndex = state.historyIndex;
      this.canUndo = this.editorStateService.canUndo();
      this.canRedo = this.editorStateService.canRedo();
    });
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  undo(): void {
    this.editorStateService.undo();
  }

  redo(): void {
    this.editorStateService.redo();
  }

  isCurrentAction(index: number): boolean {
    return index === this.currentIndex;
  }

  isFutureAction(index: number): boolean {
    return index > this.currentIndex;
  }

  getActionIcon(action: EditorAction): string {
    switch (action.type) {
      case 'crop':
        return 'crop';
      case 'rotate':
        return 'rotate_right';
      case 'resize':
        return 'photo_size_select_large';
      case 'filter':
        return 'filter';
      case 'brightness':
        return 'brightness_6';
      case 'contrast':
        return 'contrast';
      case 'saturation':
        return 'palette';
      default:
        return 'edit';
    }
  }

  formatTime(date: Date): string {
    return new Date(date).toLocaleTimeString('uk-UA', {
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  clearHistory(): void {
    if (confirm('Очистити всю історію дій?')) {
      this.editorStateService.clearHistory();
    }
  }
}
