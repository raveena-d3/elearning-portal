import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

import { MqttDashboardComponent } from './mqtt-dashboard.component';
import { MqttService } from 'src/app/core/services/mqtt.service';

describe('MqttDashboardComponent', () => {
  let component: MqttDashboardComponent;
  let fixture: ComponentFixture<MqttDashboardComponent>;

  let mockMqttService: any;
  let connectedSubject: BehaviorSubject<boolean>;
  let messagesSubject: BehaviorSubject<any[]>;
  let statsSubject: BehaviorSubject<any>;

  beforeEach(async () => {
    connectedSubject = new BehaviorSubject<boolean>(true);
    messagesSubject = new BehaviorSubject<any[]>([
      {
        id: '1',
        topic: 'user/login',
        payload: 'User logged in'
      }
    ]);
    statsSubject = new BehaviorSubject<any>({
      logins: 1,
      enrollments: 2,
      quizzes: 3,
      discussions: 4,
      completions: 5,
      progress: 6
    });

    mockMqttService = {
      connected$: connectedSubject.asObservable(),
      messages$: messagesSubject.asObservable(),
      stats$: statsSubject.asObservable(),
      clearMessages: jasmine.createSpy('clearMessages')
    };

    await TestBed.configureTestingModule({
      declarations: [MqttDashboardComponent],
      providers: [
        { provide: MqttService, useValue: mockMqttService }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(MqttDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should subscribe and load connected, messages and stats on init', () => {
    expect(component.connected).toBeTrue();
    expect(component.messages.length).toBe(1);
    expect(component.messages[0].id).toBe('1');
    expect(component.stats.logins).toBe(1);
    expect(component.stats.progress).toBe(6);
  });

  it('should update connected value when connected$ emits', () => {
    connectedSubject.next(false);

    expect(component.connected).toBeFalse();
  });

  it('should update messages when messages$ emits', () => {
    messagesSubject.next([
      { id: '2', topic: 'quiz/submitted', payload: 'Quiz submitted' },
      { id: '3', topic: 'course/enrolled', payload: 'Course enrolled' }
    ]);

    expect(component.messages.length).toBe(2);
    expect(component.messages[0].id).toBe('2');
    expect(component.messages[1].id).toBe('3');
  });

  it('should update stats when stats$ emits', () => {
    statsSubject.next({
      logins: 10,
      enrollments: 11,
      quizzes: 12,
      discussions: 13,
      completions: 14,
      progress: 15
    });

    expect(component.stats.logins).toBe(10);
    expect(component.stats.enrollments).toBe(11);
    expect(component.stats.quizzes).toBe(12);
    expect(component.stats.discussions).toBe(13);
    expect(component.stats.completions).toBe(14);
    expect(component.stats.progress).toBe(15);
  });

  it('should call clearMessages', () => {
    component.clearMessages();

    expect(mockMqttService.clearMessages).toHaveBeenCalled();
  });

  it('should return event id in trackById', () => {
    const event = {
      id: 'abc123',
      topic: 'user/login',
      payload: 'payload'
    } as any;

    const result = component.trackById(0, event);

    expect(result).toBe('abc123');
  });

  it('should return login style for login topic', () => {
    const style = component.getEventStyle('user/login');

    expect(style.label).toBe('Login');
    expect(style.icon).toBe('👤');
  });

  it('should return enrolled style for enrolled topic', () => {
    const style = component.getEventStyle('course/enrolled');

    expect(style.label).toBe('Enrolled');
    expect(style.icon).toBe('📚');
  });

  it('should return quiz style for submitted topic', () => {
    const style = component.getEventStyle('quiz/submitted');

    expect(style.label).toBe('Quiz');
    expect(style.icon).toBe('✅');
  });

  it('should return progress style for progress topic', () => {
    const style = component.getEventStyle('student/progress');

    expect(style.label).toBe('Progress');
    expect(style.icon).toBe('📈');
  });

  it('should return discussion style for discussion topic', () => {
    const style = component.getEventStyle('forum/discussion');

    expect(style.label).toBe('Discussion');
    expect(style.icon).toBe('💬');
  });

  it('should return completed style for completed topic', () => {
    const style = component.getEventStyle('course/completed');

    expect(style.label).toBe('Completed');
    expect(style.icon).toBe('🎓');
  });

  it('should return default style for unknown topic', () => {
    const style = component.getEventStyle('unknown/topic');

    expect(style.label).toBe('Event');
    expect(style.icon).toBe('📡');
  });

  it('should unsubscribe all subscriptions on destroy', () => {
    const unsubSpies = component['subs'].map((s: any) => spyOn(s, 'unsubscribe'));

    component.ngOnDestroy();

    unsubSpies.forEach(spy => {
      expect(spy).toHaveBeenCalled();
    });
  });
});