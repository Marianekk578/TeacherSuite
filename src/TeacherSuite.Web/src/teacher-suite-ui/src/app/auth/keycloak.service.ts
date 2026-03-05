import { Injectable } from '@angular/core';
import Keycloak from 'keycloak-js';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class KeycloakService {
  private keycloak: Keycloak;

  constructor() {
    this.keycloak = new Keycloak({
      url: environment.keycloak.url,
      realm: environment.keycloak.realm,
      clientId: environment.keycloak.clientId,
    });
  }

  async init(): Promise<boolean> {
    const authenticated = await this.keycloak.init({
      onLoad: 'login-required',
      pkceMethod: 'S256',
      checkLoginIframe: false,
    });

    if (authenticated) {
      this.setupTokenRefresh();
    }

    this.clearStaleOidcEntries();

    return authenticated;
  }

  getToken(): string | undefined {
    return this.keycloak.token;
  }

  async updateToken(minValidity = 30): Promise<string> {
    const refreshed = await this.keycloak.updateToken(minValidity);
    if (refreshed) {
      console.debug('Token refreshed');
    }
    return this.keycloak.token ?? '';
  }

  getUserProfile(): Keycloak.KeycloakProfile | undefined {
    return this.keycloak.profile;
  }

  getUsername(): string | undefined {
    return this.keycloak.tokenParsed?.['preferred_username'];
  }

  getRoles(): string[] {
    return this.keycloak.tokenParsed?.['realm_access']?.['roles'] ?? [];
  }

  hasRole(role: string): boolean {
    return this.getRoles().includes(role);
  }

  isAuthenticated(): boolean {
    return !!this.keycloak.authenticated;
  }

  logout(): void {
    this.clearStaleOidcEntries();
    this.keycloak.logout({ redirectUri: window.location.origin });
  }

  getKeycloakInstance(): Keycloak {
    return this.keycloak;
  }

  private setupTokenRefresh(): void {
    this.keycloak.onTokenExpired = () => {
      this.keycloak.updateToken(30).catch(() => {
        console.warn('Token refresh failed, redirecting to login');
        this.keycloak.login();
      });
    };
  }

  private clearStaleOidcEntries(): void {
    Object.keys(localStorage)
      .filter((key) => key.startsWith('kc-callback-'))
      .forEach((key) => localStorage.removeItem(key));
  }
}
