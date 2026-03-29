import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';

import { NavbarComponent } from './navbar.component';

describe('NavbarComponent', () => {
  let component: NavbarComponent;
  let fixture: ComponentFixture<NavbarComponent>;

  let mockOAuthService: any;

  beforeEach(async () => {
    const payload = {
      preferred_username: 'raveena',
      realm_access: {
        roles: ['Instructor']
      }
    };

    const token =
      'header.' + btoa(JSON.stringify(payload)) + '.signature';

    mockOAuthService = {
      getAccessToken: jasmine.createSpy('getAccessToken').and.returnValue(token),
      revokeTokenAndLogout: jasmine.createSpy('revokeTokenAndLogout').and.returnValue(Promise.resolve()),
      logOut: jasmine.createSpy('logOut')
    };

    await TestBed.configureTestingModule({
      declarations: [NavbarComponent],
      providers: [
        { provide: OAuthService, useValue: mockOAuthService }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(NavbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load username and role from token on init', () => {
    expect(mockOAuthService.getAccessToken).toHaveBeenCalled();
    expect(component.username).toBe('raveena');
    expect(component.role).toBe('Instructor');
  });

  it('should emit toggleSidenav event', () => {
    spyOn(component.toggleSidenav, 'emit');

    component.toggleSidenav.emit();

    expect(component.toggleSidenav.emit).toHaveBeenCalled();
  });

  it('should handle invalid token without crashing', () => {
    spyOn(console, 'error');
    mockOAuthService.getAccessToken.and.returnValue('invalid.token');

    fixture = TestBed.createComponent(NavbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(console.error).toHaveBeenCalled();
  });

  it('should call revokeTokenAndLogout on logout', async () => {
    await component.logout();

    expect(mockOAuthService.revokeTokenAndLogout).toHaveBeenCalled();
  });

  
});