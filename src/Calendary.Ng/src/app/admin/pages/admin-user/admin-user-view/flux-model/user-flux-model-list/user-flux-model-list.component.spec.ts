import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserFluxModelListComponent } from './user-flux-model-list.component';

describe('UserFluxModelListComponent', () => {
  let component: UserFluxModelListComponent;
  let fixture: ComponentFixture<UserFluxModelListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserFluxModelListComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserFluxModelListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
