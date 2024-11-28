import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'ageGenderDisplay',
  standalone: true
})
export class AgeGenderDisplayPipe implements PipeTransform {
  private readonly ageGenderMap: { [key: number]: string } = {
    0: 'Чоловік',
    1: 'Жінка',
    2: 'Хлопчик (малюк)',
    3: 'Дівчинка (малюк)',
    4: 'Хлопчик',
    5: 'Дівчинка',
    6: 'Чоловік середнього віку',
    7: 'Жінка середнього віку',
    8: 'Чоловік похилого віку',
    9: 'Жінка похилого віку',
  };

  transform(value: number): string {
    return this.ageGenderMap[value] || 'Невідомо';
  }
}