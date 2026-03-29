import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { of, throwError } from 'rxjs';

import { EnrollmentListComponent } from './enrollment-list.component';
import { EnrollmentService } from '../../core/services/enrollment.service';

describe('EnrollmentListComponent', () => {
  let component: EnrollmentListComponent;
  let fixture: ComponentFixture<EnrollmentListComponent>;

  let mockEnrollmentService: any;

  beforeEach(async () => {
    mockEnrollmentService = {
      getMyEnrollments: jasmine.createSpy('getMyEnrollments').and.returnValue(
        of([
          {
            id: 1,
            courseId: 101,
            studentId: 5
          },
          {
            id: 2,
            courseId: 102,
            studentId: 5
          }
        ])
      ),
      unenroll: jasmine.createSpy('unenroll').and.returnValue(of({}))
    };

    await TestBed.configureTestingModule({
      declarations: [EnrollmentListComponent],
      providers: [
        { provide: EnrollmentService, useValue: mockEnrollmentService }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(EnrollmentListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load enrollments on init', () => {
    expect(mockEnrollmentService.getMyEnrollments).toHaveBeenCalled();
    expect(component.enrollments.length).toBe(2);
    expect(component.loading).toBeFalse();
    expect(component.error).toBe('');
  });

  it('should set error when loading enrollments fails', () => {
    mockEnrollmentService.getMyEnrollments.and.returnValue(
      throwError(() => new Error('Load failed'))
    );
    spyOn(console, 'error');

    fixture = TestBed.createComponent(EnrollmentListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(console.error).toHaveBeenCalled();
    expect(component.error).toBe('Failed to load enrollments.');
    expect(component.loading).toBeFalse();
  });

  it('should unenroll and remove item when confirmed', () => {
    spyOn(window, 'confirm').and.returnValue(true);
    component.enrollments = [
      { id: 1, courseId: 101, studentId: 5 } as any,
      { id: 2, courseId: 102, studentId: 5 } as any
    ];

    component.unenroll(1);

    expect(mockEnrollmentService.unenroll).toHaveBeenCalledWith(1);
    expect(component.enrollments.length).toBe(1);
    expect(component.enrollments[0].id).toBe(2);
  });

  it('should not unenroll when confirmation is cancelled', () => {
    spyOn(window, 'confirm').and.returnValue(false);
    component.enrollments = [
      { id: 1, courseId: 101, studentId: 5 } as any
    ];

    component.unenroll(1);

    expect(mockEnrollmentService.unenroll).not.toHaveBeenCalled();
    expect(component.enrollments.length).toBe(1);
  });

  it('should log error when unenroll fails', () => {
    spyOn(window, 'confirm').and.returnValue(true);
    spyOn(console, 'error');
    mockEnrollmentService.unenroll.and.returnValue(
      throwError(() => new Error('Unenroll failed'))
    );
    component.enrollments = [
      { id: 1, courseId: 101, studentId: 5 } as any
    ];

    component.unenroll(1);

    expect(mockEnrollmentService.unenroll).toHaveBeenCalledWith(1);
    expect(console.error).toHaveBeenCalled();
    expect(component.enrollments.length).toBe(1);
  });
});