import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export type EditorTool =
  | 'select'
  | 'crop'
  | 'rotate'
  | 'resize'
  | 'filter'
  | 'brightness'
  | 'contrast'
  | 'saturation'
  | 'text'
  | 'rectangle'
  | 'circle'
  | 'line';

export interface EditorAction {
  type: string;
  timestamp: Date;
  data: any;
  description: string;
}

export interface EditorState {
  currentImage: HTMLImageElement | null;
  history: EditorAction[];
  historyIndex: number;
  zoom: number;
  gridEnabled: boolean;
  rulersEnabled: boolean;
  selectedTool: EditorTool;
  isDirty: boolean;
  imageWidth: number;
  imageHeight: number;
  imageFormat: string;
  imageQuality: number;
}

const initialState: EditorState = {
  currentImage: null,
  history: [],
  historyIndex: -1,
  zoom: 100,
  gridEnabled: false,
  rulersEnabled: false,
  selectedTool: 'select',
  isDirty: false,
  imageWidth: 0,
  imageHeight: 0,
  imageFormat: 'PNG',
  imageQuality: 95
};

@Injectable({
  providedIn: 'root'
})
export class EditorStateService {
  private stateSubject = new BehaviorSubject<EditorState>(initialState);
  public state$: Observable<EditorState> = this.stateSubject.asObservable();

  constructor() {}

  get currentState(): EditorState {
    return this.stateSubject.value;
  }

  setTool(tool: EditorTool): void {
    this.updateState({ selectedTool: tool });
  }

  setZoom(zoom: number): void {
    this.updateState({ zoom: Math.max(10, Math.min(400, zoom)) });
  }

  toggleGrid(): void {
    this.updateState({ gridEnabled: !this.currentState.gridEnabled });
  }

  toggleRulers(): void {
    this.updateState({ rulersEnabled: !this.currentState.rulersEnabled });
  }

  setImageDimensions(width: number, height: number): void {
    this.updateState({ imageWidth: width, imageHeight: height });
  }

  setImageFormat(format: string): void {
    this.updateState({ imageFormat: format });
  }

  setImageQuality(quality: number): void {
    this.updateState({ imageQuality: Math.max(1, Math.min(100, quality)) });
  }

  addAction(type: string, data: any, description: string): void {
    const newAction: EditorAction = {
      type,
      data,
      description,
      timestamp: new Date()
    };

    // Remove any actions after the current history index
    const newHistory = this.currentState.history.slice(0, this.currentState.historyIndex + 1);
    newHistory.push(newAction);

    this.updateState({
      history: newHistory,
      historyIndex: newHistory.length - 1,
      isDirty: true
    });
  }

  undo(): boolean {
    if (this.currentState.historyIndex > 0) {
      this.updateState({
        historyIndex: this.currentState.historyIndex - 1
      });
      return true;
    }
    return false;
  }

  redo(): boolean {
    if (this.currentState.historyIndex < this.currentState.history.length - 1) {
      this.updateState({
        historyIndex: this.currentState.historyIndex + 1
      });
      return true;
    }
    return false;
  }

  canUndo(): boolean {
    return this.currentState.historyIndex > 0;
  }

  canRedo(): boolean {
    return this.currentState.historyIndex < this.currentState.history.length - 1;
  }

  clearHistory(): void {
    this.updateState({
      history: [],
      historyIndex: -1,
      isDirty: false
    });
  }

  markAsSaved(): void {
    this.updateState({ isDirty: false });
  }

  reset(): void {
    this.stateSubject.next(initialState);
  }

  private updateState(partial: Partial<EditorState>): void {
    this.stateSubject.next({
      ...this.currentState,
      ...partial
    });
  }
}
