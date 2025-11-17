import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewFluxModelComponent } from './view-flux-model.component';

describe('ViewFluxModelComponent', () => {
  let component: ViewFluxModelComponent;
  let fixture: ComponentFixture<ViewFluxModelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ViewFluxModelComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ViewFluxModelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
