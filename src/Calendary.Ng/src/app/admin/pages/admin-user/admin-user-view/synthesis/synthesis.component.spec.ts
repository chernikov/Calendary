import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminUserSynthesisComponent } from './synthesis.component';

describe('AdminUserSynthesisComponent', () => {
  let component: AdminUserSynthesisComponent;
  let fixture: ComponentFixture<AdminUserSynthesisComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminUserSynthesisComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminUserSynthesisComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
