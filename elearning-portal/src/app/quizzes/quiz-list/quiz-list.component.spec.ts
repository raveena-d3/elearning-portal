import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { of } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';

import { QuizListComponent } from './quiz-list.component';
import { QuizService } from '../../core/services/quiz.service';
import { QuizAttemptService } from '../../core/services/quiz-attempt.service';
import { TokenStorageService } from '../../core/services/token-storage.service';

describe('QuizListComponent', () => {
  let component: QuizListComponent;
  let fixture: ComponentFixture<QuizListComponent>;

  let mockQuizService: any;
  let mockQuizAttemptService: any;
  let mockTokenStorage: any;
  let mockSnackBar: any;

  beforeEach(async () => {
    mockQuizService = {
      getByCourse: jasmine.createSpy('getByCourse').and.returnValue(
        of([
          {
            id: 1,
            title: 'Angular Quiz',
            courseId: 101,
            instructorId: 10
          },
          {
            id: 2,
            title: 'Java Quiz',
            courseId: 101,
            instructorId: 10
          }
        ])
      ),
      delete: jasmine.createSpy('delete').and.returnValue(of({}))
    };

    mockQuizAttemptService = {
      getByStudent: jasmine.createSpy('getByStudent').and.returnValue(
        of([
          {
            id: 1,
            quizId: 1,
            studentId: 5,
            score: 8,
            percentage: 80,
            attemptedAt: '2026-03-19'
          }
        ])
      )
    };

    mockTokenStorage = {
      getRole: jasmine.createSpy('getRole').and.returnValue('Student'),
      getUserId: jasmine.createSpy('getUserId').and.returnValue(5)
    };

    mockSnackBar = {
      open: jasmine.createSpy('open')
    };

    await TestBed.configureTestingModule({
      declarations: [QuizListComponent],
      providers: [
        { provide: QuizService, useValue: mockQuizService },
        { provide: QuizAttemptService, useValue: mockQuizAttemptService },
        { provide: TokenStorageService, useValue: mockTokenStorage },
        { provide: MatSnackBar, useValue: mockSnackBar }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(QuizListComponent);
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
    expect(component.userId).toBe(5);
  });

  it('should load attempts on init for Student role', () => {
    expect(mockQuizAttemptService.getByStudent).toHaveBeenCalledWith(5);
    expect(component.myAttempts.length).toBe(1);
    expect(component.myAttempts[0].quizId).toBe(1);
  });

  it('should not load attempts on init when role is not Student', () => {
    mockTokenStorage.getRole.and.returnValue('Instructor');
    mockQuizAttemptService.getByStudent.calls.reset();

    fixture = TestBed.createComponent(QuizListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.role).toBe('Instructor');
    expect(mockQuizAttemptService.getByStudent).not.toHaveBeenCalled();
  });

  it('should load quizzes by course', () => {
    component.filterCourseId = 101;

    component.loadByCourse();

    expect(mockQuizService.getByCourse).toHaveBeenCalledWith(101);
    expect(component.quizzes.length).toBe(2);
    expect(component.quizzes[0].title).toBe('Angular Quiz');
  });

  it('should not load quizzes by course when filterCourseId is null', () => {
    component.filterCourseId = null;

    component.loadByCourse();

    expect(mockQuizService.getByCourse).not.toHaveBeenCalled();
  });

  it('should delete quiz when confirmed', () => {
    spyOn(window, 'confirm').and.returnValue(true);
    component.quizzes = [
      { id: 1, title: 'Angular Quiz', courseId: 101, instructorId: 10 } as any,
      { id: 2, title: 'Java Quiz', courseId: 101, instructorId: 10 } as any
    ];

    component.deleteQuiz(1);

    expect(mockQuizService.delete).toHaveBeenCalledWith(1);
    expect(component.quizzes.length).toBe(1);
    expect(component.quizzes[0].id).toBe(2);
    expect(mockSnackBar.open).toHaveBeenCalledWith(
      'Quiz deleted',
      'Close',
      { duration: 3000 }
    );
  });

  it('should not delete quiz when confirmation is cancelled', () => {
    spyOn(window, 'confirm').and.returnValue(false);
    component.quizzes = [
      { id: 1, title: 'Angular Quiz', courseId: 101, instructorId: 10 } as any
    ];

    component.deleteQuiz(1);

    expect(mockQuizService.delete).not.toHaveBeenCalled();
    expect(component.quizzes.length).toBe(1);
  });
});