import { Routes } from '@angular/router';
import { Home } from './pages/home/home';
import { Teachers } from './pages/teachers/teachers';
import { Courses } from './pages/courses/courses';
import { AgeGroups } from './pages/age-groups/age-groups';
import { Groups } from './pages/groups/groups';
import { ProgrammingLanguages } from './pages/programming-languages/programming-languages';
import { Students } from './pages/students/students';
import { authGuard } from './auth/auth.guard';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'teachers', component: Teachers, canActivate: [authGuard] },
  { path: 'courses', component: Courses, canActivate: [authGuard] },
  { path: 'groups', component: Groups, canActivate: [authGuard] },
  { path: 'students', component: Students, canActivate: [authGuard] },
  { path: 'age-groups', component: AgeGroups, canActivate: [authGuard] },
  { path: 'programming-languages', component: ProgrammingLanguages, canActivate: [authGuard] },
  { path: '**', redirectTo: '' }
];
