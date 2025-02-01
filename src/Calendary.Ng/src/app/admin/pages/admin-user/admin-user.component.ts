import { Component } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { AdminUserService } from '../../../../services/admin-user.service';
import { AdminCreateUser, AdminUser } from '../../../../models/admin-user';
import { CreateUserDialogComponent } from './create-user-dialog/create-user-dialog.component';
import { MatDialog } from '@angular/material/dialog';

@Component({
  selector: 'app-admin-user',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule],
  templateUrl: './admin-user.component.html',
  styleUrls: ['./admin-user.component.scss']
})
export class AdminUserComponent {

  columns = ['id', 'userName', 'email', 'phoneNumber', 'actions'];
  users: AdminUser[] = [];
  filteredUsers: AdminUser[] = [];
  searchQuery: string = '';
  selectedUser: AdminUser | null = null;
  isEditMode: boolean = false;
  isCreateMode: boolean = false;

  constructor(private adminUserService: AdminUserService, 
    private dialog: MatDialog) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.adminUserService.getAll().subscribe((data) => {
      this.users = data;
      this.filteredUsers = data;
    });
  }

  selectUser(user: AdminUser): void {
    this.selectedUser = { ...user };
    this.isEditMode = true;
    this.isCreateMode = false;
  }

  createUser(): void {
    this.selectedUser = new AdminUser();
    this.isCreateMode = true;
    this.isEditMode = false;
  }

  saveUser(): void {
    if (this.selectedUser) {
      if (this.isCreateMode) {
        const newUser = new AdminCreateUser();
        newUser.userName = this.selectedUser.userName;
        newUser.email = this.selectedUser.email;
        newUser.phoneNumber = this.selectedUser.phoneNumber;

        this.adminUserService.create(newUser).subscribe(() => {
          this.loadUsers();
          this.isCreateMode = false;
          this.selectedUser = null;
        });
      } else if (this.isEditMode) {
        this.adminUserService.update(this.selectedUser).subscribe(() => {
          this.loadUsers();
          this.isEditMode = false;
          this.selectedUser = null;
        });
      }
    }
  }

  deleteUser(user: AdminUser): void {
    if (confirm(`Are you sure you want to delete ${user.userName}?`)) {
      this.adminUserService.delete(user.id).subscribe(() => {
        this.loadUsers();
      });
    }
  }

  cancel(): void {
    this.selectedUser = null;
    this.isEditMode = false;
    this.isCreateMode = false;
  }

  filterUsers(): void {
    this.filteredUsers = this.users.filter(user =>
      Object.values(user).some(value =>
        value?.toString().toLowerCase().includes(this.searchQuery.toLowerCase())
      )
    );
  }


  openCreateDialog(): void {
    const dialogRef = this.dialog.open(CreateUserDialogComponent, {
      width: '400px',
      data: { user: new AdminCreateUser() }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.adminUserService.create(result).subscribe(() => {
          this.loadUsers();
        });
      }
    });
  }
}
