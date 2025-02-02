import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { UserSynthesisService } from '../../../../../../services/admin/user-synthesis.service';
import { Synthesis } from '../../../../../../models/synthesis';
import { Training } from '../../../../../../models/training';
import { UserTrainingService } from '../../../../../../services/admin/user-training.service';
import { FullScreenPhotoComponent } from '../../../../components/full-screen-photo.component';
import { MatDialog } from '@angular/material/dialog';


@Component({
  selector: 'app-synthesis',
  standalone: true,
  imports: [CommonModule, MatTableModule],
  templateUrl: './synthesis.component.html',
  styleUrls: ['./synthesis.component.scss']
})
export class AdminUserSynthesisComponent implements OnInit {
  userId!: number;
  fluxModelId!: number;
  trainingId!: number;

  training: Training | null = null;
  synthesises: Synthesis[] = [];
  displayedColumns: string[] = ['text', 'imageUrl', 'status', 'seed'];

  constructor(
    private dialog: MatDialog,
    private route: ActivatedRoute,
    private userTrainingService: UserTrainingService,
    private synthesisService: UserSynthesisService
  ) {}

  ngOnInit(): void {
    // Припустимо, що fluxModelId передається через параметри маршруту
    this.trainingId = +this.route.snapshot.paramMap.get('id')!;
    this.userId = +this.route.snapshot.paramMap.get('userId')!;
    this.loadTraining();
    this.loadSynthesises();
  }

  loadTraining() {
    this.userTrainingService.get(this.userId, this.trainingId).subscribe({
      next: (training) => {
        this.fluxModelId = training.fluxModelId;
      },
      error: (err) => console.error('Помилка завантаження training:', err)
    });
  }

  loadSynthesises(): void {
    this.synthesisService.getSynthesisesByTrainingId(this.userId, this.trainingId).subscribe({
      next: (synthesises) => this.synthesises = synthesises,
      error: (err) => console.error('Помилка завантаження synthesis:', err)
    });
  }

  openFullScreen(photo: any): void {
    this.dialog.open(FullScreenPhotoComponent, {
      panelClass: 'full-screen-dialog',
      // disableClose: false за замовчуванням дозволяє закривати по кліку на бекграунд,
      data: { imageUrl: photo.imageUrl },
    });
  }
  
}
