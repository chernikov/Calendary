import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { PaymentService } from '../../../../services/payment.service';
import { FluxModel } from '../../../../models/flux-model';
import { PaymentRedirect } from '../../../../models/payment-redirect';
import { MatButtonModule } from '@angular/material/button';

@Component({
    selector: 'app-payment',
    imports: [MatButtonModule],
    templateUrl: './payment.component.html',
    styleUrl: './payment.component.scss'
})
export class PaymentComponent implements OnChanges {
  @Input() fluxModel: FluxModel | null = null;
 
  constructor(private paymentService: PaymentService) {
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['fluxModel'] && changes['fluxModel'].currentValue) {
      this.fluxModel = changes['fluxModel'].currentValue;
    }
  }

  confirmPayment() {
    if (!this.fluxModel) {
      return;
    }
    this.paymentService.getFluxModelPay(this.fluxModel.id).subscribe({
      next: (redirect) => {
        this.redirectToPayment(redirect);
      },
      error: (error) => {
        console.error('Failed to get payment redirect:', error);
      }
    });
  }
  
  redirectToPayment(redirect: PaymentRedirect) {
    window.location.href = redirect.paymentPage;
  }
}
