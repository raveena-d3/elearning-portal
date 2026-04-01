import { Component, OnInit } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector:    'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls:   ['./sidebar.component.css']
})
export class SidebarComponent implements OnInit {
  role        = '';
  currentLang = 'en';

  constructor(
    private oauthService: OAuthService,
    private translate:    TranslateService
  ) {}

  ngOnInit(): void {
    this.extractRole();
    this.translate.setDefaultLang('en');
    this.translate.use('en');
  }

  private extractRole(): void {
    try {
      const idClaims  = this.oauthService.getIdentityClaims() as any;
      const accessToken = this.oauthService.getAccessToken();

      if (accessToken) {
        const payload   = JSON.parse(atob(accessToken.split('.')[1]));
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

  // ★ Toggle between English and Tamil
  switchLanguage(): void {
    this.currentLang = this.currentLang === 'en' ? 'ta' : 'en';
    this.translate.use(this.currentLang);
  }
}