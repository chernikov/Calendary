import { OrderStatus } from './models';

const LABELS: Record<OrderStatus, string> = {
  Created: 'Чернетка',
  PhotoUploaded: 'Фото завантажено',
  DetailsSubmitted: 'Деталі збережено',
  Generating: 'Генеруємо',
  CoverReady: 'Обкладинка готова',
  CoverConfirmed: 'Обкладинку підтверджено',
  ReviewReady: 'Готово до перегляду',
  AwaitingPayment: 'Очікує оплати',
  Paid: 'Оплачено',
  Printing: 'Друкуємо',
  Shipped: 'Відправлено',
  Delivered: 'Доставлено',
  Cancelled: 'Скасовано',
  GenerationFailed: 'Помилка генерації',
};

/// Where the customer left off — the step each status should resume at.
const STEP: Record<OrderStatus, string[]> = {
  Created: ['upload'],
  PhotoUploaded: ['style'],
  DetailsSubmitted: ['generating'],
  Generating: ['generating'],
  GenerationFailed: ['generating'],
  // Cover-confirm/month-by-month/review are no longer part of the primary flow — both statuses
  // resolve on the generating page, which now owns the "generate watermarked PDF" action.
  CoverReady: ['generating'],
  CoverConfirmed: ['generating'],
  ReviewReady: ['checkout'],
  AwaitingPayment: ['checkout'],
  Paid: ['status'],
  Printing: ['status'],
  Shipped: ['status'],
  Delivered: ['status'],
  Cancelled: ['status'],
};

const DONE: OrderStatus[] = ['Paid', 'Printing', 'Shipped', 'Delivered'];
const FAILED: OrderStatus[] = ['Cancelled', 'GenerationFailed'];

export function orderStatusLabel(status: OrderStatus): string {
  return LABELS[status] ?? status;
}

export function orderStatusTagClass(status: OrderStatus): string {
  if (FAILED.includes(status)) return 'tag tag-accent-2';
  if (DONE.includes(status)) return 'tag tag-accent';
  return 'tag tag-neutral';
}

export function orderStepLink(orderId: string, status: OrderStatus): unknown[] {
  return ['/order', orderId, ...STEP[status]];
}

export function isOrderInProgress(status: OrderStatus): boolean {
  return !DONE.includes(status) && !FAILED.includes(status);
}
