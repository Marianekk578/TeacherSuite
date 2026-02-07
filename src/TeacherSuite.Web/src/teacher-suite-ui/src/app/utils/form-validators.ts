import { AbstractControl, ValidationErrors } from '@angular/forms';

/**
 * Validator for date of birth field
 * Ensures the date is in the past and within reasonable bounds
 */
export function dateOfBirthValidator(control: AbstractControl): ValidationErrors | null {
  if (!control.value) {
    return null;
  }

  const date = new Date(control.value);
  if (isNaN(date.getTime())) {
    return { invalidDate: true };
  }

  const today = new Date();
  today.setHours(0, 0, 0, 0);

  if (date >= today) {
    return { futureDate: true };
  }

  const oldestAllowed = new Date();
  oldestAllowed.setFullYear(oldestAllowed.getFullYear() - 122);

  if (date <= oldestAllowed) {
    return { tooOld: true };
  }

  const youngestAllowed = new Date();
  youngestAllowed.setFullYear(youngestAllowed.getFullYear() - 18);

  if (date > youngestAllowed) {
    return { tooYoung: true };
  }

  return null;
}

/**
 * Gets a user-friendly error message for date of birth validation errors
 */
export function getDateOfBirthErrorMessage(errors: ValidationErrors | null): string | null {
  if (!errors) return null;

  if (errors['required']) {
    return 'Date of birth is required';
  }

  if (errors['futureDate']) {
    return 'How can you predict when someone will be born?';
  }

  if (errors['tooOld']) {
    return "I don't think you can beat Jeanne Calment, she lived 122 years.";
  }

  if (errors['tooYoung']) {
    return 'I know students who are older.';
  }

  if (errors['invalidDate']) {
    return 'Date of birth is invalid.';
  }

  return null;
}
