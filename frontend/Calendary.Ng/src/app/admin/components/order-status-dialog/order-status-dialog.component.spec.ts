import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OrderStatusDialogComponent } from './order-status-dialog.component';

describe('OrderStatusDialogComponent', () => {
  let component: OrderStatusDialogComponent;
  let fixture: ComponentFixture<OrderStatusDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OrderStatusDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(OrderStatusDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
