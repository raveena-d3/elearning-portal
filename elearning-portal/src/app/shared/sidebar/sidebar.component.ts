import { Component, OnInit } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';


@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent implements OnInit {
  role = '';

  constructor(private oauthService: OAuthService) {}

  ngOnInit(): void {
    this.extractRole();
  }

  private extractRole(): void {
    try {
      // Try ID token claims first
      const idClaims = this.oauthService.getIdentityClaims() as any;
      
      // Try access token
      const accessToken = this.oauthService.getAccessToken();
      if (accessToken) {
        const payload = JSON.parse(atob(accessToken.split('.')[1]));
        console.log('Access token payload:', payload);
        
        const roles: string[] = payload?.realm_access?.roles ?? [];
        this.role = roles.find(r =>
          ['Admin', 'Instructor', 'Student'].includes(r)
        ) ?? '';
      }

      if (!this.role && idClaims) {
        const roles: string[] = idClaims?.realm_access?.roles ?? [];
        this.role = roles.find((r: string) =>
          ['Admin', 'Instructor', 'Student'].includes(r)
        ) ?? '';
      }

      console.log('Sidebar role:', this.role);
    } catch (e) {
      console.error('Role extraction error:', e);
    }
  }
}