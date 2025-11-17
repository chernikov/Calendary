import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { of } from 'rxjs';
import { CartComponent } from './cart.component';
import { CartStore } from '../../store/cart.store';
import { OrderService } from '../../../services/order.service';

class CartStoreStub {
  order$ = of(null);
  loading$ = of(false);
  refreshCart() {
    return of(null);
  }
  updateItemQuantity() {
    return of(void 0);
  }
  removeItem() {
    return of(void 0);
  }
}

describe('CartComponent', () => {
  let component: CartComponent;
  let fixture: ComponentFixture<CartComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CartComponent, RouterTestingModule],
      providers: [
        { provide: CartStore, useClass: CartStoreStub },
        { provide: OrderService, useValue: { updateComment: () => of({}) } },
      ],
    })
    .compileComponents();

    fixture = TestBed.createComponent(CartComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
