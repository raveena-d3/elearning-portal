import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { ActivatedRoute } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';

import { CourseDetailComponent } from './course-detail.component';
import { CourseService } from 'src/app/core/services/course.service';
import { EnrollmentService } from 'src/app/core/services/enrollment.service';
import { TokenStorageService } from 'src/app/core/services/token-storage.service';

describe('CourseDetailComponent', () => {
  let component: CourseDetailComponent;
  let fixture: ComponentFixture<CourseDetailComponent>;

  let mockCourseService: any;
  let mockEnrollmentService: any;
  let mockTokenStorage: any;
  let mockActivatedRoute: any;
  let mockSnackBar: any;

  beforeEach(async () => {
    mockCourseService = {
      getById: jasmine.createSpy('getById').and.returnValue(
        of({
          id: 1,
          title: 'Angular Course',
          description: 'Test description',
          instructorId: 1
        })
      )
    };

    mockEnrollmentService = {
      enroll: jasmine.createSpy('enroll').and.returnValue(of({}))
    };

    mockTokenStorage = {
      getRole: jasmine.createSpy('getRole').and.returnValue('USER'),
      getUserId: jasmine.createSpy('getUserId').and.returnValue(1)
    };

    mockActivatedRoute = {
      snapshot: {
        paramMap: {
          get: () => '1'
        }
      }
    };

    mockSnackBar = {
      open: jasmine.createSpy('open')
    };

    await TestBed.configureTestingModule({
      declarations: [CourseDetailComponent],
      imports: [MatCardModule],
      providers: [
        { provide: CourseService, useValue: mockCourseService },
        { provide: EnrollmentService, useValue: mockEnrollmentService },
        { provide: TokenStorageService, useValue: mockTokenStorage },
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: MatSnackBar, useValue: mockSnackBar }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CourseDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('created', () => {
    expect(component).toBeTruthy();
  });

  it(' loaded course on init', () => {
    expect(mockCourseService.getById).toHaveBeenCalledWith(1);
    expect(component.course).toEqual(
      jasmine.objectContaining({
        id: 1,
        title: 'Angular Course',
        description: 'Test description',
        instructorId: 1
      })
    );
    expect(component.loading).toBeFalse();
  });

  it(' got role and userId from token storage', () => {
    expect(mockTokenStorage.getRole).toHaveBeenCalled();
    expect(mockTokenStorage.getUserId).toHaveBeenCalled();
    expect(component.role).toBe('USER');
    expect(component.userId).toBe(1);
  });

  it('should call enrollment service when enroll is called', () => {
    component.course = {
      id: 1,
      title: 'Angular Course',
      description: 'Test description',
      instructorId: 1
    } as any;

    component.enroll();

    expect(mockEnrollmentService.enroll).toHaveBeenCalledWith(1);
  });

  it('should show success snackbar after enroll', () => {
    component.course = {
      id: 1,
      title: 'Angular Course',
      description: 'Test description',
      instructorId: 1
    } as any;

    component.enroll();

    expect(mockSnackBar.open).toHaveBeenCalledWith(
      'Enrolled successfully!',
      'Close',
      { duration: 3000 }
    );
  });

  it('should not call enroll service if course is null', () => {
    component.course = null;

    component.enroll();

    expect(mockEnrollmentService.enroll).not.toHaveBeenCalled();
  });
});