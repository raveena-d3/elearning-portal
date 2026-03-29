import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { of, throwError } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';

import { UserListComponent } from './user-list.component';
import { UserService } from '../../core/services/user.service';

describe('UserListComponent', () => {
  let component: UserListComponent;
  let fixture: ComponentFixture<UserListComponent>;

  let mockUserService: any;
  let mockSnackBar: any;

  beforeEach(async () => {
    mockUserService = {
      getAll: jasmine.createSpy('getAll').and.returnValue(
        of([
          { id: 1, username: 'admin', role: 'Admin' },
          { id: 2, username: 'student1', role: 'Student' }
        ])
      ),
      create: jasmine.createSpy('create').and.callFake((user: any) =>
        of({
          id: 3,
          username: user.username,
          role: user.role
        })
      ),
      updateRole: jasmine.createSpy('updateRole').and.returnValue(of({})),
      delete: jasmine.createSpy('delete').and.returnValue(of({}))
    };

    mockSnackBar = {
      open: jasmine.createSpy('open')
    };

    await TestBed.configureTestingModule({
      declarations: [UserListComponent],
      providers: [
        { provide: UserService, useValue: mockUserService },
        { provide: MatSnackBar, useValue: mockSnackBar }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(UserListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load users on init', () => {
    expect(mockUserService.getAll).toHaveBeenCalled();
    expect(component.users.length).toBe(2);
    expect(component.loading).toBeFalse();
  });

  it('should toggle create form and reset newUser', () => {
    component.newUser = {
      username: 'temp',
      password: '123',
      role: 'Admin'
    };

    component.toggleCreateForm();

    expect(component.showCreateForm).toBeTrue();
    expect(component.newUser).toEqual({
      username: '',
      password: '',
      role: 'Student'
    });
  });

  it('should show validation snackbar when username or password is missing', () => {
    component.newUser = {
      username: '',
      password: '',
      role: 'Student'
    };

    component.createUser();

    expect(mockUserService.create).not.toHaveBeenCalled();
    expect(mockSnackBar.open).toHaveBeenCalledWith(
      'Username and password are required',
      'Close',
      { duration: 3000 }
    );
  });

  it('should create user successfully', () => {
    component.showCreateForm = true;
    component.newUser = {
      username: 'newuser',
      password: 'pass123',
      role: 'Student'
    };

    component.createUser();

    expect(mockUserService.create).toHaveBeenCalledWith({
      username: 'newuser',
      password: 'pass123',
      role: 'Student'
    });
    expect(component.users.length).toBe(3);
    expect(component.creating).toBeFalse();
    expect(component.showCreateForm).toBeFalse();
    expect(component.newUser).toEqual({
      username: '',
      password: '',
      role: 'Student'
    });
    expect(mockSnackBar.open).toHaveBeenCalledWith(
      'User newuser created successfully in app and Keycloak!',
      'Close',
      { duration: 4000 }
    );
  });

  it('should show error snackbar when create user fails', () => {
    mockUserService.create.and.returnValue(
      throwError(() => ({ error: { message: 'Create failed' } }))
    );

    component.newUser = {
      username: 'newuser',
      password: 'pass123',
      role: 'Student'
    };

    component.createUser();

    expect(component.creating).toBeFalse();
    expect(mockSnackBar.open).toHaveBeenCalledWith(
      'Create failed',
      'Close',
      { duration: 3000 }
    );
  });

  it('should update user role successfully', () => {
    component.updateRole(1, 'Instructor');

    expect(mockUserService.updateRole).toHaveBeenCalledWith(1, 'Instructor');
    expect(mockSnackBar.open).toHaveBeenCalledWith(
      'Role updated successfully',
      'Close',
      { duration: 3000 }
    );
  });

  it('should delete user when confirmed', () => {
    spyOn(window, 'confirm').and.returnValue(true);
    component.users = [
      { id: 1, username: 'admin', role: 'Admin' } as any,
      { id: 2, username: 'student1', role: 'Student' } as any
    ];

    component.deleteUser(1);

    expect(mockUserService.delete).toHaveBeenCalledWith(1);
    expect(component.users.length).toBe(1);
    expect(component.users[0].id).toBe(2);
    expect(mockSnackBar.open).toHaveBeenCalledWith(
      'User deleted from app and Keycloak',
      'Close',
      { duration: 3000 }
    );
  });

  it('should not delete user when confirmation is cancelled', () => {
    spyOn(window, 'confirm').and.returnValue(false);
    component.users = [
      { id: 1, username: 'admin', role: 'Admin' } as any
    ];

    component.deleteUser(1);

    expect(mockUserService.delete).not.toHaveBeenCalled();
    expect(component.users.length).toBe(1);
  });
});