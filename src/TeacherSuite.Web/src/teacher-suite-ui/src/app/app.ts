import { Component, signal } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import {
  heroAcademicCap,
  heroBookOpen,
  heroClipboardDocumentList,
  heroUserGroup,
  heroUsers,
  heroCodeBracket,
  heroUserCircle,
  heroCalendarDays,
} from '@ng-icons/heroicons/outline';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, NgIconComponent],
  providers: [provideIcons({ heroAcademicCap, heroBookOpen, heroClipboardDocumentList, heroUserGroup, heroUsers, heroCodeBracket, heroUserCircle, heroCalendarDays })],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('TeacherSuite');
}
