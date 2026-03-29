import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { UserService } from '../../core/services/user.service';
import { UserResponseDTO } from '../../core/models/models';

@Component({
  selector: 'app-user-list',
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.css']
})
export class UserListComponent implements OnInit {
  users: UserResponseDTO[] = [];
  displayedColumns = ['id', 'username', 'role', 'actions'];
  loading = false;

  // Create user form
  showCreateForm = false;
  creating = false;
  newUser = {
    username: '',
    password: '',
    role: 'Student'
  };

  constructor(
    private userService: UserService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
  this.loading = true;
  this.userService.getAll().subscribe({
    next: (u) => { 
      console.log('Users loaded:', u);
      this.users = u; 
      this.loading = false; 
    },
    error: (err: any) => { 
      console.error('Load users failed:', err.status, err.error);
      this.loading = false; 
    }
  });
}

  toggleCreateForm(): void {
    this.showCreateForm = !this.showCreateForm;
    this.newUser = { username: '', password: '', role: 'Student' };
  }
  createUser(): void {
  if (!this.newUser.username || !this.newUser.password) {
    this.snackBar.open('Username and password are required',
      'Close', { duration: 3000 });
    return;
  }

  this.creating = true;
  this.userService.create(this.newUser).subscribe({
    next: (user: UserResponseDTO) => {
      this.users.push(user);
      this.creating = false;
      this.showCreateForm = false;
      this.newUser = { username: '', password: '', role: 'Student' };
      this.snackBar.open(
        `User ${user.username} created successfully in app and Keycloak!`,
        'Close', { duration: 4000 });
    },
    error: (err: any) => {
      this.creating = false;
      this.snackBar.open(
        err.error?.message ?? 'Create failed',
        'Close', { duration: 3000 });
    }
  });
}
  

  updateRole(id: number, role: string): void {
    this.userService.updateRole(id, role).subscribe({
      next: () => this.snackBar.open(
        'Role updated successfully', 'Close', { duration: 3000 }),
      error: (err: any) => this.snackBar.open(
  err.error?.message ?? 'Update failed', 'Close', { duration: 3000 })
    });
  }

  deleteUser(id: number): void {
    if (!confirm('Delete this user? This will also remove them from Keycloak!'))
      return;

    this.userService.delete(id).subscribe({
      next: () => {
        this.users = this.users.filter(u => u.id !== id);
        this.snackBar.open(
          'User deleted from app and Keycloak',
          'Close', { duration: 3000 });
      },
      error: (err: any) => this.snackBar.open(
  err.error?.message ?? 'Delete failed', 'Close', { duration: 3000 })
    });
  }
}