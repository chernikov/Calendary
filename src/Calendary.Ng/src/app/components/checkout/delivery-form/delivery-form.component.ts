import { CommonModule } from '@angular/common';
import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, of, switchMap, takeUntil } from 'rxjs';
import { NovaPostItem } from '../../../../models/nova-post-item';
import { NovaPostService } from '../../../../services/novapost.service';

@Component({
  selector: 'app-delivery-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './delivery-form.component.html',
  styleUrl: './delivery-form.component.scss',
})
export class DeliveryFormComponent implements OnInit, OnDestroy {
  @Input({ required: true }) form!: FormGroup;

  cityOptions: NovaPostItem[] = [];
  warehouseOptions: NovaPostItem[] = [];

  private destroy$ = new Subject<void>();

  constructor(private readonly novaPostService: NovaPostService) {}

  ngOnInit(): void {
    const cityControl = this.form.get('city');
    const warehouseControl = this.form.get('warehouse');

    cityControl?.valueChanges
      .pipe(
        takeUntil(this.destroy$),
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((value: string) => {
          if (!value || value.length < 2) {
            this.cityOptions = [];
            return of([]);
          }
          return this.novaPostService.searchCity(value);
        })
      )
      .subscribe((options) => {
        if (Array.isArray(options)) {
          this.cityOptions = options;
        }
      });

    warehouseControl?.valueChanges
      .pipe(
        takeUntil(this.destroy$),
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((value: string) => {
          const method = this.form.get('method')?.value;
          const city = this.form.get('city')?.value;
          if (method !== 'nova-poshta-warehouse' || !city || value?.length < 1) {
            this.warehouseOptions = [];
            return of([]);
          }
          return this.novaPostService.searchWarehouse(city, value);
        })
      )
      .subscribe((options) => {
        if (Array.isArray(options)) {
          this.warehouseOptions = options;
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  selectCity(option: NovaPostItem): void {
    this.form.patchValue({
      city: option.description,
      cityRef: option.ref,
      warehouse: '',
      warehouseRef: '',
    });
    this.cityOptions = [];
  }

  selectWarehouse(option: NovaPostItem): void {
    this.form.patchValue({
      warehouse: option.description,
      warehouseRef: option.ref,
    });
    this.warehouseOptions = [];
  }

  shouldShowWarehouse(): boolean {
    return this.form.get('method')?.value === 'nova-poshta-warehouse';
  }

  shouldShowCourierAddress(): boolean {
    return this.form.get('method')?.value === 'nova-poshta-courier';
  }
}
