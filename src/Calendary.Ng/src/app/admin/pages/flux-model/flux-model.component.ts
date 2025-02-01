import { Component, OnInit, ViewChild } from '@angular/core';
import { AdminFluxModelService } from '../../../../services/admin/flux-model.service';
import { AdminFluxModel } from '../../../../models/admin-flux-model';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { Router } from '@angular/router';

@Component({
  selector: 'app-flux-model',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatPaginatorModule, MatIconModule, MatButtonModule],
  templateUrl: './flux-model.component.html',
  styleUrl: './flux-model.component.scss'
})
export class FluxModelComponent implements OnInit 
{
  displayedColumns: string[] = ['id', 'name', 'user', 'createdAt', 'actions'];
  dataSource: MatTableDataSource<AdminFluxModel> = new MatTableDataSource<AdminFluxModel>();
  selectedModel?: AdminFluxModel;

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  constructor(private adminFluxModelService: AdminFluxModelService, 
      private router: Router
  ) {}

  ngOnInit(): void {
    this.loadFluxModels();
  }

  loadFluxModels(): void {
    this.adminFluxModelService.getAll().subscribe((data) => {
      this.dataSource = new MatTableDataSource(data);
      this.dataSource.paginator = this.paginator;
    });
  }

  viewDetails(model: AdminFluxModel): void {
    this.router.navigate(['/admin/flux-models/view', model.id]);
  }
}