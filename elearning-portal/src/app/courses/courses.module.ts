// src/app/courses/courses.module.ts
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { CourseListComponent } from './course-list/course-list.component';
import { CourseFormComponent } from './course-form/course-form.component';
import { CourseDetailComponent } from './course-detail/course-detail.component';
import { AuthGuard } from '../core/guards/auth.guard';
import { HttpClientModule } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatListModule } from '@angular/material/list';
import { SafeUrlPipe } from 'src/app/core/pipes/safe-url.pipe';
const routes: Routes = [
  { path: '', component: CourseListComponent },
  { path: 'new', component: CourseFormComponent, canActivate: [AuthGuard], data: { roles: ['Admin'] } },
  { path: ':id', component: CourseDetailComponent },
  { path: ':id/edit', component: CourseFormComponent, canActivate: [AuthGuard], data: { roles: ['Admin', 'Instructor'] } }
];

@NgModule({
  declarations: [CourseListComponent, CourseFormComponent, CourseDetailComponent,SafeUrlPipe ],
  imports: [
    CommonModule,
    FormsModule,
    HttpClientModule,
    RouterModule.forChild(routes),
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatListModule,
  ]
})
export class CoursesModule {}