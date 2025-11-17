import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FluxModel } from '../../../../models/flux-model';
import { CalendarService } from '../../../../services/calendar.service';
import { Calendar } from '../../../../models/calendar';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { Router } from '@angular/router';
import { FluxModelService } from '../../../../services/flux-model.service';

@Component({
    standalone: true,
    selector: 'app-calendar-ready',
    imports: [CommonModule, MatCardModule, MatButtonModule],
    templateUrl: './calendar-ready.component.html',
    styleUrls: ['./calendar-ready.component.scss']
})
export class CalendarReadyComponent implements OnChanges {

  @Input()
  fluxModel: FluxModel | null = null;

  calendar: Calendar | null = null;

  constructor(
    private router: Router,
    private calendarService: CalendarService,
    private fluxModelService : FluxModelService)
  {
    this.init();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['fluxModel'] && changes['fluxModel'].currentValue) {
      this.fluxModel = changes['fluxModel'].currentValue;
    }
  }

  init() {
    this.calendarService.getCalendar().subscribe(
      (calendar: Calendar) => {
        this.calendar = calendar;
      }
    );
  }

  addToCart() {
    this.calendarService.addToCart(this.calendar!).subscribe({
      next: () => {
        this.fluxModelService.archive(this.fluxModel!.id).subscribe({
          next: () => {
            this.router.navigate(['/cart']);   
          },
          error: (error) => {
            console.error('Error archive flux model', error);
         }
        })
        
      },
      error: (error) => {
        console.error('Error adding to cart', error);
     }
    });
     
     
  }
}
