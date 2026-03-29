import { Component, OnInit } from '@angular/core';
import { EnrollmentService } from '../../core/services/enrollment.service';
import { EnrollmentResponseDTO } from '../../core/models/models';

@Component({
  selector: 'app-enrollment-list',
  templateUrl: './enrollment-list.component.html',
  styleUrls: ['./enrollment-list.component.css']
})
export class EnrollmentListComponent implements OnInit {
  enrollments: EnrollmentResponseDTO[] = [];
  loading = true;
  error = '';

  constructor(private enrollmentService: EnrollmentService) {}

  ngOnInit(): void {
    this.loadEnrollments();
  }

  loadEnrollments(): void {
    this.loading = true;
    this.enrollmentService.getMyEnrollments().subscribe({
      next: (data) => {
        this.enrollments = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Enrollment error:', err);
        this.error = 'Failed to load enrollments.';
        this.loading = false;
      }
    });
  }

  unenroll(id: number): void {
    if (confirm('Are you sure you want to unenroll?')) {
      this.enrollmentService.unenroll(id).subscribe({
        next: () => {
          this.enrollments = this.enrollments.filter(e => e.id !== id);
        },
        error: (err) => console.error('Unenroll error:', err)
      });
    }
  }
}