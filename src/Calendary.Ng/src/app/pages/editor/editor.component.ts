import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FluxModel } from '../../../models/flux-model';
import { FluxModelService } from '../../../services/flux-model.service';

@Component({
  selector: 'app-editor',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule, MatButtonModule, MatProgressSpinnerModule],
  templateUrl: './editor.component.html',
  styleUrl: './editor.component.scss'
})
export class EditorComponent implements OnInit {
  isLoading = true;
  loadError = '';
  currentImage: string | null = null;
  userModels: FluxModel[] = [];
  activeModel: FluxModel | null = null;

  constructor(private fluxModelService: FluxModelService) {}

  ngOnInit(): void {
    this.loadUserModels();
  }

  loadUserModels(): void {
    this.isLoading = true;
    this.fluxModelService.current().subscribe({
      next: (model) => {
        const hasModel = model && model.id;
        this.userModels = hasModel ? [model] : [];
        this.activeModel = hasModel ? model : null;
        this.isLoading = false;
      },
      error: () => {
        this.loadError = 'Не вдалося завантажити моделі користувача.';
        this.userModels = [];
        this.activeModel = null;
        this.isLoading = false;
      },
    });
  }

  selectModel(model: FluxModel): void {
    this.activeModel = model;
  }
}
