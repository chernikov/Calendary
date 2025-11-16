import { Injectable } from '@angular/core';
import { BehaviorSubject, combineLatest, map, Observable } from 'rxjs';
import {
  CanvasElement,
  CanvasElementType,
  TextCanvasElement,
  ShapeCanvasElement,
} from '../models/canvas-overlay.model';
import { EditorStateService } from './editor-state.service';

@Injectable({ providedIn: 'root' })
export class CanvasOverlayService {
  private readonly elementsSubject = new BehaviorSubject<CanvasElement[]>([]);
  private readonly selectedIdSubject = new BehaviorSubject<string | null>(null);

  readonly elements$ = this.elementsSubject.asObservable();
  readonly selectedElement$: Observable<CanvasElement | null> = combineLatest([
    this.elementsSubject,
    this.selectedIdSubject,
  ]).pipe(
    map(([elements, selectedId]) => elements.find((element) => element.id === selectedId) ?? null)
  );

  constructor(private readonly editorState: EditorStateService) {}

  addElement(type: CanvasElementType): CanvasElement {
    const element = this.createElement(type);
    this.elementsSubject.next([...this.elementsSubject.value, element]);
    this.selectElement(element.id);
    this.editorState.addAction('canvas-add', element, `Додано ${type}`);
    return element;
  }

  updateElement(id: string, changes: Partial<CanvasElement>): void {
    const updated = this.elementsSubject.value.map((element) =>
      element.id === id ? { ...element, ...changes } as CanvasElement : element
    );
    this.elementsSubject.next(updated as CanvasElement[]);
  }

  removeElement(id: string): void {
    this.elementsSubject.next(this.elementsSubject.value.filter((element) => element.id !== id));
    if (this.selectedIdSubject.value === id) {
      this.selectedIdSubject.next(null);
    }
  }

  selectElement(id: string | null): void {
    this.selectedIdSubject.next(id);
  }

  clear(): void {
    this.elementsSubject.next([]);
    this.selectedIdSubject.next(null);
  }

  private createElement(type: CanvasElementType): CanvasElement {
    const id = crypto.randomUUID?.() ?? Math.random().toString(36).slice(2);

    if (type === 'text') {
      return {
        id,
        type,
        x: 40,
        y: 40,
        width: 220,
        height: 80,
        rotation: 0,
        fill: '#111827',
        stroke: 'transparent',
        strokeWidth: 0,
        opacity: 1,
        text: 'Новий текст',
        fontFamily: 'Inter, sans-serif',
        fontSize: 24,
        fontWeight: 'bold',
        fontStyle: 'normal',
        underline: false,
        align: 'center',
      } satisfies TextCanvasElement;
    }

    const base = {
      id,
      type,
      x: 60,
      y: 60,
      width: 160,
      height: type === 'line' ? 4 : 160,
      rotation: type === 'line' ? 0 : 0,
      fill: type === 'line' ? 'transparent' : '#f59e0b',
      stroke: '#111827',
      strokeWidth: type === 'line' ? 3 : 2,
      opacity: 0.9,
    } as ShapeCanvasElement;

    return base;
  }
}
