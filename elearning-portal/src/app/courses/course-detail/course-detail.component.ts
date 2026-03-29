import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CourseService } from '../../core/services/course.service';
import { EnrollmentService } from '../../core/services/enrollment.service';
import { TokenStorageService } from '../../core/services/token-storage.service';
import { CourseResponseDTO } from '../../core/models/models';

@Component({
  selector:    'app-course-detail',
  templateUrl: './course-detail.component.html',
  styleUrls:   ['./course-detail.component.css']
})
export class CourseDetailComponent implements OnInit {
  course:   CourseResponseDTO | null = null;
  loading   = false;
  errorMsg  = '';
  role      = '';
  userId    = 0;
  username=' ';
  isEnrolled=false;

  constructor(
    private route:             ActivatedRoute,
    private courseService:     CourseService,
    private enrollmentService: EnrollmentService,
    private tokenStorage:      TokenStorageService,
    private snackBar:          MatSnackBar
  ) {}

  ngOnInit(): void {
    this.role   = this.tokenStorage.getRole();
    this.userId = this.tokenStorage.getUserId();
    this.username = this.tokenStorage.getUsername(); 
    const id    = Number(this.route.snapshot.paramMap.get('id'));
    this.loading = true;
    this.courseService.getById(id).subscribe({
      next:  (c) => { 
        this.course = c; 
        this.loading = false; 
       if(this.role === 'Student'){
        this.enrollmentService.checkEnrollment(c.id).subscribe({
          next:(res)=> this.isEnrolled=res.isEnrolled,
          error:()=>this.isEnrolled=false
        });
       }
       if(this.role === 'Admin'|| this.role ==='Instructor'){
        this.isEnrolled=true;
       }
      },
      error: ()  => { this.errorMsg = 'Course not found.'; this.loading = false; }
    });
  }

  enroll(): void {
    if (!this.course) return;
    this.enrollmentService.enroll(this.course.id).subscribe({
      next:()=>{
        this.isEnrolled=true;
        this.snackBar.open('Enrolled successfully!','close',{duration:3000})
      },
      error: (err) => this.snackBar.open(err.error?.message ?? 'Enrollment failed', 'Close', { duration: 3000 })
    });
  }

  // Convert YouTube watch URL to embed URL
  getEmbedUrl(url: string): string {
    try {
      const videoId = new URL(url).searchParams.get('v');
      if (videoId) return `https://www.youtube.com/embed/${videoId}`;
      if (url.includes('youtu.be')) {
        const id = url.split('youtu.be/')[1].split('?')[0];
        return `https://www.youtube.com/embed/${id}`;
      }
    } catch {}
    return url;
  }

  // Get video stream URL — no token needed since endpoint is AllowAnonymous
  getCourseVideoUrl(): string {
    if (!this.course) return '';
    return `/api/course/${this.course.id}/video`;
}
}