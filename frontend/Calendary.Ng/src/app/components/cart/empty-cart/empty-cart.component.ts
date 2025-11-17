import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-empty-cart',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './empty-cart.component.html',
  styleUrl: './empty-cart.component.scss',
})
export class EmptyCartComponent {
  @Input() title = 'Ваш кошик порожній';
  @Input() description = 'Додайте шаблон з каталогу, щоб розпочати оформлення замовлення.';
  @Input() actionLabel = 'Перейти до каталогу';
  @Input() actionLink: string | string[] = '/catalog';
}
