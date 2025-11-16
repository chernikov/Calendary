import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.scss',
})
export class FooterComponent {
  readonly year = new Date().getFullYear();

  readonly quickLinks = [
    { label: 'Каталог шаблонів', href: '/master' },
    { label: 'Редактор', href: '/editor' },
    { label: 'Кошик', href: '/cart' },
  ];
}
