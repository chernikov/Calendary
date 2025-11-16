import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CreditService, CreditBalance } from '../../../services/credit.service';

@Component({
  selector: 'app-credit-balance',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="credit-balance-widget">
      <div class="balance-header">
        <span class="icon">💳</span>
        <span class="label">Мої кредити</span>
      </div>

      <div class="balance-amount" *ngIf="balance">
        <span class="total">{{ balance.total }}</span>
        <span class="suffix">кредитів</span>
      </div>

      <div class="balance-details" *ngIf="balance">
        <div class="detail">
          <span class="label">Куплені:</span>
          <span class="value">{{ balance.purchased }}</span>
        </div>
        <div class="detail">
          <span class="label">Бонусні:</span>
          <span class="value">{{ balance.bonus }}</span>
        </div>
      </div>

      <div class="expiring-warning" *ngIf="balance && balance.expiringIn30Days > 0">
        ⚠️ {{ balance.expiringIn30Days }} кредитів згорять через 30 днів
      </div>

      <button class="btn-buy-credits" (click)="navigateToShop()">
        Купити кредити
      </button>
    </div>
  `,
  styles: [`
    .credit-balance-widget {
      background: white;
      border-radius: 12px;
      padding: 20px;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    }

    .balance-header {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-bottom: 16px;
      font-size: 14px;
      color: #666;
    }

    .balance-amount {
      display: flex;
      align-items: baseline;
      gap: 8px;
      margin-bottom: 16px;
    }

    .balance-amount .total {
      font-size: 36px;
      font-weight: bold;
      color: #2563eb;
    }

    .balance-amount .suffix {
      font-size: 16px;
      color: #666;
    }

    .balance-details {
      display: flex;
      flex-direction: column;
      gap: 8px;
      margin-bottom: 16px;
      padding: 12px;
      background: #f3f4f6;
      border-radius: 8px;
    }

    .balance-details .detail {
      display: flex;
      justify-content: space-between;
      font-size: 14px;
    }

    .balance-details .label {
      color: #666;
    }

    .balance-details .value {
      font-weight: 600;
      color: #111;
    }

    .expiring-warning {
      padding: 12px;
      background: #fef3c7;
      border-radius: 8px;
      font-size: 13px;
      color: #92400e;
      margin-bottom: 16px;
    }

    .btn-buy-credits {
      width: 100%;
      padding: 12px;
      background: #2563eb;
      color: white;
      border: none;
      border-radius: 8px;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      transition: background 0.2s;
    }

    .btn-buy-credits:hover {
      background: #1d4ed8;
    }
  `]
})
export class CreditBalanceComponent implements OnInit {
  balance: CreditBalance | null = null;

  constructor(
    private creditService: CreditService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadBalance();
  }

  loadBalance() {
    this.creditService.getBalance().subscribe({
      next: (balance) => {
        this.balance = balance;
      },
      error: (err) => {
        console.error('Failed to load credit balance', err);
      }
    });
  }

  navigateToShop() {
    this.router.navigate(['/credits/shop']);
  }
}
