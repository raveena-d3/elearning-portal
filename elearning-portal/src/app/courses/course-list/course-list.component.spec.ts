import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { of } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';

import { CourseListComponent } from './course-list.component';
import { CourseService } from '../../core/services/course.service';
import { EnrollmentService } from '../../core/services/enrollment.service';
import { TokenStorageService } from '../../core/services/token-storage.service';

describe('CourseListComponent', () => {
  let component: CourseListComponent;
  let fixture: ComponentFixture<CourseListComponent>;

  let mockCourseService: any;
  let mockEnrollmentService: any;
  let mockTokenStorage: any;
  let mockSnackBar: any;

  beforeEach(async () => {
    mockCourseService = {
      getAll: jasmine.createSpy('getAll').and.returnValue(
        of([
          {
            id: 1,
            title: 'Angular Basics',
            description: 'Intro course',
            instructorId: 10
          },
          {
            id: 2,
            title: 'Java Basics',
            description: 'Core Java',
            instructorId: 11
          }
        ])
      ),
      delete: jasmine.createSpy('delete').and.returnValue(of({}))
    };

    mockEnrollmentService = {
      enroll: jasmine.createSpy('enroll').and.returnValue(of({}))
    };

    mockTokenStorage = {
      getRole: jasmine.createSpy('getRole').and.returnValue('Student'),
      getUserId: jasmine.createSpy('getUserId').and.returnValue(101)
    };

    mockSnackBar = {
      open: jasmine.createSpy('open')
    };

    await TestBed.configureTestingModule({
      declarations: [CourseListComponent],
      providers: [
        { provide: CourseService, useValue: mockCourseService },
        { provide: EnrollmentService, useValue: mockEnrollmentService },
        { provide: TokenStorageService, useValue: mockTokenStorage },
        { provide: MatSnackBar, useValue: mockSnackBar }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(CourseListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load role and userId on init', () => {
    expect(mockTokenStorage.getRole).toHaveBeenCalled();
    expect(mockTokenStorage.getUserId).toHaveBeenCalled();
    expect(component.role).toBe('Student');
    expect(component.userId).toBe(101);
  });

  it('should load courses on init', () => {
    expect(mockCourseService.getAll).toHaveBeenCalled();
    expect(component.courses.length).toBe(2);
    expect(component.loading).toBeFalse();
  });

  it('should call enrollment service when enroll is called', () => {
    component.enroll(1);

    expect(mockEnrollmentService.enroll).toHaveBeenCalledWith(1);
  });

  it('should show success snackbar after enroll', () => {
    component.enroll(1);

    expect(mockSnackBar.open).toHaveBeenCalledWith(
      'Enrolled successfully!',
      'Close',
      { duration: 3000 }
    );
  });

  it('should delete course when confirmed', () => {
    spyOn(window, 'confirm').and.returnValue(true);
    component.courses = [
      { id: 1, title: 'Angular Basics', description: 'Intro course', instructorId: 10 } as any,
      { id: 2, title: 'Java Basics', description: 'Core Java', instructorId: 11 } as any
    ];

    component.deleteCourse(1);

    expect(mockCourseService.delete).toHaveBeenCalledWith(1);
    expect(component.courses.length).toBe(1);
    expect(component.courses[0].id).toBe(2);
    expect(mockSnackBar.open).toHaveBeenCalledWith(
      'Course deleted',
      'Close',
      { duration: 3000 }
    );
  });

  it('should not delete course when confirmation is cancelled', () => {
    spyOn(window, 'confirm').and.returnValue(false);
    component.courses = [
      { id: 1, title: 'Angular Basics', description: 'Intro course', instructorId: 10 } as any
    ];

    component.deleteCourse(1);

    expect(mockCourseService.delete).not.toHaveBeenCalled();
    expect(component.courses.length).toBe(1);
  });
});