// src/app/courses/course-list/course-list.component.ts
import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CourseService } from '../../core/services/course.service';
import { EnrollmentService } from '../../core/services/enrollment.service';
import { TokenStorageService } from '../../core/services/token-storage.service';
import { CourseResponseDTO } from '../../core/models/models';

@Component({
  selector: 'app-course-list',
  templateUrl: './course-list.component.html',
  styleUrls: ['./course-list.component.css']
})
export class CourseListComponent implements OnInit {
  courses: CourseResponseDTO[] = [];
  loading = false;
  role = '';
  userId = 0;
  username=' ';

  constructor(
    private courseService: CourseService,
    private enrollmentService: EnrollmentService,
    private tokenStorage: TokenStorageService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.role = this.tokenStorage.getRole();
    this.userId = this.tokenStorage.getUserId();
    this.username = this.tokenStorage.getUsername();
    this.loadCourses();
  }

  loadCourses(): void {
    this.loading = true;

    // ★ Instructor sees only their own courses
    const obs = this.role === 'Instructor'
        ? this.courseService.getMyCourses()
        : this.courseService.getAll();

    obs.subscribe({
        next:  (data) => { this.courses = data; this.loading = false; },
        error: ()     => { this.loading = false; }
    });
}

  enroll(courseId: number): void {
      this.enrollmentService.enroll(courseId).subscribe({
      next: () => this.snackBar.open('Enrolled successfully!', 'Close', { duration: 3000 }),
      error: (err) => this.snackBar.open(err.error?.message ?? 'Enrollment failed', 'Close', { duration: 3000 })
    });
  }

  deleteCourse(id: number): void {
    if (!confirm('Delete this course?')) return;
    this.courseService.delete(id).subscribe({
      next: () => {
        this.courses = this.courses.filter(c => c.id !== id);
        this.snackBar.open('Course deleted', 'Close', { duration: 3000 });
      },
      error: (err) => this.snackBar.open(err.error?.message ?? 'Delete failed', 'Close', { duration: 3000 })
    });
  }
}