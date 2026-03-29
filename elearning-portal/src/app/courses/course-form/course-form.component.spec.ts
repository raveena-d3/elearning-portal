import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { of, throwError } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';

import { CourseFormComponent } from './course-form.component';
import { CourseService } from '../../core/services/course.service';
import { TokenStorageService } from '../../core/services/token-storage.service';
import { FormsModule } from '@angular/forms';

describe('CourseFormComponent', () => {
  let component: CourseFormComponent;
  let fixture: ComponentFixture<CourseFormComponent>;

  let mockCourseService: any;
  let mockTokenStorage: any;
  let mockActivatedRoute: any;
  let mockRouter: any;

  beforeEach(async () => {
    mockCourseService = {
      getById: jasmine.createSpy('getById').and.returnValue(
        of({
          id: 5,
          title: 'Java Basics',
          description: 'Intro to Java',
          instructorId: 10
        })
      ),
      create: jasmine.createSpy('create').and.returnValue(of({})),
      update: jasmine.createSpy('update').and.returnValue(of({}))
    };

    mockTokenStorage = {
      getRole: jasmine.createSpy('getRole').and.returnValue('Instructor'),
      getUserId: jasmine.createSpy('getUserId').and.returnValue(10)
    };

    mockActivatedRoute = {
      snapshot: {
        paramMap: {
          get: jasmine.createSpy('get').and.returnValue('0')
        }
      }
    };

    mockRouter = {
      url: '/courses/create',
      navigate: jasmine.createSpy('navigate')
    };

    await TestBed.configureTestingModule({
      declarations: [CourseFormComponent],
      imports: [FormsModule],
      providers: [
        { provide: CourseService, useValue: mockCourseService },
        { provide: TokenStorageService, useValue: mockTokenStorage },
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: Router, useValue: mockRouter }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(CourseFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should set instructorId when role is Instructor', () => {
    expect(mockTokenStorage.getRole).toHaveBeenCalled();
    expect(mockTokenStorage.getUserId).toHaveBeenCalled();
    expect(component.form.instructorId).toBe(10);
  });

  it('should not be in edit mode when id is 0', () => {
    expect(component.courseId).toBe(0);
    expect(component.isEdit).toBeFalse();
  });

  it('should be in edit mode and load course when url has edit and id exists', () => {
    mockActivatedRoute.snapshot.paramMap.get.and.returnValue('5');
    mockRouter.url = '/courses/5/edit';

    fixture = TestBed.createComponent(CourseFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.courseId).toBe(5);
    expect(component.isEdit).toBeTrue();
    expect(mockCourseService.getById).toHaveBeenCalledWith(5);
    expect(component.form.title).toBe('Java Basics');
    expect(component.form.description).toBe('Intro to Java');
    expect(component.form.instructorId).toBe(10);
  });

  it('should call create on submit in create mode', () => {
    component.isEdit = false;
    component.form = {
      title: 'New Course',
      description: 'New Description',
      instructorId: 10
    };

    component.onSubmit();

    expect(mockCourseService.create).toHaveBeenCalledWith(component.form);
    expect(mockCourseService.update).not.toHaveBeenCalled();
    expect(component.loading).toBeFalse();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/courses']);
  });

  it('should call update on submit in edit mode', () => {
    component.isEdit = true;
    component.courseId = 5;
    component.form = {
      title: 'Updated Course',
      description: 'Updated Description',
      instructorId: 10
    };

    component.onSubmit();

    expect(mockCourseService.update).toHaveBeenCalledWith(5, component.form);
    expect(mockCourseService.create).not.toHaveBeenCalled();
    expect(component.loading).toBeFalse();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/courses']);
  });

  it('should set errorMsg when create fails', () => {
    mockCourseService.create.and.returnValue(
      throwError(() => ({ error: { message: 'Create failed' } }))
    );

    component.isEdit = false;
    component.form = {
      title: 'Fail Course',
      description: 'Fail Description',
      instructorId: 10
    };

    component.onSubmit();

    expect(component.loading).toBeFalse();
    expect(component.errorMsg).toBe('Create failed');
  });

  it('should set default errorMsg when update fails without message', () => {
    mockCourseService.update.and.returnValue(
      throwError(() => ({ error: {} }))
    );

    component.isEdit = true;
    component.courseId = 5;
    component.form = {
      title: 'Fail Update',
      description: 'Fail Update Desc',
      instructorId: 10
    };

    component.onSubmit();

    expect(component.loading).toBeFalse();
    expect(component.errorMsg).toBe('Operation failed.');
  });
});