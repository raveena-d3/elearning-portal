import { Injectable } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';

@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  constructor(private oauthService: OAuthService) {}

  getToken(): string {
    return this.oauthService.getAccessToken() ?? '';
  }

  private getAccessTokenPayload(): any {
    try {
      const token = this.oauthService.getAccessToken();
      if (!token) return null;
      return JSON.parse(atob(token.split('.')[1]));
    } catch { return null; }
  }

  getRole(): string {
    const payload = this.getAccessTokenPayload();
    const roles: string[] = payload?.realm_access?.roles ?? [];
    return roles.find(r =>
      ['Admin', 'Instructor', 'Student'].includes(r)
    ) ?? '';
  }

  getUsername(): string {
    const payload = this.getAccessTokenPayload();
    return payload?.preferred_username ?? '';
  }

  getUserId(): number {
    const payload = this.getAccessTokenPayload();
    // Keycloak does not store DB userId in token
    // We need to get it from the database via username
    return payload?.db_id ?? 0;
}

  isLoggedIn(): boolean {
    return this.oauthService.hasValidAccessToken();
  }

  clear(): void {
    this.oauthService.logOut();
  }

  saveToken(token: string): void {}
  saveUser(user: any): void {}
}