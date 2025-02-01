import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminUserViewComponent } from './admin-user-view.component';

describe('AdminUserViewComponent', () => {
  let component: AdminUserViewComponent;
  let fixture: ComponentFixture<AdminUserViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminUserViewComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminUserViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
