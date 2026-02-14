import { Routes } from '@angular/router';
import { Home } from './pages/home/home';
import { Teachers } from './pages/teachers/teachers';
import { Courses } from './pages/courses/courses';
import { AgeGroups } from './pages/age-groups/age-groups';
import { ProgrammingLanguages } from './pages/programming-languages/programming-languages';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'teachers', component: Teachers },
  { path: 'courses', component: Courses },
  { path: 'age-groups', component: AgeGroups },
  { path: 'programming-languages', component: ProgrammingLanguages },
  { path: '**', redirectTo: '' }
];
