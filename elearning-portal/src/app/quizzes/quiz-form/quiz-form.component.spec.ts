import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { of, throwError } from 'rxjs';
import { Router } from '@angular/router';

import { QuizFormComponent } from './quiz-form.component';
import { QuizService } from '../../core/services/quiz.service';
import { TokenStorageService } from '../../core/services/token-storage.service';

describe('QuizFormComponent', () => {
  let component: QuizFormComponent;
  let fixture: ComponentFixture<QuizFormComponent>;

  let mockQuizService: any;
  let mockTokenStorage: any;
  let mockRouter: any;

  beforeEach(async () => {
    mockQuizService = {
      create: jasmine.createSpy('create').and.returnValue(of({}))
    };

    mockTokenStorage = {
      getRole: jasmine.createSpy('getRole').and.returnValue('Instructor'),
      getUserId: jasmine.createSpy('getUserId').and.returnValue(10)
    };

    mockRouter = {
      navigate: jasmine.createSpy('navigate')
    };

    await TestBed.configureTestingModule({
      declarations: [QuizFormComponent],
      providers: [
        { provide: QuizService, useValue: mockQuizService },
        { provide: TokenStorageService, useValue: mockTokenStorage },
        { provide: Router, useValue: mockRouter }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(QuizFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should set instructorId on init when role is Instructor', () => {
    expect(mockTokenStorage.getRole).toHaveBeenCalled();
    expect(mockTokenStorage.getUserId).toHaveBeenCalled();
    expect(component.form.instructorId).toBe(10);
  });

  it('should add a new question', () => {
    const initialLength = component.questions.length;

    component.addQuestion();

    expect(component.questions.length).toBe(initialLength + 1);
    expect(component.questions[1]).toEqual({
      question: '',
      options: ['', '', '', ''],
      correctAnswer: ''
    });
  });

  it('should create quiz and navigate on submit', () => {
    component.form.title = 'Angular Quiz';
    component.form.courseId = 101;
    component.questions = [
      {
        question: 'What is Angular?',
        options: ['Framework', 'Library', 'Database', 'Tool'],
        correctAnswer: 'Framework'
      }
    ];

    component.onSubmit();

    expect(mockQuizService.create).toHaveBeenCalledWith({
      title: 'Angular Quiz',
      questionsJson: JSON.stringify(component.questions),
      courseId: 101,
      instructorId: 10
    });
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/quizzes']);
    expect(component.errorMsg).toBe('');
  });

  it('should set errorMsg when create quiz fails', () => {
    mockQuizService.create.and.returnValue(
      throwError(() => ({ error: { message: 'Quiz creation failed' } }))
    );

    component.form.title = 'Angular Quiz';
    component.form.courseId = 101;
    component.questions = [
      {
        question: 'What is Angular?',
        options: ['Framework', 'Library', 'Database', 'Tool'],
        correctAnswer: 'Framework'
      }
    ];

    component.onSubmit();

    expect(component.errorMsg).toBe('Quiz creation failed');
    expect(mockRouter.navigate).not.toHaveBeenCalled();
  });

  it('should set default errorMsg when create quiz fails without message', () => {
    mockQuizService.create.and.returnValue(
      throwError(() => ({ error: {} }))
    );

    component.form.title = 'Angular Quiz';
    component.form.courseId = 101;

    component.onSubmit();

    expect(component.errorMsg).toBe('Failed to create quiz.');
    expect(mockRouter.navigate).not.toHaveBeenCalled();
  });
});