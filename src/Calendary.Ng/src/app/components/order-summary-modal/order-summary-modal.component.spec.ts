import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OrderSummaryModalComponent } from './order-summary-modal.component';

describe('OrderSummaryModalComponent', () => {
  let component: OrderSummaryModalComponent;
  let fixture: ComponentFixture<OrderSummaryModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OrderSummaryModalComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(OrderSummaryModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
