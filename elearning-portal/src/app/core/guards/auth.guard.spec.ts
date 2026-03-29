import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';

import { AuthGuard } from './auth.guard';

describe('AuthGuard', () => {
  let guard: AuthGuard;

  let mockOAuthService: any;
  let mockRouter: any;

  beforeEach(() => {
    mockOAuthService = {
      hasValidAccessToken: jasmine.createSpy('hasValidAccessToken'),
      initLoginFlow: jasmine.createSpy('initLoginFlow')
    };

    mockRouter = {
      navigate: jasmine.createSpy('navigate')
    };

    TestBed.configureTestingModule({
      providers: [
        AuthGuard,
        { provide: OAuthService, useValue: mockOAuthService },
        { provide: Router, useValue: mockRouter }
      ]
    });

    guard = TestBed.inject(AuthGuard);
  });

  it('should be created', () => {
    expect(guard).toBeTruthy();
  });

  it('should return true when access token is valid', () => {
    mockOAuthService.hasValidAccessToken.and.returnValue(true);

    const result = guard.canActivate();

    expect(result).toBeTrue();
    expect(mockOAuthService.hasValidAccessToken).toHaveBeenCalled();
    expect(mockOAuthService.initLoginFlow).not.toHaveBeenCalled();
  });

  it('should return false and start login flow when access token is invalid', () => {
    mockOAuthService.hasValidAccessToken.and.returnValue(false);

    const result = guard.canActivate();

    expect(result).toBeFalse();
    expect(mockOAuthService.hasValidAccessToken).toHaveBeenCalled();
    expect(mockOAuthService.initLoginFlow).toHaveBeenCalled();
  });
});