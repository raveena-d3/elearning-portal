import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { QuizListComponent } from './quiz-list/quiz-list.component';
import { QuizFormComponent } from './quiz-form/quiz-form.component';
import { QuizAttemptComponent } from './quiz-attempt/quiz-attempt.component';
import { AuthGuard } from '../core/guards/auth.guard';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';

const routes: Routes = [
  { path: '', component: QuizListComponent },
  {
    path: 'new',
    component: QuizFormComponent,
    canActivate: [AuthGuard],
    data: { roles: ['Admin', 'Instructor'] }
  },
  {
    path: ':id/attempt',
    component: QuizAttemptComponent,
    canActivate: [AuthGuard],
    data: { roles: ['Student'] }
  }
];

@NgModule({
  declarations: [
    QuizListComponent,
    QuizFormComponent,
    QuizAttemptComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    RouterModule.forChild(routes),
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatTableModule
  ]
})
export class QuizzesModule {}