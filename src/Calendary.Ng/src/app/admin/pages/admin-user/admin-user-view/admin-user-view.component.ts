import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { AdminUserService } from '../../../../../services/admin/user.service';
import { AdminUser } from '../../../../../models/admin-user';
import { UserCalendarListComponent } from './calendar/user-calendar-list/user-calendar-list.component';
import { UserFluxModelListComponent } from './flux-model/user-flux-model-list/user-flux-model-list.component';


@Component({
  selector: 'app-admin-user-view',
  standalone: true, 
  imports: [CommonModule, MatTableModule, MatButtonModule, UserCalendarListComponent, UserFluxModelListComponent],
  templateUrl: './admin-user-view.component.html',
  styleUrls: ['./admin-user-view.component.scss']
})
export class AdminUserViewComponent implements OnInit {
  user: AdminUser | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private adminUserService: AdminUserService
  ) {}

  ngOnInit(): void {
    const userIdParam = this.route.snapshot.paramMap.get('id');
    const userId = userIdParam ? +userIdParam : 0;
    this.loadUser(userId);
  }

  loadUser(userId: number): void {
    this.adminUserService.getById(userId).subscribe({
      next: (user) => this.user = user,
      error: (err) => console.error('Помилка завантаження користувача', err)
    });
  }

  goBack(): void {
    this.router.navigate(['/admin/users']);
  }
}