import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { OrderItem } from '../../../../../models/order-item';

@Component({
  selector: 'app-cart-item',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './cart-item.component.html',
  styleUrl: './cart-item.component.scss',
})
export class CartItemComponent implements OnChanges {
  @Input({ required: true }) item!: OrderItem;
  @Input() disabled = false;

  @Output() quantityChange = new EventEmitter<number>();
  @Output() remove = new EventEmitter<void>();
  @Output() preview = new EventEmitter<string>();

  quantity = 1;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['item'] && this.item) {
      this.quantity = this.item.quantity;
    }
  }

  handleQuantityInput(value: string): void {
    const parsed = Number(value);
    if (!Number.isFinite(parsed)) {
      return;
    }
    const next = Math.max(1, Math.floor(parsed));
    this.quantity = next;
  }

  submitQuantityChange(): void {
    if (!this.item || this.quantity === this.item.quantity) {
      return;
    }
    this.quantityChange.emit(this.quantity);
  }

  requestPreview(): void {
    if (this.item?.calendar?.previewPath) {
      this.preview.emit(this.item.calendar.previewPath);
    }
  }
}
