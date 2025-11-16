import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { debounceTime } from 'rxjs/operators';

export interface CatalogFilters {
  search?: string;
  category?: string | null;
  minPrice?: number;
  maxPrice?: number;
}

@Component({
  selector: 'app-catalog-filter-bar',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
  ],
  templateUrl: './filter-bar.component.html',
  styleUrl: './filter-bar.component.scss',
})
export class FilterBarComponent implements OnChanges {
  @Input() categories: string[] = [];
  @Input() value: CatalogFilters = {};
  @Output() valueChange = new EventEmitter<CatalogFilters>();

  form: FormGroup;
  readonly minLimit = 150;
  readonly maxLimit = 600;

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      search: [''],
      category: [''],
      minPrice: [this.minLimit],
      maxPrice: [this.maxLimit],
    });

    this.form.valueChanges
      .pipe(debounceTime(300))
      .subscribe((filters) => {
        const normalized: CatalogFilters = {
          search: filters.search,
          category: filters.category || null,
          minPrice: filters.minPrice,
          maxPrice: filters.maxPrice,
        };

        this.valueChange.emit(normalized);
      });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['value'] && this.value) {
      this.form.patchValue(
        {
          search: this.value.search || '',
          category: this.value.category || '',
          minPrice: this.value.minPrice ?? this.minLimit,
          maxPrice: this.value.maxPrice ?? this.maxLimit,
        },
        { emitEvent: false }
      );
    }
  }

  resetFilters(): void {
    this.form.setValue({
      search: '',
      category: '',
      minPrice: this.minLimit,
      maxPrice: this.maxLimit,
    });
    this.valueChange.emit({
      search: '',
      category: null,
      minPrice: this.minLimit,
      maxPrice: this.maxLimit,
    });
  }

  clampPrices(): void {
    const min = Math.max(this.minLimit, this.form.value.minPrice || this.minLimit);
    const max = Math.min(this.maxLimit, this.form.value.maxPrice || this.maxLimit);
    if (min > max) {
      this.form.patchValue({ minPrice: max, maxPrice: min }, { emitEvent: false });
    }
  }
}
