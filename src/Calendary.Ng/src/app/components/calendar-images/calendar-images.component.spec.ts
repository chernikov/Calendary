import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CalendarImagesComponent } from './calendar-images.component';

describe('CalendarImagesComponent', () => {
  let component: CalendarImagesComponent;
  let fixture: ComponentFixture<CalendarImagesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CalendarImagesComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CalendarImagesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
