import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home',
  imports: [],
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
