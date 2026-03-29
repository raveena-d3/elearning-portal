import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import mqtt from 'mqtt';

export interface MqttEvent {
  id:         string;
  topic:      string;
  event:      string;
  student?:   string;
  course?:    string;
  quiz?:      string;
  score?:     number;
  percent?:   number;
  message:    string;
  timestamp:  string;
  receivedAt: string;
}

export interface MqttStats {
  logins:      number;
  enrollments: number;
  quizzes:     number;
  discussions: number;
  completions: number;
  progress:    number;
}

@Injectable({ providedIn: 'root' })
export class MqttService {

  private readonly BROKER_URL = 'wss://broker.hivemq.com:8884/mqtt';

  private readonly TOPICS = [
    'elearning/student/login',
    'elearning/course/enrolled',
    'elearning/quiz/submitted',
    'elearning/student/progress',
    'elearning/discussion/asked',
    'elearning/course/completed',
  ];

  connected$  = new BehaviorSubject<boolean>(false);
  messages$   = new BehaviorSubject<MqttEvent[]>([]);
  stats$      = new BehaviorSubject<MqttStats>({
    logins: 0, enrollments: 0, quizzes: 0,
    discussions: 0, completions: 0, progress: 0
  });

  constructor() { this.connect(); }

  private connect(): void {
    const client = mqtt.connect(this.BROKER_URL, {
      clientId:        'elearning-angular-' + Math.random().toString(16).slice(2),
      clean:           true,
      reconnectPeriod: 3000,
    });

    client.on('connect', () => {
      this.connected$.next(true);
      this.TOPICS.forEach(topic => client.subscribe(topic, { qos: 1 }));
    });

    client.on('message', (topic: string, payload: Buffer) => {
      let parsed: any = {};
      try { parsed = JSON.parse(payload.toString()); } catch {}

      const event: MqttEvent = {
        id: Date.now() + '-' + Math.random(),
        topic,
        receivedAt: new Date().toLocaleTimeString(),
        ...parsed,
      };

      this.messages$.next([event, ...this.messages$.getValue()].slice(0, 50));

      const stats = { ...this.stats$.getValue() };
      if (topic.includes('login'))      stats.logins++;
      if (topic.includes('enrolled'))   stats.enrollments++;
      if (topic.includes('submitted'))  stats.quizzes++;
      if (topic.includes('discussion')) stats.discussions++;
      if (topic.includes('completed'))  stats.completions++;
      if (topic.includes('progress'))   stats.progress++;
      this.stats$.next(stats);
    });

    client.on('disconnect', () => this.connected$.next(false));
    client.on('error',      () => this.connected$.next(false));
  }

  clearMessages(): void { this.messages$.next([]); }
}