import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { MqttService, MqttEvent, MqttStats } from 'src/app/core/services/mqtt.service';

@Component({
  selector:    'app-mqtt-dashboard',
  templateUrl: './mqtt-dashboard.component.html',
  styleUrls:   ['./mqtt-dashboard.component.css'],
})
export class MqttDashboardComponent implements OnInit, OnDestroy {

  connected = false;
  messages:  MqttEvent[] = [];
  stats:     MqttStats = {
    logins: 0, enrollments: 0, quizzes: 0,
    discussions: 0, completions: 0, progress: 0
  };

  private subs: Subscription[] = [];

  constructor(private mqttService: MqttService) {}

  ngOnInit(): void {
    this.subs.push(
      this.mqttService.connected$.subscribe(v => this.connected = v),
      this.mqttService.messages$.subscribe(v  => this.messages  = v),
      this.mqttService.stats$.subscribe(v     => this.stats     = v),
    );
  }

  ngOnDestroy(): void {
    this.subs.forEach(s => s.unsubscribe());
  }

  clearMessages(): void {
    this.mqttService.clearMessages();
  }

  trackById(_: number, event: MqttEvent): string {
    return event.id;
  }

  getEventStyle(topic: string) {
    if (topic.includes('login'))      return { icon: '👤', color: '#185FA5', bg: '#E6F1FB', label: 'Login'      };
    if (topic.includes('enrolled'))   return { icon: '📚', color: '#0F6E56', bg: '#E1F5EE', label: 'Enrolled'   };
    if (topic.includes('submitted'))  return { icon: '✅', color: '#3B6D11', bg: '#EAF3DE', label: 'Quiz'       };
    if (topic.includes('progress'))   return { icon: '📈', color: '#854F0B', bg: '#FAEEDA', label: 'Progress'   };
    if (topic.includes('discussion')) return { icon: '💬', color: '#533AB7', bg: '#EEEDFE', label: 'Discussion' };
    if (topic.includes('completed'))  return { icon: '🎓', color: '#993C1D', bg: '#FAECE7', label: 'Completed'  };
    return { icon: '📡', color: '#5F5E5A', bg: '#F1EFE8', label: 'Event' };
  }
}