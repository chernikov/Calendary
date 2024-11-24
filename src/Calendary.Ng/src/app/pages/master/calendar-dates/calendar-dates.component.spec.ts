import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CalendarDatesComponent } from './calendar-dates.component';

describe('CalendarDatesComponent', () => {
  let component: CalendarDatesComponent;
  let fixture: ComponentFixture<CalendarDatesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CalendarDatesComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CalendarDatesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
