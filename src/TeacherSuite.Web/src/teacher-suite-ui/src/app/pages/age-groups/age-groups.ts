import { Component } from '@angular/core';
import { NgIconComponent, provideIcons } from '@ng-icons/core';
import { heroUsers } from '@ng-icons/heroicons/outline';

@Component({
  selector: 'app-age-groups',
  imports: [NgIconComponent],
  providers: [provideIcons({ heroUsers })],
  templateUrl: './age-groups.html',
  styleUrl: './age-groups.scss',
})
export class AgeGroups {

}
