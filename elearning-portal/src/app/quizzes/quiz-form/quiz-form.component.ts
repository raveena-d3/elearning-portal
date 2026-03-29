// src/app/quizzes/quiz-form/quiz-form.component.ts
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { QuizService } from '../../core/services/quiz.service';
import { TokenStorageService } from '../../core/services/token-storage.service';
import { QuizCreateDTO, QuizQuestion } from '../../core/models/models';

@Component({
  selector: 'app-quiz-form',
  templateUrl: './quiz-form.component.html',
  styleUrls: ['./quiz-form.component.css']
})
export class QuizFormComponent implements OnInit {
  form: QuizCreateDTO = {
    title: '',
    questionsJson: '',
    courseId: 0,
    instructorId: 0
  };

  questions: QuizQuestion[] = [
    { question: '', options: ['', '', '', ''], correctAnswer: '' }
  ];

  errorMsg = '';

  constructor(
    private quizService: QuizService,
    private tokenStorage: TokenStorageService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const role = this.tokenStorage.getRole();
    if (role === 'Instructor') {
      this.form.instructorId = this.tokenStorage.getUserId();
    }
  }

  addQuestion(): void {
    this.questions.push({
      question: '',
      options: ['', '', '', ''],
      correctAnswer: ''
    });
  }

  onSubmit(): void {
    this.errorMsg = '';
    this.form.questionsJson = JSON.stringify(this.questions);
    this.quizService.create(this.form).subscribe({
      next: () => this.router.navigate(['/quizzes']),
      error: (err) => {
        this.errorMsg = err.error?.message ?? 'Failed to create quiz.';
      }
    });
  }
}