import { Component, EventEmitter, HostListener, Input, Output } from '@angular/core';

@Component({
  selector: 'app-image-lightbox',
  standalone: true,
  template: `
    <div class="lightbox-backdrop" (click)="closed.emit()">
      <div class="lightbox-content" (click)="$event.stopPropagation()">
        <img [src]="url" alt="Збільшене зображення" class="lightbox-img" />
        <button type="button" class="lightbox-close" (click)="closed.emit()">✕</button>
      </div>
    </div>
  `,
})
export class ImageLightboxComponent {
  @Input({ required: true }) url!: string;
  @Output() closed = new EventEmitter<void>();

  @HostListener('document:keydown.escape')
  onEscape(): void {
    this.closed.emit();
  }
}
