import { finalize } from 'rxjs';
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { UserSynthesisService } from '../../../../../../services/admin/user-synthesis.service';
import { CreateSynthesis, Synthesis } from '../../../../../../models/synthesis';
import { Training } from '../../../../../../models/training';
import { UserTrainingService } from '../../../../../../services/admin/user-training.service';
import { FullScreenPhotoComponent } from '../../../../components/full-screen-photo.component';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { CreateSynthesisDialogComponent } from './create-synthesis-dialog/create-synthesis-dialog.component';



@Component({
  selector: 'app-synthesis',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatIconModule, MatButtonModule],
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
      next: (synthesises) => this.synthesises = synthesises.sort(p => -p.id),
      error: (err) => console.error('Помилка завантаження synthesis:', err)
    });
  }

  openFullScreen(imageUrl: string): void {
    this.dialog.open(FullScreenPhotoComponent, {
      panelClass: 'full-screen-dialog',
      // disableClose: false за замовчуванням дозволяє закривати по кліку на бекграунд,
      data: { imageUrl: imageUrl },
    });
  }
  

  openCreateSynthesisDialog() {
    const dialogRef = this.dialog.open(CreateSynthesisDialogComponent, {
      width: '400px',
      data: { fluxModelId: this.fluxModelId, 
              trainingId : this.trainingId  }
    });

    dialogRef.afterClosed().subscribe((result: CreateSynthesis | undefined) => {
      if (result) {
        // Викликаємо генерацію synthesis з отриманими даними (result.text та result.seed)
        this.synthesisService.createAndRun(this.userId, result)
          .pipe(finalize(() => this.loadSynthesises()))
          .subscribe({
            next: () => console.log('Synthesis згенеровано успішно'),
            error: (err) => console.error('Помилка генерації synthesis:', err)
          });
      }
    });
  }
}
