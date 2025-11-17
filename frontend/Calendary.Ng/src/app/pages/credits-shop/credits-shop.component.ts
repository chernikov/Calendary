import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CreditService, CreditPackage } from '../../../services/credit.service';

@Component({
  selector: 'app-credits-shop',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="credits-shop-container">
      <div class="shop-header">
        <h1>Купити кредити</h1>
        <p class="subtitle">Виберіть пакет кредитів для генерації AI контенту</p>
      </div>

      <div class="packages-grid" *ngIf="packages.length > 0">
        <div class="package-card"
             *ngFor="let package of packages"
             [class.popular]="package.name === 'Premium'"
             (click)="selectPackage(package)">

          <div class="popular-badge" *ngIf="package.name === 'Premium'">
            ⭐ Популярний вибір
          </div>

          <div class="package-name">{{ package.name }}</div>

          <div class="package-credits">
            <span class="credits-amount">{{ package.credits }}</span>
            <span class="credits-label">кредитів</span>
          </div>

          <div class="bonus-badge" *ngIf="package.bonusCredits > 0">
            +{{ package.bonusCredits }} бонусних кредитів
            <span class="bonus-percent">({{ calculateBonus(package) }}% бонус)</span>
          </div>

          <div class="package-price">
            <span class="price">{{ package.priceUAH }}</span>
            <span class="currency">грн</span>
          </div>

          <div class="package-description" *ngIf="package.description">
            {{ package.description }}
          </div>

          <div class="credit-examples">
            <div class="example">
              ✓ {{ calculateModels(package) }} моделей
            </div>
            <div class="example">
              ✓ {{ calculateImages(package) }} фото (Flux)
            </div>
          </div>

          <button class="btn-purchase"
                  [disabled]="purchasing === package.id"
                  (click)="purchasePackage(package); $event.stopPropagation()">
            <span *ngIf="purchasing !== package.id">Купити</span>
            <span *ngIf="purchasing === package.id">Обробка...</span>
          </button>
        </div>
      </div>

      <div class="info-section">
        <h3>Що таке кредити?</h3>
        <p>Кредити - це внутрішня валюта платформи для оплати AI-генерації:</p>
        <ul>
          <li><strong>Створення моделі:</strong> 145 кредитів</li>
          <li><strong>Генерація фото (Flux):</strong> 14 кредитів</li>
          <li><strong>Генерація фото (NanoBanana):</strong> 3 кредити</li>
        </ul>
        <p class="note">Куплені кредити не мають терміну дії. Бонусні кредити дійсні 12 місяців.</p>
      </div>
    </div>
  `,
  styles: [`
    .credits-shop-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 40px 20px;
    }

    .shop-header {
      text-align: center;
      margin-bottom: 48px;
    }

    .shop-header h1 {
      font-size: 36px;
      font-weight: bold;
      margin-bottom: 12px;
    }

    .subtitle {
      font-size: 18px;
      color: #666;
    }

    .packages-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
      gap: 24px;
      margin-bottom: 48px;
    }

    .package-card {
      position: relative;
      background: white;
      border: 2px solid #e5e7eb;
      border-radius: 16px;
      padding: 32px 24px;
      cursor: pointer;
      transition: all 0.3s;
      text-align: center;
    }

    .package-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 12px 24px rgba(0, 0, 0, 0.1);
      border-color: #2563eb;
    }

    .package-card.popular {
      border-color: #2563eb;
      box-shadow: 0 8px 16px rgba(37, 99, 235, 0.1);
    }

    .popular-badge {
      position: absolute;
      top: -12px;
      left: 50%;
      transform: translateX(-50%);
      background: #2563eb;
      color: white;
      padding: 6px 16px;
      border-radius: 20px;
      font-size: 12px;
      font-weight: 600;
    }

    .package-name {
      font-size: 24px;
      font-weight: bold;
      margin-bottom: 16px;
      color: #111;
    }

    .package-credits {
      margin-bottom: 12px;
    }

    .credits-amount {
      font-size: 48px;
      font-weight: bold;
      color: #2563eb;
    }

    .credits-label {
      font-size: 16px;
      color: #666;
      margin-left: 8px;
    }

    .bonus-badge {
      background: #dcfce7;
      color: #166534;
      padding: 8px 16px;
      border-radius: 8px;
      font-size: 14px;
      font-weight: 600;
      margin-bottom: 16px;
      display: inline-block;
    }

    .bonus-percent {
      font-size: 12px;
      opacity: 0.8;
    }

    .package-price {
      margin-bottom: 16px;
    }

    .price {
      font-size: 32px;
      font-weight: bold;
      color: #111;
    }

    .currency {
      font-size: 18px;
      color: #666;
      margin-left: 4px;
    }

    .package-description {
      font-size: 14px;
      color: #666;
      margin-bottom: 16px;
    }

    .credit-examples {
      background: #f3f4f6;
      padding: 16px;
      border-radius: 8px;
      margin-bottom: 20px;
      text-align: left;
    }

    .credit-examples .example {
      font-size: 14px;
      color: #374151;
      margin-bottom: 8px;
    }

    .credit-examples .example:last-child {
      margin-bottom: 0;
    }

    .btn-purchase {
      width: 100%;
      padding: 14px;
      background: #2563eb;
      color: white;
      border: none;
      border-radius: 8px;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      transition: background 0.2s;
    }

    .btn-purchase:hover:not(:disabled) {
      background: #1d4ed8;
    }

    .btn-purchase:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .info-section {
      background: #f9fafb;
      padding: 32px;
      border-radius: 12px;
      margin-top: 48px;
    }

    .info-section h3 {
      font-size: 24px;
      margin-bottom: 16px;
    }

    .info-section ul {
      list-style: none;
      padding: 0;
      margin: 16px 0;
    }

    .info-section li {
      padding: 8px 0;
      font-size: 16px;
    }

    .info-section .note {
      font-size: 14px;
      color: #666;
      font-style: italic;
      margin-top: 16px;
    }
  `]
})
export class CreditsShopComponent implements OnInit {
  packages: CreditPackage[] = [];
  purchasing: number | null = null;

  constructor(private creditService: CreditService) {}

  ngOnInit() {
    this.loadPackages();
  }

  loadPackages() {
    this.creditService.getPackages().subscribe({
      next: (packages) => {
        this.packages = packages;
      },
      error: (err) => {
        console.error('Failed to load credit packages', err);
      }
    });
  }

  selectPackage(pkg: CreditPackage) {
    console.log('Selected package:', pkg);
  }

  purchasePackage(pkg: CreditPackage) {
    this.purchasing = pkg.id;

    this.creditService.purchasePackage(pkg.id).subscribe({
      next: (response) => {
        // Redirect to Monobank payment page
        window.location.href = response.paymentUrl;
      },
      error: (err) => {
        console.error('Purchase failed', err);
        this.purchasing = null;
        alert('Помилка при створенні платежу. Спробуйте пізніше.');
      }
    });
  }

  calculateBonus(pkg: CreditPackage): number {
    if (pkg.credits === 0) return 0;
    return Math.round((pkg.bonusCredits / pkg.credits) * 100);
  }

  calculateModels(pkg: CreditPackage): number {
    const totalCredits = pkg.credits + pkg.bonusCredits;
    return Math.floor(totalCredits / 145);
  }

  calculateImages(pkg: CreditPackage): number {
    const totalCredits = pkg.credits + pkg.bonusCredits;
    return Math.floor(totalCredits / 14);
  }
}
