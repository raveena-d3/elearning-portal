import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { of } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';

import { DiscussionListComponent } from './discussion-list.component';
import { DiscussionService } from '../../core/services/discussion.service';
import { TokenStorageService } from '../../core/services/token-storage.service';

describe('DiscussionListComponent', () => {
  let component: DiscussionListComponent;
  let fixture: ComponentFixture<DiscussionListComponent>;

  let mockDiscussionService: any;
  let mockTokenStorage: any;
  let mockSnackBar: any;

  beforeEach(async () => {
    mockDiscussionService = {
      getByStudent: jasmine.createSpy('getByStudent').and.returnValue(
        of([
          {
            id: 1,
            courseId: 101,
            studentId: 5,
            question: 'What is Angular?',
            answer: ''
          }
        ])
      ),
      getByCourse: jasmine.createSpy('getByCourse').and.returnValue(
        of([
          {
            id: 2,
            courseId: 101,
            studentId: 5,
            question: 'Explain components',
            answer: ''
          }
        ])
      ),
      askQuestion: jasmine.createSpy('askQuestion').and.callFake((question) =>
        of({
          id: 3,
          ...question
        })
      ),
      answerQuestion: jasmine.createSpy('answerQuestion').and.callFake((id, answer) =>
        of({
          id,
          courseId: 101,
          studentId: 5,
          question: 'Old question',
          answer
        })
      ),
      delete: jasmine.createSpy('delete').and.returnValue(of({}))
    };

    mockTokenStorage = {
      getRole: jasmine.createSpy('getRole').and.returnValue('Student'),
      getUserId: jasmine.createSpy('getUserId').and.returnValue(5)
    };

    mockSnackBar = {
      open: jasmine.createSpy('open')
    };

    await TestBed.configureTestingModule({
      declarations: [DiscussionListComponent],
      providers: [
        { provide: DiscussionService, useValue: mockDiscussionService },
        { provide: TokenStorageService, useValue: mockTokenStorage },
        { provide: MatSnackBar, useValue: mockSnackBar }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(DiscussionListComponent);
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
    expect(component.newQuestion.studentId).toBe(5);
  });

  it('should load student discussions on init when role is Student', () => {
    expect(mockDiscussionService.getByStudent).toHaveBeenCalledWith(5);
    expect(component.discussions.length).toBe(1);
    expect(component.discussions[0].question).toBe('What is Angular?');
  });

  it('should load discussions by course', () => {
    component.filterCourseId = 101;

    component.loadByCourse();

    expect(mockDiscussionService.getByCourse).toHaveBeenCalledWith(101);
    expect(component.discussions.length).toBe(1);
    expect(component.discussions[0].id).toBe(2);
  });

  it('should not load discussions by course when filterCourseId is null', () => {
    component.filterCourseId = null;

    component.loadByCourse();

    expect(mockDiscussionService.getByCourse).not.toHaveBeenCalled();
  });

  it('should ask question and add it to top of discussions', () => {
  component.newQuestion.courseId = 101;
  component.newQuestion.studentId = 5;
  component.newQuestion.question = 'New question';

  component.askQuestion();

  expect(mockDiscussionService.askQuestion).toHaveBeenCalled();
  expect(component.discussions[0].question).toBe('New question');
  expect(component.newQuestion.question).toBe('');
  expect(component.newQuestion.courseId).toBe(0);
  expect(mockSnackBar.open).toHaveBeenCalledWith(
    'Question submitted!',
    'Close',
    { duration: 3000 }
  );
});

  it('should submit answer and update discussion', () => {
    component.discussions = [
      {
        id: 1,
        courseId: 101,
        studentId: 5,
        question: 'Old question',
        answer: ''
      } as any
    ];
    component.answerInputs[1] = 'This is the answer';

    component.submitAnswer(1);

    expect(mockDiscussionService.answerQuestion).toHaveBeenCalledWith(1, 'This is the answer');
    expect(component.discussions[0].answer).toBe('This is the answer');
    expect(component.answerInputs[1]).toBeUndefined();
    expect(mockSnackBar.open).toHaveBeenCalledWith(
      'Answer submitted!',
      'Close',
      { duration: 3000 }
    );
  });

  it('should not submit answer when input is empty', () => {
    component.answerInputs[1] = '';

    component.submitAnswer(1);

    expect(mockDiscussionService.answerQuestion).not.toHaveBeenCalled();
  });

  it('should delete discussion when confirmed', () => {
    spyOn(window, 'confirm').and.returnValue(true);
    component.discussions = [
      { id: 1, courseId: 101, studentId: 5, question: 'Q1', answer: '' } as any,
      { id: 2, courseId: 101, studentId: 5, question: 'Q2', answer: '' } as any
    ];

    component.deleteDiscussion(1);

    expect(mockDiscussionService.delete).toHaveBeenCalledWith(1);
    expect(component.discussions.length).toBe(1);
    expect(component.discussions[0].id).toBe(2);
    expect(mockSnackBar.open).toHaveBeenCalledWith(
      'Deleted',
      'Close',
      { duration: 3000 }
    );
  });

  it('should not delete discussion when confirmation is cancelled', () => {
    spyOn(window, 'confirm').and.returnValue(false);
    component.discussions = [
      { id: 1, courseId: 101, studentId: 5, question: 'Q1', answer: '' } as any
    ];

    component.deleteDiscussion(1);

    expect(mockDiscussionService.delete).not.toHaveBeenCalled();
    expect(component.discussions.length).toBe(1);
  });
});