import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { AdminFluxModel } from '../../../../../models/flux-model';
import { AdminFluxModelService } from '../../../../../services/admin/flux-model.service';

@Component({
  selector: 'app-view-flux-model',
  standalone: true,
  imports: [CommonModule, MatButtonModule],
  templateUrl: './view-flux-model.component.html',
  styleUrl: './view-flux-model.component.scss'
})
export class ViewFluxModelComponent {
    fluxModel?: AdminFluxModel;
    isLoading: boolean = true;
  
    constructor(
      private route: ActivatedRoute,
      private adminFluxModelService: AdminFluxModelService,
      private router : Router
    ) {}
  
    ngOnInit(): void {
      this.loadFluxModel();
    }
  
    loadFluxModel(): void {
      const id = Number(this.route.snapshot.paramMap.get('id'));
      if (!isNaN(id)) {
        this.adminFluxModelService.getById(id).subscribe(
          (data) => {
            this.fluxModel = data;
            this.isLoading = false;
          },
          (error) => {
            console.error('Error loading flux model:', error);
            this.isLoading = false;
          }
        );
      }
    }

    goBack(): void {
      this.router.navigate(['/admin/flux-models']);
    }
  }