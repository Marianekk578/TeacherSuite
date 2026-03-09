import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { KeycloakService } from './keycloak.service';

export const authGuard: CanActivateFn = () => {
  const keycloakService = inject(KeycloakService);
  const router = inject(Router);

  if (keycloakService.isAuthenticated()) {
    return true;
  }

  router.navigate(['/']);
  return false;
};

export function roleGuard(...requiredRoles: string[]): CanActivateFn {
  return () => {
    const keycloakService = inject(KeycloakService);

    if (!keycloakService.isAuthenticated()) {
      return false;
    }

    return requiredRoles.some((role) => keycloakService.hasRole(role));
  };
}
