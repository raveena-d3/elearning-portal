import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { MqttDashboardComponent } from './components/mqtt-dashboard/mqtt-dashboard.component';

const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'dashboard',
    canActivate: [AuthGuard],
    loadChildren: () =>
      import('./dashboard/dashboard.module').then(m => m.DashboardModule)
  },
  {
    path: 'courses',
    canActivate: [AuthGuard],
    loadChildren: () =>
      import('./courses/courses.module').then(m => m.CoursesModule)
  },
  {
    path: 'enrollments',
    canActivate: [AuthGuard],
    loadChildren: () =>
      import('./enrollments/enrollments.module').then(m => m.EnrollmentsModule)
  },
  {
    path: 'quizzes',
    canActivate: [AuthGuard],
    loadChildren: () =>
      import('./quizzes/quizzes.module').then(m => m.QuizzesModule)
  },
  {
    path: 'discussions',
    canActivate: [AuthGuard],
    loadChildren: () =>
      import('./discussions/discussions.module').then(m => m.DiscussionsModule)
  },
  {
    path: 'users',
    canActivate: [AuthGuard],
    data: { roles: ['Admin'] },
    loadChildren: () =>
      import('./users/users.module').then(m => m.UsersModule)
  },
  { path: 'live', canActivate: [AuthGuard], data:{roles:['Admin']},component: MqttDashboardComponent },
  { path: '**', redirectTo: 'dashboard' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}