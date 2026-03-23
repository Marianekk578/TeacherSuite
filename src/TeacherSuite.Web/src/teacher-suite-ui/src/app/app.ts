import { Component, signal } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import {
  heroAcademicCap,
  heroBookOpen,
  heroUserGroup,
  heroUsers,
  heroCodeBracket,
  heroUserCircle,
} from '@ng-icons/heroicons/outline';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, NgIconComponent],
  providers: [provideIcons({ heroAcademicCap, heroBookOpen, heroUserGroup, heroUsers, heroCodeBracket, heroUserCircle })],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('TeacherSuite');
}
