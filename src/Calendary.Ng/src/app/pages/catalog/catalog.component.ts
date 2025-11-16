import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, ParamMap, Router, RouterModule } from '@angular/router';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { Subject, takeUntil } from 'rxjs';
import { TemplateService } from '../../../services/template.service';
import { TemplateSummary } from '../../../models/template';
import {
  FilterBarComponent,
  CatalogFilters,
} from './components/filter-bar/filter-bar.component';
import { TemplateCardComponent } from './components/template-card/template-card.component';
import { TemplateCardSkeletonComponent } from './components/template-card-skeleton/template-card-skeleton.component';
import {
  TemplatePreviewDialogComponent,
  TemplatePreviewDialogData,
} from './components/template-preview-dialog/template-preview-dialog.component';

@Component({
  selector: 'app-catalog',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatProgressSpinnerModule,
    MatPaginatorModule,
    MatSnackBarModule,
    MatDialogModule,
    FilterBarComponent,
    TemplateCardComponent,
    TemplateCardSkeletonComponent,
  ],
  templateUrl: './catalog.component.html',
  styleUrl: './catalog.component.scss',
})
export class CatalogComponent implements OnInit, OnDestroy {
  templates: TemplateSummary[] = [];
  categories: string[] = [];
  readonly defaultMinPrice = 150;
  readonly defaultMaxPrice = 600;
  filters: CatalogFilters = {
    search: '',
    category: null,
    minPrice: this.defaultMinPrice,
    maxPrice: this.defaultMaxPrice,
  };
  loading = true;
  error = '';
  totalCount = 0;
  pageSize = 12;
  pageIndex = 0;
  readonly skeletons = Array.from({ length: 8 });

  private readonly destroy$ = new Subject<void>();
  private previewDialogRef?: MatDialogRef<TemplatePreviewDialogComponent>;

  constructor(
    private readonly templateService: TemplateService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly dialog: MatDialog,
    private readonly snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.templateService
      .getCategories()
      .pipe(takeUntil(this.destroy$))
      .subscribe((categories) => (this.categories = categories));

    this.route.queryParamMap.pipe(takeUntil(this.destroy$)).subscribe((params) => {
      this.applyParams(params);
      const previewId = params.get('preview');
      if (previewId) {
        this.openPreviewById(Number(previewId));
      } else {
        this.dialog.closeAll();
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onFiltersChange(value: CatalogFilters): void {
    this.updateQueryParams({
      search: value.search || null,
      category: value.category || null,
      minPrice: value.minPrice,
      maxPrice: value.maxPrice,
      page: 1,
    });
  }

  onPageChange(event: PageEvent): void {
    this.updateQueryParams({ page: event.pageIndex + 1, pageSize: event.pageSize });
  }

  openPreview(template: TemplateSummary): void {
    const currentPreviewId = Number(this.route.snapshot.queryParamMap.get('preview'));
    if (currentPreviewId === template.id) {
      this.openPreviewById(template.id);
      return;
    }

    this.updateQueryParams({ preview: template.id });
  }

  clearFilters(): void {
    this.updateQueryParams({
      search: null,
      category: null,
      minPrice: this.defaultMinPrice,
      maxPrice: this.defaultMaxPrice,
      page: 1,
    });
  }

  trackByTemplate(_: number, item: TemplateSummary): number {
    return item.id;
  }

  trackSkeleton(index: number): number {
    return index;
  }

  private applyParams(params: ParamMap): void {
    this.filters = {
      search: params.get('search') || '',
      category: params.get('category'),
      minPrice: params.has('minPrice') ? Number(params.get('minPrice')) : this.defaultMinPrice,
      maxPrice: params.has('maxPrice') ? Number(params.get('maxPrice')) : this.defaultMaxPrice,
    };
    this.pageIndex = Math.max(0, parseInt(params.get('page') || '1', 10) - 1);
    this.pageSize = parseInt(params.get('pageSize') || `${this.pageSize}`, 10);
    this.fetchTemplates();
  }

  private fetchTemplates(): void {
    this.loading = true;
    this.error = '';

    this.templateService
      .getTemplates({
        ...this.filters,
        page: this.pageIndex + 1,
        pageSize: this.pageSize,
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.templates = result.items;
          this.totalCount = result.totalCount;
          this.loading = false;
        },
        error: () => {
          this.templates = [];
          this.totalCount = 0;
          this.loading = false;
          this.error = 'Не вдалося завантажити каталог. Спробуйте пізніше.';
        },
      });
  }

  private openPreviewById(id: number): void {
    if (!id || Number.isNaN(id)) {
      return;
    }

    this.previewDialogRef?.close();

    this.templateService
      .getTemplate(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (template) => {
          const data: TemplatePreviewDialogData = { template };
          this.previewDialogRef = this.dialog.open(TemplatePreviewDialogComponent, {
            data,
            width: '720px',
          });

          this.previewDialogRef
            .afterClosed()
            .pipe(takeUntil(this.destroy$))
            .subscribe(() => {
              this.previewDialogRef = undefined;
              if (this.route.snapshot.queryParamMap.has('preview')) {
                this.updateQueryParams({ preview: null });
              }
            });
        },
        error: () => {
          this.snackBar.open('Не вдалося завантажити шаблон для перегляду', 'OK', { duration: 3000 });
          this.updateQueryParams({ preview: null });
        },
      });
  }

  private updateQueryParams(params: Record<string, any>): void {
    const queryParams = { ...this.route.snapshot.queryParams } as Record<string, any>;

    Object.entries(params).forEach(([key, value]) => {
      if (value === null || value === undefined || value === '') {
        delete queryParams[key];
      } else {
        queryParams[key] = value;
      }
    });

    this.router.navigate([], {
      relativeTo: this.route,
      queryParams,
      replaceUrl: true,
    });
  }

  get activeFilterLabels(): string[] {
    const labels: string[] = [];
    if (this.filters.search) {
      labels.push(`Пошук: "${this.filters.search}"`);
    }
    if (this.filters.category) {
      labels.push(`Категорія: ${this.filters.category}`);
    }
    if (
      (this.filters.minPrice ?? this.defaultMinPrice) !== this.defaultMinPrice ||
      (this.filters.maxPrice ?? this.defaultMaxPrice) !== this.defaultMaxPrice
    ) {
      labels.push(`Ціна: ${this.filters.minPrice}–${this.filters.maxPrice} ₴`);
    }
    return labels;
  }
}
