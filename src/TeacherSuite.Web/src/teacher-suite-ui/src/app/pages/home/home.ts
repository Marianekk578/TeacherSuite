import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import {
  heroAcademicCap,
  heroBookOpen,
  heroUserGroup,
  heroUsers,
  heroCodeBracket,
} from '@ng-icons/heroicons/outline';

@Component({
  selector: 'app-home',
  imports: [NgIconComponent],
  providers: [provideIcons({ heroAcademicCap, heroBookOpen, heroUserGroup, heroUsers, heroCodeBracket })],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {
  constructor(private router: Router) {}

  navigateTo(path: string): void {
    this.router.navigate([path]);
  }

  handleKeyboardNavigation(event: KeyboardEvent, path: string): void {
    if (event.key === 'Enter') {
      event.preventDefault();
      this.navigateTo(path);
    }
  }
}
