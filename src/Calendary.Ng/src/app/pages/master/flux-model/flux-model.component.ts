import { Component, EventEmitter, Output } from '@angular/core';
import { FluxModelService } from '../../../../services/flux-model.service';
import { FluxModel } from '../../../../models/flux-model';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-flux-model',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './flux-model.component.html',
  styleUrl: './flux-model.component.scss'
})
export class FluxModelComponent {
  ageGender: number | null = null;
  fluxModel: FluxModel | null = null;

  @Output() 
  onUpdate = new EventEmitter<FluxModel>();

  constructor(private fluxModelService: FluxModelService) {
    this.init();
  }

  init() {
    this.fluxModelService.current().subscribe({
      next: (model) => {
        this.fluxModel = model;
        console.log('Поточний FluxModel:', model);
        this.onUpdate.emit(model);
      },
      error: (err) => {
        console.error('Помилка завантаження поточного FluxModel:', err);
      }
    });
  }

  createFluxModel(): void {
    if (!this.ageGender) 
    {
      alert('Оберіть категорію.');
      return;
    }

    this.fluxModelService.create(this.ageGender).subscribe({
      next: (model) => {
        this.fluxModel = model;
        this.onUpdate.emit(model);
        console.log('FluxModel створено:', model);
      },
      error: (err) => {
        console.error('Помилка створення FluxModel:', err);
      }
    });
  }
}