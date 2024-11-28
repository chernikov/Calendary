import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FluxModelComponent } from './flux-model.component';

describe('FluxModelComponent', () => {
  let component: FluxModelComponent;
  let fixture: ComponentFixture<FluxModelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FluxModelComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FluxModelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
