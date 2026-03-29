import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { QuizService } from '../../core/services/quiz.service';
import { QuizAttemptService } from '../../core/services/quiz-attempt.service';
import { TokenStorageService } from '../../core/services/token-storage.service';
import { QuizResponseDTO, QuizAttemptResponseDTO } from '../../core/models/models';

@Component({
  selector: 'app-quiz-list',
  templateUrl: './quiz-list.component.html',
  styleUrls: ['./quiz-list.component.css']
})
export class QuizListComponent implements OnInit {
  quizzes: QuizResponseDTO[] = [];
  myAttempts: QuizAttemptResponseDTO[] = [];
  role = '';
  userId = 0;
  filterCourseId: number | null = null;
  attemptColumns = ['quiz', 'score', 'percentage', 'date'];

  constructor(
    private quizService: QuizService,
    private quizAttemptService: QuizAttemptService,
    private tokenStorage: TokenStorageService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.role = this.tokenStorage.getRole();
    this.userId = this.tokenStorage.getUserId();
    if (this.role === 'Instructor') {
        this.quizService.getMyCoursesQuizzes()
            .subscribe({
                next:  (q) => this.quizzes = q,
                error: ()  => {}
            });
    }


    if (this.role === 'Student') {
      this.quizAttemptService.getByStudent(this.userId)
        .subscribe({
          next: (a) => this.myAttempts = a,
          error: () => {}
        });
    }
  }

  loadByCourse(): void {
    if (!this.filterCourseId) return;
    if (this.role === 'Instructor') return;
    this.quizService.getByCourse(this.filterCourseId)
      .subscribe(q => this.quizzes = q);
  }

  deleteQuiz(id: number): void {
    if (!confirm('Delete this quiz?')) return;
    this.quizService.delete(id).subscribe({
      next: () => {
        this.quizzes = this.quizzes.filter(q => q.id !== id);
        this.snackBar.open('Quiz deleted', 'Close', { duration: 3000 });
      },
      error: (err) => this.snackBar.open(
        err.error?.message ?? 'Delete failed', 'Close', { duration: 3000 }
      )
    });
  }
}