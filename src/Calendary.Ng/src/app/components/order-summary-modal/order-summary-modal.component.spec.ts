import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { SummaryOrder } from '../../../models/summary-order';
import { OrderSummaryModalComponent } from './order-summary-modal.component';

describe('OrderSummaryModalComponent', () => {
  let component: OrderSummaryModalComponent;
  let fixture: ComponentFixture<OrderSummaryModalComponent>;

  const order = new SummaryOrder();
  order.user.userName = 'Test User';

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OrderSummaryModalComponent],
      providers: [{ provide: MAT_DIALOG_DATA, useValue: order }],
    }).compileComponents();

    fixture = TestBed.createComponent(OrderSummaryModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
