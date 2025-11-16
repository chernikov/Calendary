import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Holiday } from '@models/holiday';
import { HolidayService } from '@services/holiday.service';


@Component({
    standalone: true,
    selector: 'app-holidays',
    imports: [CommonModule, FormsModule],
    templateUrl: './holiday.component.html',
    styleUrls: ['./holiday.component.scss']
})
export class HolidayComponent implements OnInit {
  holidays: Holiday[] = [];
  newHoliday: Holiday = new Holiday();

  constructor(private holidayService: HolidayService) {}

  ngOnInit(): void {
    this.loadHolidays();
  }

  // Метод для завантаження всіх свят
  loadHolidays(): void {
    this.holidayService.getHolidays().subscribe(
      (data: Holiday[]) => {
        this.holidays = this.sortHolidaysByMonthAndDay(data);
      },
      (error) => {
        console.error('Error loading holidays', error);
      }
    );
  }

  // Метод для створення нового свята
  addHoliday(newHoliday: Holiday): void {
    this.holidayService.createHoliday(newHoliday).subscribe(
      (holiday: Holiday) => {
        this.holidays.push(holiday);
        this.newHoliday = new Holiday(); // Очищення форми після додавання
      },
      (error) => {
        console.error('Error creating holiday', error);
      }
    );
  }

  // Метод для оновлення свята
  updateHoliday(id: number, updatedHoliday: Holiday): void {
    this.holidayService.updateHoliday(updatedHoliday).subscribe(
      (holiday: Holiday) => {
        const index = this.holidays.findIndex(h => h.id === id);
        if (index !== -1) {
          this.holidays[index] = holiday;
        }
      },
      (error) => {
        console.error('Error updating holiday', error);
      }
    );
  }

  // Метод для видалення свята
  deleteHoliday(id: number): void {
    this.holidayService.deleteHoliday(id).subscribe(
      () => {
        this.holidays = this.holidays.filter(h => h.id !== id);
      },
      (error) => {
        console.error('Error deleting holiday', error);
      }
    );
  }

  sortHolidaysByMonthAndDay(holidays: Holiday[]): Holiday[] {
    return holidays.sort((a, b) => {
      const aDate = new Date(a.date);
      const bDate = new Date(b.date);

      // Сортуємо за місяцем і днем, ігноруючи рік
      if (aDate.getMonth() !== bDate.getMonth()) {
        return aDate.getMonth() - bDate.getMonth();
      }
      return aDate.getDate() - bDate.getDate();
    });
  }
}
