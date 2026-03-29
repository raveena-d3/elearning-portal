import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { of, throwError } from 'rxjs';
import { ActivatedRoute } from '@angular/router';

import { QuizAttemptComponent } from './quiz-attempt.component';
import { QuizService } from '../../core/services/quiz.service';
import { QuizAttemptService } from '../../core/services/quiz-attempt.service';
import { TokenStorageService } from '../../core/services/token-storage.service';

describe('QuizAttemptComponent', () => {
  let component: QuizAttemptComponent;
  let fixture: ComponentFixture<QuizAttemptComponent>;

  let mockQuizService: any;
  let mockQuizAttemptService: any;
  let mockTokenStorage: any;
  let mockActivatedRoute: any;

  beforeEach(async () => {
    mockQuizService = {
      getById: jasmine.createSpy('getById').and.returnValue(
        of({
          id: 1,
          title: 'Angular Quiz',
          courseTitle: 'Angular Basics',
          questionsJson: JSON.stringify([
            {
              question: 'What is Angular?',
              options: ['Framework', 'Library'],
              correctAnswer: 'Framework'
            },
            {
              question: 'What is TypeScript?',
              options: ['Language', 'Database'],
              correctAnswer: 'Language'
            }
          ])
        })
      )
    };

    mockQuizAttemptService = {
      getByStudent: jasmine.createSpy('getByStudent').and.returnValue(of([])),
      submit: jasmine.createSpy('submit').and.returnValue(
        of({
          id: 1,
          quizId: 1,
          studentId: 5,
          score: 2
        })
      )
    };

    mockTokenStorage = {
      getUserId: jasmine.createSpy('getUserId').and.returnValue(5)
    };

    mockActivatedRoute = {
      snapshot: {
        paramMap: {
          get: () => '1'
        }
      }
    };

    await TestBed.configureTestingModule({
      declarations: [QuizAttemptComponent],
      providers: [
        { provide: QuizService, useValue: mockQuizService },
        { provide: QuizAttemptService, useValue: mockQuizAttemptService },
        { provide: TokenStorageService, useValue: mockTokenStorage },
        { provide: ActivatedRoute, useValue: mockActivatedRoute }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(QuizAttemptComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load quiz and parse questions on init', () => {
    expect(mockTokenStorage.getUserId).toHaveBeenCalled();
    expect(mockQuizService.getById).toHaveBeenCalledWith(1);
    expect(component.quiz?.title).toBe('Angular Quiz');
    expect(component.questions.length).toBe(2);
    expect(component.questions[0].question).toBe('What is Angular?');
    expect(component.loading).toBeFalse();
  });

  it('should check if quiz was already attempted', () => {
    expect(mockQuizAttemptService.getByStudent).toHaveBeenCalledWith(5);
    expect(component.alreadyAttempted).toBeFalse();
  });

  it('should mark alreadyAttempted when existing attempt is found', () => {
    mockQuizAttemptService.getByStudent.and.returnValue(
      of([
        {
          id: 11,
          quizId: 1,
          studentId: 5,
          score: 1
        }
      ])
    );

    fixture = TestBed.createComponent(QuizAttemptComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.alreadyAttempted).toBeTrue();
    expect(component.result?.quizId).toBe(1);
  });

  it('should select answer', () => {
    component.selectAnswer(0, 'Framework');

    expect(component.selectedAnswers[0]).toBe('Framework');
  });

  it('should return answeredCount correctly', () => {
    component.selectedAnswers[0] = 'Framework';
    component.selectedAnswers[1] = 'Language';

    expect(component.answeredCount).toBe(2);
  });

  it('should submit quiz successfully', () => {
    component.quiz = {
      id: 1,
      title: 'Angular Quiz',
      courseTitle: 'Angular Basics',
      questionsJson: ''
    };
    component.questions = [
      {
        question: 'What is Angular?',
        options: ['Framework', 'Library'],
        correctAnswer: 'Framework'
      },
      {
        question: 'What is TypeScript?',
        options: ['Language', 'Database'],
        correctAnswer: 'Language'
      }
    ];
    component.selectedAnswers[0] = 'Framework';
    component.selectedAnswers[1] = 'Language';

    component.submitQuiz();

    expect(mockQuizAttemptService.submit).toHaveBeenCalledWith({
      quizId: 1,
      studentId: 5,
      answers: ['Framework', 'Language']
    });
    expect(component.submitted).toBeTrue();
    expect(component.submitting).toBeFalse();
    expect(component.result?.score).toBe(2);
  });

  it('should not submit quiz when quiz is null', () => {
    component.quiz = null;

    component.submitQuiz();

    expect(mockQuizAttemptService.submit).not.toHaveBeenCalled();
  });

  it('should set errorMsg when submit fails', () => {
    mockQuizAttemptService.submit.and.returnValue(
      throwError(() => ({ error: { message: 'Submit failed' } }))
    );

    component.quiz = {
      id: 1,
      title: 'Angular Quiz',
      courseTitle: 'Angular Basics',
      questionsJson: ''
    };
    component.questions = [
      {
        question: 'What is Angular?',
        options: ['Framework', 'Library'],
        correctAnswer: 'Framework'
      }
    ];
    component.selectedAnswers[0] = 'Framework';

    component.submitQuiz();

    expect(component.submitting).toBeFalse();
    expect(component.errorMsg).toBe('Submit failed');
  });

  it('should set parse error when questionsJson is invalid', () => {
    spyOn(console, 'error');
    mockQuizService.getById.and.returnValue(
      of({
        id: 1,
        title: 'Broken Quiz',
        courseTitle: 'Angular Basics',
        questionsJson: 'invalid-json'
      })
    );

    fixture = TestBed.createComponent(QuizAttemptComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(console.error).toHaveBeenCalled();
    expect(component.questions.length).toBe(0);
    expect(component.errorMsg).toBe('Could not parse quiz questions.');
  });

  it('should set errorMsg when quiz is not found', () => {
    mockQuizService.getById.and.returnValue(
      throwError(() => new Error('Quiz not found'))
    );

    fixture = TestBed.createComponent(QuizAttemptComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.errorMsg).toBe('Quiz not found.');
    expect(component.loading).toBeFalse();
  });
});