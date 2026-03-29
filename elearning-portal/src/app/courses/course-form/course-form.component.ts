import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CourseService } from '../../core/services/course.service';
import { TokenStorageService } from '../../core/services/token-storage.service';
import { CourseCreateDTO } from '../../core/models/models';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector:    'app-course-form',
  templateUrl: './course-form.component.html',
  styleUrls:   ['./course-form.component.css']
})
export class CourseFormComponent implements OnInit {
  isEdit                    = false;
  courseId                  = 0;
  loading                   = false;
  errorMsg                  = '';
  uploadingVideo            = false;
  selectedFile: File | null = null;
  videoUploadSuccess        = false;
  existingVideoOriginalName = '';
  deletingVideo             = false;
  newYoutubeLink            = '';
  role = '';


  form: CourseCreateDTO = {
    title: '', description: '', instructorId: 0, youtubeLinks: []
  };

  constructor(
    private courseService: CourseService,
    private tokenStorage:  TokenStorageService,
    private route:         ActivatedRoute,
    private router:        Router,
    private snackBar:      MatSnackBar
  ) {}

  ngOnInit(): void {
    this.courseId = Number(this.route.snapshot.paramMap.get('id'));
    this.isEdit   = !!this.courseId && this.router.url.includes('edit');

    this.role = this.tokenStorage.getRole();  // ★ ADD THIS

    // Instructor ID always comes from token for Instructor role
    if (this.role === 'Instructor') {
        this.form.instructorId = this.tokenStorage.getUserId();
    }

    if (this.isEdit) {
        this.courseService.getById(this.courseId).subscribe(course => {
            this.form = {
                title:        course.title,
                description:  course.description,
                instructorId: course.instructorId,
                youtubeLinks: course.youtubeLinks ?? []
            };
            this.existingVideoOriginalName = course.videoOriginalName ?? '';
        });
    }
}

  addYoutubeLink(): void {
    const link = this.newYoutubeLink.trim();
    if (!link) return;

    if (!link.includes('youtube.com') && !link.includes('youtu.be')) {
      this.snackBar.open('Please enter a valid YouTube URL', 'Close', { duration: 3000 });
      return;
    }

    if (!this.form.youtubeLinks) this.form.youtubeLinks = [];
    this.form.youtubeLinks.push(link);
    this.newYoutubeLink = '';
  }

  removeYoutubeLink(index: number): void {
    this.form.youtubeLinks?.splice(index, 1);
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (!file) return;

    const allowedTypes = ['video/mp4', 'video/webm', 'video/ogg'];
    if (!allowedTypes.includes(file.type)) {
      this.snackBar.open('Only MP4, WebM, OGG files allowed', 'Close', { duration: 3000 });
      return;
    }
    if (file.size > 500 * 1024 * 1024) {
      this.snackBar.open('File must be under 500MB', 'Close', { duration: 3000 });
      return;
    }
    this.selectedFile = file;
  }

  onSubmit(): void {
    this.loading  = true;
    this.errorMsg = '';

    const obs = this.isEdit
      ? this.courseService.update(this.courseId, this.form)
      : this.courseService.create(this.form);

    obs.subscribe({
      next: (course) => {
        this.loading  = false;
        this.courseId = course.id;

        // ★ If video selected, upload first THEN navigate
        if (this.selectedFile) {
          this.uploadingVideo = true;
          this.courseService.uploadVideo(this.courseId, this.selectedFile).subscribe({
            next: () => {
              this.uploadingVideo     = false;
              this.videoUploadSuccess = true;
              this.snackBar.open('Course saved and video uploaded!', 'Close', { duration: 3000 });
              this.router.navigate(['/courses']);  // ← navigate AFTER upload
            },
            error: (err) => {
              this.uploadingVideo = false;
              this.snackBar.open(err.error?.message ?? 'Video upload failed', 'Close', { duration: 3000 });
              this.router.navigate(['/courses']);  // ← navigate even if upload fails
            }
          });
        } else {
          // No video — navigate immediately
          this.snackBar.open('Course saved!', 'Close', { duration: 3000 });
          this.router.navigate(['/courses']);
        }
      },
      error: (err) => {
        this.loading  = false;
        this.errorMsg = err.error?.message ?? 'Operation failed.';
      }
    });
  }

  deleteVideo(): void {
    if (!confirm('Are you sure you want to delete this video?')) return;
    this.deletingVideo = true;

    this.courseService.deleteVideo(this.courseId).subscribe({
      next: () => {
        this.deletingVideo             = false;
        this.existingVideoOriginalName = '';
        this.snackBar.open('Video deleted successfully', 'Close', { duration: 3000 });
      },
      error: (err) => {
        this.deletingVideo = false;
        this.snackBar.open(err.error?.message ?? 'Delete failed', 'Close', { duration: 3000 });
      }
    });
  }
}