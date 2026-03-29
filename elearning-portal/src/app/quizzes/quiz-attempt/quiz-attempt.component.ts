import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { QuizService } from '../../core/services/quiz.service';
import { QuizAttemptService } from '../../core/services/quiz-attempt.service';
import { TokenStorageService } from '../../core/services/token-storage.service';
import { QuizAttemptResponseDTO } from '../../core/models/models';

interface QuizQuestion {
  question: string;
  options: string[];
  correctAnswer: string;
}

interface QuizDetail {
  id: number;
  title: string;
  courseTitle: string;
  questionsJson: string;
}

@Component({
  selector: 'app-quiz-attempt',
  templateUrl: './quiz-attempt.component.html',
  styleUrls: ['./quiz-attempt.component.css']
})
export class QuizAttemptComponent implements OnInit {
  quiz: QuizDetail | null = null;
  questions: QuizQuestion[] = [];
  selectedAnswers: { [index: number]: string } = {};
  loading = false;
  submitting = false;
  submitted = false;
  alreadyAttempted = false;
  errorMsg = '';
  result: QuizAttemptResponseDTO | null = null;
  userId = 0;

  constructor(
    private route: ActivatedRoute,
    private quizService: QuizService,
    private quizAttemptService: QuizAttemptService,
    private tokenStorage: TokenStorageService
  ) {}

  ngOnInit(): void {
    this.userId = this.tokenStorage.getUserId();
    const quizId = Number(this.route.snapshot.paramMap.get('id'));
    this.loading = true;

    this.quizService.getById(quizId).subscribe({
      next: (quiz: any) => {
        this.quiz = quiz;

        // Try all possible casing variants from backend
        const raw = quiz.questionsJson
          || quiz.QuestionsJson
          || quiz['questionsJson']
          || '[]';

        console.log('Raw questionsJson:', raw); // debug - check browser console

        try {
          const parsed = JSON.parse(raw);
          // Handle both camelCase and PascalCase from backend
          this.questions = parsed.map((q: any) => ({
            question: q.question || q.Question || '',
            options: q.options || q.Options || [],
            correctAnswer: q.correctAnswer || q.CorrectAnswer || ''
          }));
        } catch (e) {
          console.error('Parse error:', e);
          this.questions = [];
          this.errorMsg = 'Could not parse quiz questions.';
        }

        // Check if already attempted
        this.quizAttemptService.getByStudent(this.userId).subscribe({
          next: (attempts) => {
            const existing = attempts.find(a => a.quizId === quizId);
            if (existing) {
              this.alreadyAttempted = true;
              this.result = existing;
            }
            this.loading = false;
          },
          error: () => { this.loading = false; }
        });
      },
      error: () => {
        this.errorMsg = 'Quiz not found.';
        this.loading = false;
      }
    });
  }

  selectAnswer(questionIndex: number, option: string): void {
    this.selectedAnswers[questionIndex] = option;
  }

  get answeredCount(): number {
    return Object.keys(this.selectedAnswers).length;
  }

  submitQuiz(): void {
    if (!this.quiz) return;
    this.submitting = true;
    this.errorMsg = '';

    const answers: string[] = this.questions.map((_, i) =>
      this.selectedAnswers[i] ?? ''
    );

    this.quizAttemptService.submit({
      quizId: this.quiz.id,
      studentId: this.userId,
      answers
    }).subscribe({
      next: (result) => {
        this.result = result;
        this.submitted = true;
        this.submitting = false;
      },
      error: (err) => {
        this.submitting = false;
        this.errorMsg = err.error?.message ?? 'Failed to submit quiz.';
      }
    });
  }
}