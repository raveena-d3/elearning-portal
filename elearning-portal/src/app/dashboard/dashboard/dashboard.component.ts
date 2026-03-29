import { Component, OnInit } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { CourseService } from '../../core/services/course.service';
import { UserService } from '../../core/services/user.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  username = '';
  role = '';
  totalCourses = 0;
  totalUsers = 0;
  loading = true;

  constructor(
    private oauthService: OAuthService,
    private courseService: CourseService,
    private userService: UserService
  ) {}

  ngOnInit(): void {
    try {
      const token = this.oauthService.getAccessToken();
      if (token) {
        const payload = JSON.parse(atob(token.split('.')[1]));
        this.username = payload?.preferred_username ?? 'User';
        const roles: string[] = payload?.realm_access?.roles ?? [];
        this.role = roles.find(r =>
          ['Admin', 'Instructor', 'Student'].includes(r)
        ) ?? '';
      }
    } catch (e) {
      console.error('Token parse error:', e);
    }
    this.loadStats();
  }

  loadStats(): void {
    this.courseService.getAll().subscribe({
      next: (courses) => {
        this.totalCourses = courses.length;
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });

    if (this.role === 'Admin') {
      this.userService.getAll().subscribe({
        next: (users) => this.totalUsers = users.length,
        error: () => {}
      });
    }
  }
}