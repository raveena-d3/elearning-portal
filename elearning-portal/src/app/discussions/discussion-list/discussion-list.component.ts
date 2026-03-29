// src/app/discussions/discussion-list/discussion-list.component.ts
import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DiscussionService } from '../../core/services/discussion.service';
import { TokenStorageService } from '../../core/services/token-storage.service';
import { DiscussionResponseDTO, DiscussionCreateDTO } from '../../core/models/models';

@Component({
  selector: 'app-discussion-list',
  templateUrl: './discussion-list.component.html',
  styleUrls: ['./discussion-list.component.css']
})
export class DiscussionListComponent implements OnInit {
  discussions: DiscussionResponseDTO[] = [];
  role = '';
  userId = 0;
  filterCourseId: number | null = null;
  answerInputs: { [id: number]: string } = {};
  newQuestion: DiscussionCreateDTO = { courseId: 0, studentId: 0, question: '' };

  constructor(
    private discussionService: DiscussionService,
    private tokenStorage: TokenStorageService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.role = this.tokenStorage.getRole();
    this.userId = this.tokenStorage.getUserId();
    this.newQuestion.studentId = this.userId;

    // Students see their own discussions on load
    if (this.role === 'Student') {
      this.discussionService.getByStudent(this.userId)
        .subscribe(d => this.discussions = d);
    }
    if (this.role === 'Instructor') {
        this.discussionService.getMyCoursesDiscussions()
            .subscribe(d => this.discussions = d);
    }

  }

  loadByCourse(): void {
    if (!this.filterCourseId) return;
    this.discussionService.getByCourse(this.filterCourseId)
      .subscribe(d => this.discussions = d);
  }

  askQuestion(): void {
    this.discussionService.askQuestion(this.newQuestion).subscribe({
      next: (d) => {
        this.discussions.unshift(d);
        this.newQuestion.question = '';
        this.newQuestion.courseId = 0;
        this.snackBar.open('Question submitted!', 'Close', { duration: 3000 });
      },
      error: (err) => this.snackBar.open(
        err.error?.message ?? 'Failed to submit question', 'Close', { duration: 3000 }
      )
    });
  }

  submitAnswer(id: number): void {
    const answer = this.answerInputs[id];
    if (!answer) return;
    this.discussionService.answerQuestion(id, answer).subscribe({
      next: (updated) => {
        const idx = this.discussions.findIndex(d => d.id === id);
        if (idx !== -1) this.discussions[idx] = updated;
        delete this.answerInputs[id];
        this.snackBar.open('Answer submitted!', 'Close', { duration: 3000 });
      },
      error: (err) => this.snackBar.open(
        err.error?.message ?? 'Failed to submit answer', 'Close', { duration: 3000 }
      )
    });
  }

  deleteDiscussion(id: number): void {
    if (!confirm('Delete this discussion?')) return;
    this.discussionService.delete(id).subscribe({
      next: () => {
        this.discussions = this.discussions.filter(d => d.id !== id);
        this.snackBar.open('Deleted', 'Close', { duration: 3000 });
      }
    });
  }
}