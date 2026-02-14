import { Component, OnInit, ChangeDetectorRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ProgrammingLanguageService, ProgrammingLanguage, CreateProgrammingLanguageDto, UpdateProgrammingLanguageDto } from '../../services/programming-language.service';

@Component({
  selector: 'app-programming-languages',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './programming-languages.html',
  styleUrl: './programming-languages.scss',
})
export class ProgrammingLanguages implements OnInit {
  programmingLanguages: ProgrammingLanguage[] = [];
  loading = false;
  error: string | null = null;

  showModal = false;
  isEditMode = false;
  currentLanguageId: number | null = null;
  modalError: string | null = null;

  languageForm: FormGroup;

  showDeleteConfirm = false;
  languageToDelete: ProgrammingLanguage | null = null;

  constructor(
    private programmingLanguageService: ProgrammingLanguageService,
    private cdr: ChangeDetectorRef,
    private fb: FormBuilder
  ) {
    this.languageForm = this.fb.group({
      name: ['', [Validators.required]]
    });
  }

  ngOnInit() {
    this.loadProgrammingLanguages();
  }

  loadProgrammingLanguages() {
    this.loading = true;
    this.error = null;
    this.cdr.detectChanges();

    this.programmingLanguageService.getAllProgrammingLanguages().subscribe({
      next: (languages) => {
        this.programmingLanguages = languages;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.error = 'Failed to load programming languages. Please try again.';
        this.loading = false;
        console.error('Error loading programming languages:', error);
        this.cdr.detectChanges();
      }
    });
  }

  openAddModal() {
    this.isEditMode = false;
    this.currentLanguageId = null;
    this.modalError = null;
    this.languageForm.reset({
      name: ''
    });
    this.showModal = true;
  }

  openEditModal(language: ProgrammingLanguage) {
    this.isEditMode = true;
    this.currentLanguageId = language.id;
    this.modalError = null;
    this.languageForm.reset({
      name: language.name
    });
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
    this.isEditMode = false;
    this.currentLanguageId = null;
    this.modalError = null;
  }

  @HostListener('document:keydown.escape', ['$event'])
  onEscapeKey(event: Event) {
    if (this.showModal) {
      this.closeModal();
    }
    if (this.showDeleteConfirm) {
      this.cancelDelete();
    }
  }

  saveLanguage() {
    this.modalError = null;

    if (this.languageForm.invalid) {
      this.languageForm.markAllAsTouched();
      this.modalError = this.getFormErrorMessage();
      this.cdr.detectChanges();
      return;
    }

    const languagePayload = this.languageForm.getRawValue() as CreateProgrammingLanguageDto | UpdateProgrammingLanguageDto;

    if (this.isEditMode && this.currentLanguageId !== null) {
      this.programmingLanguageService.updateProgrammingLanguage(this.currentLanguageId, languagePayload).subscribe({
        next: () => {
          this.loadProgrammingLanguages();
          this.closeModal();
        },
        error: (error) => {
          this.modalError = 'Failed to update programming language. Please check your input and try again.';
          console.error('Error updating programming language:', error);
          this.cdr.detectChanges();
        }
      });
    } else {
      this.programmingLanguageService.createProgrammingLanguage(languagePayload).subscribe({
        next: () => {
          this.loadProgrammingLanguages();
          this.closeModal();
        },
        error: (error) => {
          this.modalError = 'Failed to create programming language. Please check your input and try again.';
          console.error('Error creating programming language:', error);
          this.cdr.detectChanges();
        }
      });
    }
  }

  confirmDelete(language: ProgrammingLanguage) {
    this.languageToDelete = language;
    this.showDeleteConfirm = true;
  }

  cancelDelete() {
    this.languageToDelete = null;
    this.showDeleteConfirm = false;
  }

  deleteLanguage() {
    if (this.languageToDelete) {
      this.programmingLanguageService.deleteProgrammingLanguage(this.languageToDelete.id).subscribe({
        next: () => {
          this.loadProgrammingLanguages();
          this.cancelDelete();
        },
        error: (error) => {
          this.error = 'Failed to delete programming language. Please try again.';
          console.error('Error deleting programming language:', error);
          this.cancelDelete();
        }
      });
    }
  }

  private getFormErrorMessage(): string | null {
    const controls = this.languageForm.controls;

    if (controls['name']?.errors?.['required']) {
      return 'Programming language name is required';
    }

    return 'Please fix the errors in the form.';
  }
}
