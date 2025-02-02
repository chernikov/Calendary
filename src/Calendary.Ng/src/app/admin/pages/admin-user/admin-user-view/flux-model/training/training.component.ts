import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-training',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule],
  templateUrl: './training.component.html',
  styleUrls: ['./training.component.scss']
})
export class TrainingComponent implements OnInit {
  trainingId!: number;

  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
    // Отримання id тренування з параметрів маршруту
    this.trainingId = +this.route.snapshot.paramMap.get('id')!;
    // Тут можна завантажити детальну інформацію про тренування
  }
}