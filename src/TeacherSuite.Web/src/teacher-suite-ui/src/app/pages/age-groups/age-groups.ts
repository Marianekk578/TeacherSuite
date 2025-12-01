import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AgeGroupService, AgeGroup, CreateAgeGroupCommand } from '../../services/age-group.service';

@Component({
  selector: 'app-age-groups',
  imports: [CommonModule, FormsModule],
  templateUrl: './age-groups.html',
  styleUrl: './age-groups.scss',
})
export class AgeGroups implements OnInit {
  ageGroups = signal<AgeGroup[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  
  showModal = signal(false);
  
  formData = signal({
    name: '',
    minAge: 0,
    maxAge: 0
  });

  constructor(private ageGroupService: AgeGroupService) {}

  ngOnInit() {
    this.loadAgeGroups();
  }

  loadAgeGroups() {
    this.loading.set(true);
    this.error.set(null);
    this.ageGroupService.getAllAgeGroups().subscribe({
      next: (data) => {
        this.ageGroups.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load age groups');
        this.loading.set(false);
        console.error('Error loading age groups:', err);
      }
    });
  }

  openCreateModal() {
    this.formData.set({
      name: '',
      minAge: 0,
      maxAge: 0
    });
    this.showModal.set(true);
  }

  closeModal() {
    this.showModal.set(false);
  }

  submitForm() {
    const data = this.formData();
    
    const command: CreateAgeGroupCommand = {
      name: data.name,
      minAge: data.minAge,
      maxAge: data.maxAge
    };
    
    this.ageGroupService.createAgeGroup(command).subscribe({
      next: () => {
        this.closeModal();
        this.loadAgeGroups();
      },
      error: (err) => {
        this.error.set('Failed to create age group');
        console.error('Error creating age group:', err);
      }
    });
  }
}

