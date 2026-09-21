import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { OrderActions, selectDownloadingPdf, selectOrder } from '../../core/state/order';
import { OrderDto } from '../../core/models';
import { OrderService } from '../../core/order.service';
import { ImageLightboxComponent } from '../../shared/image-lightbox.component';
import { PdfPreviewComponent } from '../../shared/pdf-preview.component';

const MONTH_NAMES = [
  'Січень', 'Лютий', 'Березень', 'Квітень', 'Травень', 'Червень',
  'Липень', 'Серпень', 'Вересень', 'Жовтень', 'Листопад', 'Грудень',
];

@Component({
    selector: 'app-review',
    template: `
    <div class="page">
      <h2 style="font-size: 28px;">Ваш календар</h2>

      @if (order(); as o) {
        @if (o.status !== 'ReviewReady' && !isPastReview(o)) {
          <p class="text-muted">Ще не всі аркуші готові — поверніться, коли генерація завершиться.</p>
        }

        @if (o.status === 'ReviewReady') {
          <a [routerLink]="['/order', o.id, 'style']" style="display: inline-flex; align-items: center; gap: 4px; margin-bottom: var(--space-2); font-size: 13.5px;">
            ← Назад до образів
          </a>
        }

        <div style="display: grid; grid-template-columns: repeat(auto-fill, minmax(110px, 1fr)); gap: var(--space-2); margin: var(--space-4) 0;">
          @for (sheet of o.sheets; track sheet.id) {
            <a
              [routerLink]="sheet.kind === 'Cover' ? ['/order', o.id, 'cover'] : ['/order', o.id, 'months', sheet.index]"
              style="text-decoration: none; color: inherit; position: relative; display: block;"
            >
              @if (sheet.imageUrl) {
                <div style="aspect-ratio: 3/4; background-size: cover; background-position: center; border-radius: var(--radius-sm);"
                     [style.background-image]="'url(' + sheet.imageUrl + ')'"></div>
                <button
                  type="button"
                  class="zoom-trigger"
                  (click)="$event.preventDefault(); $event.stopPropagation(); zoomUrl.set(sheet.imageUrl!)"
                >⤢</button>
              } @else {
                <div class="sheet-thumb" style="width: 100%; height: auto; aspect-ratio: 3/4;"></div>
              }
              <div style="font-size: 11px; margin-top: 4px; text-align: center;">
                {{ sheet.kind === 'Cover' ? 'Обкладинка' : monthName(sheet.index) }}
              </div>
            </a>
          }
        </div>

        <div class="hr"></div>

        <div style="display: flex; justify-content: space-between; align-items: baseline; padding: 16px 0;">
          <span class="d" style="font-family: var(--font-heading); font-weight: 600; font-size: 17px;">До сплати</span>
          <span class="money" style="font-size: 30px; font-weight: 500;">{{ o.price }} ₴</span>
        </div>

        @if (o.status === 'ReviewReady' || isPastReview(o)) {
          <div style="display: flex; gap: var(--space-2); max-width: 320px; margin-bottom: var(--space-2);">
            <button
              class="btn btn-secondary"
              style="flex: 1;"
              [disabled]="previewLoading()"
              (click)="openPreview(o)"
            >
              {{ previewLoading() ? 'Завантаження…' : 'Переглянути' }}
            </button>
            <button
              class="btn btn-secondary"
              style="flex: 1;"
              [disabled]="downloadingPdf()"
              (click)="downloadPdf(o)"
            >
              Завантажити PDF
            </button>
          </div>
          @if (previewError()) {
            <p style="color: var(--color-accent-2-700); font-size: 13px; margin-bottom: var(--space-2);">{{ previewError() }}</p>
          }
        }

        <button class="btn btn-primary btn-block" style="max-width: 320px;" (click)="proceed(o)">До оплати</button>
      }

      @if (zoomUrl(); as z) {
        <app-image-lightbox [url]="z" (closed)="zoomUrl.set(null)" />
      }

      @if (previewUrl(); as p) {
        <app-pdf-preview [url]="p" [fileName]="previewFileName()" (closed)="closePreview()" />
      }
    </div>
  `,
    imports: [RouterLink, ImageLightboxComponent, PdfPreviewComponent]
})
export class ReviewComponent implements OnInit {
  private readonly store = inject(Store);
  private readonly orderService = inject(OrderService);
  readonly order = this.store.selectSignal(selectOrder);
  readonly downloadingPdf = this.store.selectSignal(selectDownloadingPdf);
  readonly zoomUrl = signal<string | null>(null);
  readonly previewUrl = signal<string | null>(null);
  readonly previewFileName = signal<string>('calendary.pdf');
  readonly previewLoading = signal(false);
  readonly previewError = signal<string | null>(null);
  private readonly orderId: string;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
  ) {
    this.orderId = this.route.snapshot.paramMap.get('orderId')!;
  }

  ngOnInit(): void {
    this.store.dispatch(OrderActions.loadOrder({ orderId: this.orderId }));
  }

  monthName(index: number): string {
    return MONTH_NAMES[index - 1] ?? '';
  }

  isPastReview(o: OrderDto): boolean {
    return ['AwaitingPayment', 'Paid', 'Printing', 'PrintReady', 'Shipped', 'Delivered'].includes(o.status);
  }

  proceed(o: OrderDto): void {
    this.router.navigate(['/order', o.id, 'checkout']);
  }

  downloadPdf(o: OrderDto): void {
    this.store.dispatch(OrderActions.downloadPdf({ orderId: o.id }));
  }

  openPreview(o: OrderDto): void {
    this.previewLoading.set(true);
    this.previewError.set(null);
    this.orderService.downloadPdf(o.id).subscribe({
      next: (blob) => {
        this.previewFileName.set(`calendary-${o.id}.pdf`);
        this.previewUrl.set(URL.createObjectURL(blob));
        this.previewLoading.set(false);
      },
      error: () => {
        this.previewError.set('Не вдалося завантажити PDF.');
        this.previewLoading.set(false);
      },
    });
  }

  closePreview(): void {
    const url = this.previewUrl();
    if (url) {
      URL.revokeObjectURL(url);
    }
    this.previewUrl.set(null);
  }
}
