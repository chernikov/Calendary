import { Component, EventEmitter, HostListener, Input, OnChanges, OnInit, Output, SimpleChanges, signal } from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { ImageStyleDto, OrderPhotoDto, PromptDto, PromptLibraryDto, SheetStatus, SheetVariantDto } from '../../core/models';
import { ImageLightboxComponent } from '../../shared/image-lightbox.component';

type PickerTab = 'photo' | 'style' | 'prompt';

/// Unified per-sheet picker (see #351) — used from the planning-step tile grid, the cover page,
/// and each month page. Owns its own staged picks (seeded from the *Input on open) rather than a
/// live reference into the caller's state, since callers differ: style-dates has a local unsaved
/// "plan" row per sheet, cover/month operate directly on an already-generated Sheet.
@Component({
  selector: 'app-sheet-picker-modal',
  standalone: true,
  template: `
    <div class="dialog-backdrop" (click)="closed.emit()">
      <div class="dialog sheet-modal" (click)="$event.stopPropagation()">
        <div class="sheet-modal-left">
          <div class="dialog-title">{{ sheetName }}</div>

          <div class="sheet-summary">
            <button type="button" class="sheet-summary-chip" [class.active]="activeTab() === 'photo'" (click)="activeTab.set('photo')">
              @if (selectedPhoto(); as p) {
                <img [src]="p.thumbUrl" alt="Фото" />
              } @else {
                <div class="swatch"></div>
              }
              <span>Фото</span>
            </button>
            <button type="button" class="sheet-summary-chip" [class.active]="activeTab() === 'style'" (click)="activeTab.set('style')">
              @if (selectedStyleImage(); as src) {
                <img [src]="src" alt="Стиль" />
              } @else {
                <div class="swatch"></div>
              }
              <span>{{ styleName() || 'Стиль' }}</span>
            </button>
            <button type="button" class="sheet-summary-chip" [class.active]="activeTab() === 'prompt'" (click)="activeTab.set('prompt')">
              @if (selectedPromptImage(); as src) {
                <img [src]="src" alt="Персонаж" />
              } @else {
                <div class="swatch"></div>
              }
              <span>{{ promptName() || 'Персонаж' }}</span>
            </button>
          </div>

          <div class="sheet-picker-body">
            @if (activeTab() === 'photo') {
              <div class="picker-grid picker-grid-compact">
                @for (photo of photos; track photo.id) {
                  <button
                    type="button"
                    class="picker-item picker-item-compact"
                    [class.selected]="isPhotoSelected(photo.id)"
                    (click)="photoId.set(photo.id)"
                  >
                    <img [src]="photo.thumbUrl" alt="Фото" loading="lazy" />
                  </button>
                }
                <button type="button" class="picker-item picker-item-compact picker-item-add" (click)="fileInput.click()">
                  <span class="picker-item-add-icon">+</span>
                  <span class="picker-item-name">Додати</span>
                </button>
                <input #fileInput type="file" accept="image/*" hidden (change)="onPhotoFileSelected($event)" />
              </div>
              @if (photoUploadError) {
                <p style="color: var(--color-accent-2-700); font-size: 12px; margin-top: var(--space-2);">{{ photoUploadError }}</p>
              }
            } @else if (activeTab() === 'style') {
              <div class="picker-grid picker-grid-compact">
                @for (style of library?.styles ?? []; track style.id) {
                  <button
                    type="button"
                    class="picker-item picker-item-compact"
                    [class.selected]="styleId() === style.id"
                    (click)="styleId.set(style.id)"
                    [title]="style.description"
                  >
                    <img [src]="styleImage(style)" [alt]="style.name" loading="lazy" />
                    <span class="picker-item-name">{{ style.name }}</span>
                  </button>
                }
              </div>
            } @else {
              @for (theme of library?.themes ?? []; track theme.id) {
                <div>
                  <div class="card-title" style="margin-bottom: 4px;">{{ theme.name }}</div>
                  <div class="picker-grid picker-grid-compact">
                    @for (prompt of theme.prompts; track prompt.id) {
                      <button
                        type="button"
                        class="picker-item picker-item-compact"
                        [class.selected]="promptId() === prompt.id"
                        (click)="promptId.set(prompt.id)"
                        [title]="prompt.description"
                      >
                        <img [src]="promptImage(prompt)" [alt]="prompt.name" loading="lazy" />
                        <span class="picker-item-name">{{ prompt.name }}</span>
                      </button>
                    }
                  </div>
                </div>
              }
            }
          </div>
        </div>

        <div class="sheet-modal-right">
          <div class="sheet-preview-inline">
            <ng-container *ngTemplateOutlet="previewTpl"></ng-container>
          </div>

          @if (variants.length > 0) {
            <p class="text-muted" style="font-size: 11.5px;">Залишилось {{ regenerationsLeft() }} перегенерацій</p>
          }

          <div class="dialog-actions">
            <button class="btn btn-secondary" (click)="closed.emit()">Закрити</button>
            <button
              class="btn btn-primary"
              [disabled]="!promptId() || !styleId() || status === 'Generating'"
              (click)="onGenerate()"
            >
              Згенерувати
            </button>
          </div>
        </div>
      </div>

      @if (previewModalOpen()) {
        <div class="sheet-preview-modal-backdrop" (click)="previewModalOpen.set(false)">
          <div class="sheet-preview-modal" (click)="$event.stopPropagation()">
            <ng-container *ngTemplateOutlet="previewTpl"></ng-container>
            <button class="btn btn-secondary" (click)="previewModalOpen.set(false)">Закрити</button>
          </div>
        </div>
      }

      @if (zoomUrl(); as z) {
        <app-image-lightbox [url]="z" (closed)="zoomUrl.set(null)" />
      }
    </div>

    <ng-template #previewTpl>
      <div class="sheet-preview">
        @if (status === 'Generating') {
          <span class="text-muted">Генерується…</span>
        } @else {
          @if (viewedVariant(); as v) {
            <img [src]="v.imageUrl" alt="Згенероване зображення" (click)="zoomUrl.set(v.imageUrl)" />
            <button type="button" class="zoom-trigger" (click)="zoomUrl.set(v.imageUrl)">⤢</button>
            @if (variants.length > 1) {
              <button type="button" class="sheet-preview-arrow prev" [disabled]="viewedIndex() === 0" (click)="showPrevVariant()">‹</button>
              <button type="button" class="sheet-preview-arrow next" [disabled]="viewedIndex() === variants.length - 1" (click)="showNextVariant()">›</button>
            }
          } @else {
            <div class="sheet-preview-composite">
              <div class="sheet-preview-composite-item">
                @if (selectedPhoto(); as p) {
                  <img [src]="p.thumbUrl" alt="Фото" />
                } @else {
                  <div class="swatch"></div>
                }
                <span>Фото</span>
              </div>
              <div class="sheet-preview-composite-item">
                @if (selectedStyleImage(); as src) {
                  <img [src]="src" alt="Стиль" />
                } @else {
                  <div class="swatch"></div>
                }
                <span>{{ styleName() || 'Стиль не обрано' }}</span>
              </div>
              <div class="sheet-preview-composite-item">
                @if (selectedPromptImage(); as src) {
                  <img [src]="src" alt="Персонаж" />
                } @else {
                  <div class="swatch"></div>
                }
                <span>{{ promptName() || 'Персонаж не обрано' }}</span>
              </div>
            </div>
          }
        }
      </div>
    </ng-template>
  `,
  imports: [ImageLightboxComponent, NgTemplateOutlet],
})
export class SheetPickerModalComponent implements OnInit, OnChanges {
  @Input({ required: true }) sheetName!: string;
  @Input({ required: true }) photos!: OrderPhotoDto[];
  @Input() library: PromptLibraryDto | null = null;
  @Input({ required: true }) initialPromptId!: string;
  @Input({ required: true }) initialStyleId!: string;
  @Input({ required: true }) initialPhotoId!: string;
  @Input() variants: SheetVariantDto[] = [];
  @Input() activeVariantId: string | null = null;
  @Input() status: SheetStatus = 'Pending';
  @Input() regenerationsRemaining = 0;
  @Input() photoUploadError: string | null = null;

  @Output() closed = new EventEmitter<void>();
  @Output() generate = new EventEmitter<{ promptId: string; styleId: string; photoId: string }>();
  @Output() activateVariant = new EventEmitter<string>();
  @Output() addPhoto = new EventEmitter<File>();

  readonly activeTab = signal<PickerTab>('photo');
  readonly promptId = signal('');
  readonly styleId = signal('');
  readonly photoId = signal('');
  readonly viewedIndex = signal(0);
  readonly zoomUrl = signal<string | null>(null);
  // Mobile-only (see .sheet-preview-inline's media query): the preview panel isn't shown inline
  // there, so pop it up automatically once generation starts, through to the result.
  readonly previewModalOpen = signal(false);

  @HostListener('document:keydown.escape')
  onEscape(): void {
    this.closed.emit();
  }

  ngOnInit(): void {
    this.promptId.set(this.initialPromptId);
    this.styleId.set(this.initialStyleId);
    this.photoId.set(this.initialPhotoId);
  }

  // The modal instance stays open across polling updates (e.g. while a generation is running),
  // so a newly-arrived variant/active-pointer must re-sync the viewed index reactively — seeding
  // it once in ngOnInit would leave the gallery stuck showing the composite placeholder forever
  // once the first variant actually finishes.
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['variants'] || changes['activeVariantId']) {
      const activeIndex = this.variants.findIndex((v) => v.id === this.activeVariantId);
      this.viewedIndex.set(activeIndex >= 0 ? activeIndex : this.variants.length - 1);
    }

    // On mobile the preview is popup-only (no inline panel — see .sheet-preview-inline's media
    // query), so surface it the moment a generation kicks off; it stays open through to the
    // result since it's the same popup instance reflecting the same reactive state.
    const statusChange = changes['status'];
    if (statusChange && !statusChange.firstChange && this.status === 'Generating' && this.isMobileViewport()) {
      this.previewModalOpen.set(true);
    }
  }

  private isMobileViewport(): boolean {
    return typeof window !== 'undefined' && window.matchMedia('(max-width: 640px)').matches;
  }

  // Empty photoId means "use the default" — the first uploaded photo, not "no photo" — so the
  // summary/preview should actually show it, not a blank swatch.
  selectedPhoto(): OrderPhotoDto | undefined {
    return this.photoId() ? this.photos.find((p) => p.id === this.photoId()) : this.photos[0];
  }

  // With no explicit pin, the first uploaded photo IS the default — highlight its own tile
  // instead of a separate duplicate "default" tile that looks identical when there's one photo.
  isPhotoSelected(photoId: string): boolean {
    return this.photoId() ? this.photoId() === photoId : this.photos[0]?.id === photoId;
  }

  // Generation is no longer hard-capped (see #359) — the counter can go negative server-side as a
  // pure usage signal, but showing that to customers would look broken, so clamp at 0.
  regenerationsLeft(): number {
    return Math.max(0, this.regenerationsRemaining);
  }

  onPhotoFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;
    this.addPhoto.emit(file);
  }

  styleName(): string {
    return this.library?.styles.find((s) => s.id === this.styleId())?.name ?? '';
  }

  promptName(): string {
    for (const theme of this.library?.themes ?? []) {
      const prompt = theme.prompts.find((p) => p.id === this.promptId());
      if (prompt) return prompt.name;
    }
    return '';
  }

  selectedStyleImage(): string | null {
    const style = this.library?.styles.find((s) => s.id === this.styleId());
    return style ? this.styleImage(style) : null;
  }

  selectedPromptImage(): string | null {
    for (const theme of this.library?.themes ?? []) {
      const prompt = theme.prompts.find((p) => p.id === this.promptId());
      if (prompt) return this.promptImage(prompt);
    }
    return null;
  }

  // Admin-generated previews land in previewImageUrl; until then show a stable placeholder.
  promptImage(prompt: PromptDto): string {
    return prompt.previewImageUrl ?? `https://picsum.photos/seed/prompt-${prompt.id}/240/300`;
  }

  styleImage(style: ImageStyleDto): string {
    return style.previewImageUrl ?? `https://picsum.photos/seed/style-${style.id}/240/300`;
  }

  viewedVariant(): SheetVariantDto | undefined {
    return this.variants[this.viewedIndex()];
  }

  showPrevVariant(): void {
    if (this.viewedIndex() === 0) return;
    this.viewedIndex.update((i) => i - 1);
    this.restoreViewedVariant();
  }

  showNextVariant(): void {
    if (this.viewedIndex() === this.variants.length - 1) return;
    this.viewedIndex.update((i) => i + 1);
    this.restoreViewedVariant();
  }

  private restoreViewedVariant(): void {
    const variant = this.viewedVariant();
    if (variant && variant.id !== this.activeVariantId) {
      this.activateVariant.emit(variant.id);
    }
  }

  onGenerate(): void {
    if (!this.promptId() || !this.styleId()) return;
    this.generate.emit({ promptId: this.promptId(), styleId: this.styleId(), photoId: this.photoId() });
  }
}
