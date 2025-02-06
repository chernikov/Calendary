import { finalize } from 'rxjs';
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { UserSynthesisService } from '../../../../../../services/admin/user-synthesis.service';
import { CreateSynthesis } from '../../../../../../models/synthesis';
import { Training } from '../../../../../../models/training';
import { UserTrainingService } from '../../../../../../services/admin/user-training.service';
import { FullScreenPhotoComponent } from '../../../../components/full-screen-photo.component';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { CreateSynthesisDialogComponent } from './create-synthesis-dialog/create-synthesis-dialog.component';
import { UserPromptService } from '../../../../../../services/admin/user-prompt.service';
import { Prompt } from '../../../../../../models/prompt';
import { MatProgressSpinner } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-synthesis',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatIconModule, MatButtonModule, MatProgressSpinner],
  templateUrl: './synthesis.component.html',
  styleUrls: ['./synthesis.component.scss'],
})
export class AdminUserSynthesisComponent implements OnInit {
  userId!: number;
  fluxModelId!: number;
  trainingId!: number;

  // Прапорець завантаження
  isLoading = false;

  training: Training | null = null;
  prompts: Prompt[] = [];
  displayedColumns: string[] = ['text', 'synthData', 'actions'];

  constructor(
    private dialog: MatDialog,
    private route: ActivatedRoute,
    private userTrainingService: UserTrainingService,
    private synthesisService: UserSynthesisService,
    private promptService: UserPromptService
  ) {}

  ngOnInit(): void {
    this.trainingId = +this.route.snapshot.paramMap.get('id')!;
    this.userId = +this.route.snapshot.paramMap.get('userId')!;
    this.loadTraining();
    this.loadPrompts();
  }

  loadTraining() {
    this.userTrainingService.get(this.userId, this.trainingId).subscribe({
      next: (training) => {
        this.fluxModelId = training.fluxModelId;
      },
      error: (err) => console.error('Помилка завантаження training:', err),
    });
  }

  loadPrompts(): void {
    this.promptService
      .getPromptByTrainingId(this.userId, this.trainingId)
      .subscribe({
        next: (prompts) => (this.prompts = prompts.sort((p) => -p.id)),
        error: (err) => console.error('Помилка завантаження synthesis:', err),
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
      data: { fluxModelId: this.fluxModelId, trainingId: this.trainingId },
    });

    dialogRef.afterClosed().subscribe((result: CreateSynthesis | undefined) => {
        if (result) {
          this.createSynthesis(result);
          // Викликаємо генерацію synthesis з отриманими даними (result.text та result.seed)
        }
      }
    );
  }

  runAgain(promptId: number, text: string): void {
    var synthesis = new CreateSynthesis();
    synthesis.fluxModelId = this.fluxModelId;
    synthesis.trainingId = this.trainingId;
    synthesis.text = text!;
    synthesis.promptId = promptId;
    this.createSynthesis(synthesis);
  }

  createSynthesis(synthesis: CreateSynthesis): void {
    this.isLoading = true;
    this.synthesisService
      .createAndRun(this.userId, synthesis)
      .pipe(
        finalize(() => {
          this.loadPrompts();
          this.isLoading = false;
        })
      )
      .subscribe({
        next: () => console.log('Synthesis згенеровано успішно'),
        error: (err) => console.error('Помилка генерації synthesis:', err),
      });
  }
}
