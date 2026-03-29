import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { of } from 'rxjs';
import { OAuthService } from 'angular-oauth2-oidc';

import { DashboardComponent } from './dashboard.component';
import { CourseService } from '../../core/services/course.service';
import { UserService } from '../../core/services/user.service';

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;

  let mockOAuthService: any;
  let mockCourseService: any;
  let mockUserService: any;

  beforeEach(async () => {
    const payload = {
      preferred_username: 'raveena',
      realm_access: {
        roles: ['Admin']
      }
    };

    const token =
      'header.' + btoa(JSON.stringify(payload)) + '.signature';

    mockOAuthService = {
      getAccessToken: jasmine.createSpy('getAccessToken').and.returnValue(token)
    };

    mockCourseService = {
      getAll: jasmine.createSpy('getAll').and.returnValue(
        of([
          { id: 1, title: 'Angular', description: 'Frontend', instructorId: 10 },
          { id: 2, title: 'Java', description: 'Backend', instructorId: 11 }
        ])
      )
    };

    mockUserService = {
      getAll: jasmine.createSpy('getAll').and.returnValue(
        of([
          { id: 1, username: 'admin' },
          { id: 2, username: 'student' },
          { id: 3, username: 'instructor' }
        ])
      )
    };

    await TestBed.configureTestingModule({
      declarations: [DashboardComponent],
      providers: [
        { provide: OAuthService, useValue: mockOAuthService },
        { provide: CourseService, useValue: mockCourseService },
        { provide: UserService, useValue: mockUserService }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load username and role from token on init', () => {
    expect(mockOAuthService.getAccessToken).toHaveBeenCalled();
    expect(component.username).toBe('raveena');
    expect(component.role).toBe('Admin');
  });

  it('should load total courses', () => {
    expect(mockCourseService.getAll).toHaveBeenCalled();
    expect(component.totalCourses).toBe(2);
    expect(component.loading).toBeFalse();
  });

  it('should load total users for Admin role', () => {
    expect(mockUserService.getAll).toHaveBeenCalled();
    expect(component.totalUsers).toBe(3);
  });

  it('should not load users when role is not Admin', () => {
    const nonAdminPayload = {
      preferred_username: 'student1',
      realm_access: {
        roles: ['Student']
      }
    };

    const nonAdminToken =
      'header.' + btoa(JSON.stringify(nonAdminPayload)) + '.signature';

    mockOAuthService.getAccessToken.and.returnValue(nonAdminToken);
    mockUserService.getAll.calls.reset();

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.username).toBe('student1');
    expect(component.role).toBe('Student');
    expect(mockUserService.getAll).not.toHaveBeenCalled();
  });

  it('should handle invalid token without crashing', () => {
    mockOAuthService.getAccessToken.and.returnValue('invalid.token');
    spyOn(console, 'error');

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(console.error).toHaveBeenCalled();
    expect(mockCourseService.getAll).toHaveBeenCalled();
  });
});