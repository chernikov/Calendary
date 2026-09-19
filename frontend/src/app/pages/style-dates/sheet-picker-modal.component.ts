import { Component, EventEmitter, HostListener, Input, OnChanges, OnInit, Output, SimpleChanges, signal } from '@angular/core';
import { ImageStyleDto, OrderPhotoDto, PromptDto, PromptLibraryDto, SheetStatus, SheetVariantDto } from '../../core/models';

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
            <div class="sheet-summary-chip">
              @if (selectedPhoto(); as p) {
                <img [src]="p.thumbUrl" alt="Фото" />
              } @else {
                <div class="swatch"></div>
              }
              <span>Фото</span>
            </div>
            <div class="sheet-summary-chip">
              @if (selectedStyleImage(); as src) {
                <img [src]="src" alt="Стиль" />
              } @else {
                <div class="swatch"></div>
              }
              <span>{{ styleName() || 'Стиль' }}</span>
            </div>
            <div class="sheet-summary-chip">
              @if (selectedPromptImage(); as src) {
                <img [src]="src" alt="Персонаж" />
              } @else {
                <div class="swatch"></div>
              }
              <span>{{ promptName() || 'Персонаж' }}</span>
            </div>
          </div>

          <div class="sheet-tabs">
            <button type="button" class="sheet-tab" [class.active]="activeTab() === 'photo'" (click)="activeTab.set('photo')">Фото</button>
            <button type="button" class="sheet-tab" [class.active]="activeTab() === 'style'" (click)="activeTab.set('style')">Стиль</button>
            <button type="button" class="sheet-tab" [class.active]="activeTab() === 'prompt'" (click)="activeTab.set('prompt')">Персонаж</button>
          </div>

          @if (activeTab() === 'photo') {
            <div class="picker-grid">
              <button
                type="button"
                class="picker-item"
                [class.selected]="!photoId()"
                (click)="photoId.set('')"
              >
                <div class="swatch" style="aspect-ratio: 4/5;"></div>
                <span class="picker-item-name">За замовчуванням</span>
                <span class="picker-item-desc">Перше завантажене фото</span>
              </button>
              @for (photo of photos; track photo.id) {
                <button
                  type="button"
                  class="picker-item"
                  [class.selected]="photoId() === photo.id"
                  (click)="photoId.set(photo.id)"
                >
                  <img [src]="photo.thumbUrl" alt="Фото" loading="lazy" />
                </button>
              }
            </div>
          } @else if (activeTab() === 'style') {
            <div class="picker-grid">
              @for (style of library?.styles ?? []; track style.id) {
                <button
                  type="button"
                  class="picker-item"
                  [class.selected]="styleId() === style.id"
                  (click)="styleId.set(style.id)"
                >
                  <img [src]="styleImage(style)" [alt]="style.name" loading="lazy" />
                  <span class="picker-item-name">{{ style.name }}</span>
                  <span class="picker-item-desc">{{ style.description }}</span>
                </button>
              }
            </div>
          } @else {
            @for (theme of library?.themes ?? []; track theme.id) {
              <div>
                <div class="card-title" style="margin-bottom: 4px;">{{ theme.name }}</div>
                <p class="picker-theme-desc">{{ theme.description }}</p>
                <div class="picker-grid">
                  @for (prompt of theme.prompts; track prompt.id) {
                    <button
                      type="button"
                      class="picker-item"
                      [class.selected]="promptId() === prompt.id"
                      (click)="promptId.set(prompt.id)"
                    >
                      <img [src]="promptImage(prompt)" [alt]="prompt.name" loading="lazy" />
                      <span class="picker-item-name">{{ prompt.name }}</span>
                      <span class="picker-item-desc">{{ prompt.description }}</span>
                    </button>
                  }
                </div>
              </div>
            }
          }
        </div>

        <div class="sheet-modal-right">
          <div class="sheet-preview">
            @if (status === 'Generating') {
              <span class="text-muted">Генерується…</span>
            } @else {
              @if (viewedVariant(); as v) {
                <img [src]="v.imageUrl" alt="Згенероване зображення" />
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

          @if (variants.length > 0) {
            <p class="text-muted" style="font-size: 11.5px;">Залишилось {{ regenerationsRemaining }} перегенерацій</p>
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
    </div>
  `,
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

  @Output() closed = new EventEmitter<void>();
  @Output() generate = new EventEmitter<{ promptId: string; styleId: string; photoId: string }>();
  @Output() activateVariant = new EventEmitter<string>();

  readonly activeTab = signal<PickerTab>('photo');
  readonly promptId = signal('');
  readonly styleId = signal('');
  readonly photoId = signal('');
  readonly viewedIndex = signal(0);

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
  }

  selectedPhoto(): OrderPhotoDto | undefined {
    return this.photos.find((p) => p.id === this.photoId());
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
