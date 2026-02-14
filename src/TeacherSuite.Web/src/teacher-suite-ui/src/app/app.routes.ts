import { Routes } from '@angular/router';
import { Home } from './pages/home/home';
import { Teachers } from './pages/teachers/teachers';
import { Courses } from './pages/courses/courses';
import { AgeGroups } from './pages/age-groups/age-groups';
import { Groups } from './pages/groups/groups';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'teachers', component: Teachers },
  { path: 'courses', component: Courses },
  { path: 'groups', component: Groups },
  { path: 'age-groups', component: AgeGroups },
  { path: '**', redirectTo: '' }
];
