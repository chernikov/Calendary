import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdditionalCalendarSettingsComponent } from './additional-calendar-settings.component';

describe('AdditionalCalendarSettingsComponent', () => {
  let component: AdditionalCalendarSettingsComponent;
  let fixture: ComponentFixture<AdditionalCalendarSettingsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdditionalCalendarSettingsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdditionalCalendarSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
