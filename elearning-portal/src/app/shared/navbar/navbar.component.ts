import { Component, OnInit, EventEmitter, Output } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css']
})
export class NavbarComponent implements OnInit {
  @Output() toggleSidenav = new EventEmitter<void>();
  username = '';
  role = '';

  constructor(private oauthService: OAuthService) {}

  ngOnInit(): void {
    try {
      const accessToken = this.oauthService.getAccessToken();
      if (accessToken) {
        const payload = JSON.parse(atob(accessToken.split('.')[1]));
        this.username = payload?.preferred_username ?? '';
        const roles: string[] = payload?.realm_access?.roles ?? [];
        this.role = roles.find(r =>
          ['Admin', 'Instructor', 'Student'].includes(r)
        ) ?? '';
      }
      console.log('Navbar - username:', this.username, 'role:', this.role);
    } catch (e) {
      console.error('Navbar error:', e);
    }
  }

  logout(): void {
  this.oauthService.revokeTokenAndLogout().catch(() => {
    this.oauthService.logOut(true); // true = no redirect
    window.location.href = '/';
  });
}
}