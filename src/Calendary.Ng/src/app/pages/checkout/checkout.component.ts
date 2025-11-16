import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { forkJoin, Subject, takeUntil } from 'rxjs';
import { ContactFormComponent } from '../../components/checkout/contact-form/contact-form.component';
import { DeliveryFormComponent } from '../../components/checkout/delivery-form/delivery-form.component';
import { OrderSummaryComponent } from '../../components/checkout/order-summary/order-summary.component';
import { EmptyCartComponent } from '../../components/cart/empty-cart/empty-cart.component';
import { CartStore } from '../../store/cart.store';
import { Order } from '../../../models/order';
import { UserInfo } from '../../../models/user';
import { DeliveryInfo } from '../../../models/delivery-info';
import { NovaPostItem } from '../../../models/nova-post-item';
import { CartService } from '../../../services/cart.service';
import { UserService } from '../../../services/user.service';
import { VerificationService } from '../../../services/verification.service';
import { VerificationDialogComponent } from '../../components/verification-dialog/verification-dialog.component';
import { OrderSummaryModalComponent } from '../../components/order-summary-modal/order-summary-modal.component';
import { ConfirmationModalComponent } from '../../components/confirmation-modal/confirmation-modal.component';
import { PaymentService } from '../../../services/payment.service';

@Component({
    standalone: true,
    selector: 'app-checkout',
    imports: [CommonModule, ReactiveFormsModule, RouterModule, ContactFormComponent, DeliveryFormComponent, OrderSummaryComponent, EmptyCartComponent],
    templateUrl: './checkout.component.html',
    styleUrl: './checkout.component.scss'
})
export class CheckoutComponent implements OnInit, OnDestroy {
  contactForm: FormGroup;
  deliveryForm: FormGroup;

  order: Order | null = null;
  isSubmitting = false;
  isLoadingCart = true;
  errorMessage: string | null = null;
  isEmailConfirmed = false;
  isPhoneConfirmed = false;

  private destroy$ = new Subject<void>();

  constructor(
    private readonly fb: FormBuilder,
    private readonly cartStore: CartStore,
    private readonly cartService: CartService,
    private readonly userService: UserService,
    private readonly verificationService: VerificationService,
    private readonly paymentService: PaymentService,
    private readonly dialog: MatDialog,
    private readonly router: Router
  ) {
    this.contactForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required, Validators.pattern(/^\+?380\d{9}$/)]],
    });

    this.deliveryForm = this.fb.group({
      method: ['nova-poshta-warehouse', Validators.required],
      city: ['', Validators.required],
      cityRef: [''],
      warehouse: ['', Validators.required],
      warehouseRef: [''],
      address: this.fb.group({
        street: [''],
        building: [''],
        apartment: [''],
      }),
    });
  }

  ngOnInit(): void {
    this.cartStore.order$.pipe(takeUntil(this.destroy$)).subscribe((order) => {
      this.order = order;
    });

    this.cartStore.loading$.pipe(takeUntil(this.destroy$)).subscribe((loading) => {
      this.isLoadingCart = loading;
    });

    this.cartStore.refreshCart().subscribe({
      error: () => {
        this.isLoadingCart = false;
        this.errorMessage = 'Не вдалося завантажити кошик.';
      },
    });

    this.loadUserInfo();
    this.loadDeliveryInfo();
    this.setupDeliveryValidators();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadUserInfo(): void {
    this.userService.getInfo().subscribe((info) => {
      if (!info) {
        return;
      }
      const [firstName = '', ...rest] = (info.userName || '').split(' ');
      const lastName = rest.join(' ');
      this.contactForm.patchValue({
        firstName: firstName || info.userName,
        lastName: lastName || '',
        email: info.email,
        phone: info.phoneNumber,
      });
      this.isEmailConfirmed = info.isEmailConfirmed;
      this.isPhoneConfirmed = info.isPhoneNumberConfirmed;
    });
  }

  private loadDeliveryInfo(): void {
    this.cartService.getDeliveryInfo().subscribe((delivery) => {
      if (!delivery) {
        return;
      }
      const method = this.detectDeliveryMethod(delivery.postOffice);
      this.deliveryForm.patchValue({
        method,
        city: delivery.city.description,
        cityRef: delivery.city.ref,
        warehouse: delivery.postOffice.description,
        warehouseRef: delivery.postOffice.ref,
      });
    });
  }

  private detectDeliveryMethod(postOffice: NovaPostItem): string {
    if (!postOffice?.description) {
      return 'nova-poshta-warehouse';
    }
    const description = postOffice.description.toLowerCase();
    if (description.includes('кур')) {
      return 'nova-poshta-courier';
    }
    if (description.includes('самовивіз')) {
      return 'pickup';
    }
    return 'nova-poshta-warehouse';
  }

  private setupDeliveryValidators(): void {
    this.applyDeliveryValidators(this.deliveryForm.get('method')?.value);
    this.deliveryForm
      .get('method')
      ?.valueChanges.pipe(takeUntil(this.destroy$))
      .subscribe((method) => this.applyDeliveryValidators(method));
  }

  private applyDeliveryValidators(method: string): void {
    const warehouseControl = this.deliveryForm.get('warehouse');
    const addressGroup = this.deliveryForm.get('address') as FormGroup;
    const street = addressGroup.get('street');
    const building = addressGroup.get('building');

    if (method === 'nova-poshta-warehouse') {
      warehouseControl?.setValidators([Validators.required]);
      street?.clearValidators();
      building?.clearValidators();
    } else if (method === 'nova-poshta-courier') {
      warehouseControl?.clearValidators();
      street?.setValidators([Validators.required]);
      building?.setValidators([Validators.required]);
    } else {
      warehouseControl?.clearValidators();
      street?.clearValidators();
      building?.clearValidators();
    }

    warehouseControl?.updateValueAndValidity({ emitEvent: false });
    street?.updateValueAndValidity({ emitEvent: false });
    building?.updateValueAndValidity({ emitEvent: false });
  }

  onSubmit(): void {
    if (!this.order) {
      return;
    }
    if (this.contactForm.invalid || this.deliveryForm.invalid) {
      this.contactForm.markAllAsTouched();
      this.deliveryForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = null;

    const contactPayload = this.buildContactPayload();
    const deliveryPayload = this.buildDeliveryPayload();

    forkJoin([
      this.userService.updateInfo(contactPayload),
      this.cartService.updateDeliveryInfo(deliveryPayload),
    ])
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => this.showOrderSummary(),
        error: (error) => {
          console.error(error);
          this.errorMessage = 'Не вдалося зберегти дані. Спробуйте ще раз.';
          this.isSubmitting = false;
        },
      });
  }

  private buildContactPayload(): UserInfo {
    const payload = new UserInfo();
    payload.userName = `${this.contactForm.value.firstName} ${this.contactForm.value.lastName}`.trim();
    payload.email = this.contactForm.value.email;
    payload.phoneNumber = this.contactForm.value.phone;
    return payload;
  }

  private buildDeliveryPayload(): DeliveryInfo {
    const payload = new DeliveryInfo();
    payload.city.description = this.deliveryForm.value.city;
    payload.city.ref = this.deliveryForm.value.cityRef;

    if (this.deliveryForm.value.method === 'nova-poshta-courier') {
      const address = this.deliveryForm.value.address;
      payload.postOffice.description = `Кур'єр: ${address.street || ''} ${address.building || ''}`.trim();
    } else if (this.deliveryForm.value.method === 'pickup') {
      payload.postOffice.description = 'Самовивіз з шоуруму Calendary';
    } else {
      payload.postOffice.description = this.deliveryForm.value.warehouse;
      payload.postOffice.ref = this.deliveryForm.value.warehouseRef;
    }
    return payload;
  }

  private showOrderSummary(): void {
    this.cartService.summary().subscribe({
      next: (summary) => {
        if (!summary.user.isEmailConfirmed || !summary.user.isPhoneNumberConfirmed) {
          this.dialog.open(ConfirmationModalComponent, { width: '400px' });
          this.isSubmitting = false;
          return;
        }
        const dialogRef = this.dialog.open(OrderSummaryModalComponent, {
          width: '600px',
          data: summary,
        });
        dialogRef.afterClosed().subscribe((result) => {
          if (result === 'confirm') {
            this.redirectToPayment();
          } else {
            this.isSubmitting = false;
          }
        });
      },
      error: (error) => {
        console.error('Failed to load summary', error);
        this.errorMessage = 'Не вдалося завантажити підсумок замовлення.';
        this.isSubmitting = false;
      },
    });
  }

  private redirectToPayment(): void {
    this.paymentService.getPay().subscribe({
      next: (redirect) => {
        window.location.href = redirect.paymentPage;
      },
      error: (error) => {
        console.error('Failed to redirect to payment', error);
        this.isSubmitting = false;
      },
    });
  }

  requestEmailVerification(): void {
    if (this.contactForm.get('email')?.invalid) {
      this.contactForm.get('email')?.markAsTouched();
      return;
    }
    this.verificationService.sendEmailVerification().subscribe(() => {
      this.openVerificationModal('email');
    });
  }

  requestPhoneVerification(): void {
    if (this.contactForm.get('phone')?.invalid) {
      this.contactForm.get('phone')?.markAsTouched();
      return;
    }
    this.verificationService.sendPhoneVerification().subscribe(() => {
      this.openVerificationModal('phone');
    });
  }

  private openVerificationModal(type: 'email' | 'phone'): void {
    const dialogRef = this.dialog.open(VerificationDialogComponent, {
      width: '400px',
      data: { type },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.loadUserInfo();
      }
    });
  }

  backToCart(): void {
    this.router.navigate(['/cart']);
  }
}
