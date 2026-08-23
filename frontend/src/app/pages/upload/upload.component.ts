import { Component, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { OrderService } from '../../core/order.service';

@Component({
  selector: 'app-upload',
  standalone: true,
  template: `
    <div class="page page-narrow">
      <div class="step-label"><span>Крок 2 із 5</span></div>
      <h2 style="font-size: 28px;">Ваше фото</h2>
      <p class="text-muted">
        Одне чітке фото обличчя, без сонцезахисних окулярів чи капелюхів, гарне освітлення.
        Ми використаємо його як основу для образів усіх дванадцяти місяців.
      </p>

      <label
        for="photo"
        style="display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 8px;
               border: 1px dashed var(--color-divider); border-radius: var(--radius-md); padding: var(--space-6);
               cursor: pointer; background: var(--color-surface); text-align: center;"
      >
        @if (previewUrl()) {
          <img [src]="previewUrl()" style="max-height: 220px; border-radius: var(--radius-sm);" alt="Прев'ю фото" />
        } @else {
          <span class="d" style="font-family: var(--font-heading); font-weight: 600; font-size: 18px;">Перетягніть фото сюди</span>
          <span class="text-muted" style="font-size: 13px;">або натисніть, щоб обрати файл</span>
        }
      </label>
      <input id="photo" type="file" accept="image/*" style="display: none;" (change)="onFileSelected($event)" />

      @if (error()) {
        <p style="color: var(--color-accent-2-700); font-size: 13px; margin-top: var(--space-2);">{{ error() }}</p>
      }

      <button class="btn btn-primary btn-block" [disabled]="!previewUrl() || loading()" (click)="continue()">
        Далі
      </button>
    </div>
  `,
})
export class UploadComponent {
  readonly previewUrl = signal<string | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  private readonly orderId: string;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly orders: OrderService,
  ) {
    this.orderId = this.route.snapshot.paramMap.get('orderId')!;
  }

  onFileSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    if (!file.type.startsWith('image/')) {
      this.error.set('Оберіть файл зображення.');
      return;
    }
    this.error.set(null);
    const reader = new FileReader();
    reader.onload = () => this.previewUrl.set(reader.result as string);
    reader.readAsDataURL(file);
  }

  continue(): void {
    const dataUrl = this.previewUrl();
    if (!dataUrl) return;
    this.loading.set(true);
    this.orders.uploadPhoto(this.orderId, dataUrl).subscribe({
      next: () => this.router.navigate(['/order', this.orderId, 'style']),
      error: () => {
        this.error.set('Не вдалося завантажити фото.');
        this.loading.set(false);
      },
    });
  }
}
