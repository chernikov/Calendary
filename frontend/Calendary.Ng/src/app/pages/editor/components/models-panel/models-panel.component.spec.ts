import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ModelsPanelComponent } from './models-panel.component';

describe('ModelsPanelComponent', () => {
  let component: ModelsPanelComponent;
  let fixture: ComponentFixture<ModelsPanelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ModelsPanelComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ModelsPanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
