import { Component, EventEmitter, HostListener, Input, Output, inject } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

@Component({
  selector: 'app-pdf-preview',
  standalone: true,
  template: `
    <div class="pdf-preview-backdrop" (click)="closed.emit()">
      <div class="pdf-preview-panel" (click)="$event.stopPropagation()">
        <div class="pdf-preview-header">
          <span class="dialog-title" style="font-size: 16px;">Перегляд календаря</span>
          <div style="display: flex; gap: var(--space-2); align-items: center;">
            <a class="btn btn-secondary" [href]="url" [download]="fileName">Завантажити</a>
            <button type="button" class="lightbox-close" (click)="closed.emit()">✕</button>
          </div>
        </div>
        <iframe [src]="safeUrl" class="pdf-preview-frame" title="Перегляд PDF"></iframe>
      </div>
    </div>
  `,
})
export class PdfPreviewComponent {
  @Input({ required: true }) url!: string;
  @Input({ required: true }) fileName!: string;
  @Output() closed = new EventEmitter<void>();

  private readonly sanitizer = inject(DomSanitizer);

  get safeUrl(): SafeResourceUrl {
    return this.sanitizer.bypassSecurityTrustResourceUrl(this.url);
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    this.closed.emit();
  }
}
