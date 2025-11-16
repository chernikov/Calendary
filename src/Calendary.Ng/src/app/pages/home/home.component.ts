import { CommonModule, NgOptimizedImage } from '@angular/common';
import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CtaButtonComponent } from '@ui/cta-button/cta-button.component';
import { SectionComponent } from '@ui/section/section.component';
import { FeatureCard, FeatureGridComponent } from '@ui/feature-grid/feature-grid.component';

@Component({
    standalone: true,
    selector: 'app-home',
    imports: [
      CommonModule,
      RouterLink,
      NgOptimizedImage,
      CtaButtonComponent,
      SectionComponent,
      FeatureGridComponent,
    ],
    templateUrl: './home.component.html',
    styleUrl: './home.component.scss'
})
export class HomeComponent {
  readonly features: FeatureCard[] = [
    {
      title: 'AI-генерація стилізованих зображень',
      description: 'Replicate моделі допомагають створити 12 ілюстрацій у вибраному стилі без складних налаштувань.',
      icon: '✨',
      accent: 'primary',
    },
    {
      title: 'Drag & Drop редактор',
      description: 'Розміщуйте ілюстрації по місяцях, додавайте важливі дати й одразу бачте результат.',
      icon: '🗓️',
      accent: 'neutral',
    },
    {
      title: 'Друк та доставка в Україні',
      description: 'Готовий PDF надсилається у друкарню, а готове замовлення доставляє Нова Пошта.',
      icon: '📦',
      accent: 'accent',
    },
    {
      title: 'Особистий кабінет',
      description: 'Зберігайте дизайн, повторюйте замовлення та відстежуйте статуси оплати й доставки.',
      icon: '🔐',
      accent: 'neutral',
    },
  ];

  readonly workflowSteps = [
    {
      title: 'Завантажте або згенеруйте зображення',
      description: 'Підкажіть AI бажаний стиль або завантажте власні фотографії та застосуйте швидкі фільтри.',
    },
    {
      title: 'Розмістіть моменти по місяцях',
      description: 'Перетягуйте ілюстрації, додавайте нагадування й кольорові мітки подій у календарній сітці.',
    },
    {
      title: 'Оформіть замовлення',
      description: 'Додайте календар до кошика, оберіть доставку та оплату через MonoBank або банківську карту.',
    },
  ];

  constructor(private router: Router) { }

  goToMaster() {
    this.router.navigate(['/master']).then(() => {
      window.location.reload();
    });
  }
}

