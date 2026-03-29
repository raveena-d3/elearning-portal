import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';

import { SidebarComponent } from './sidebar.component';

describe('SidebarComponent', () => {
  let component: SidebarComponent;
  let fixture: ComponentFixture<SidebarComponent>;

  let mockOAuthService: any;

  beforeEach(async () => {
    const payload = {
      realm_access: {
        roles: ['Admin']
      }
    };

    const token =
      'header.' + btoa(JSON.stringify(payload)) + '.signature';

    mockOAuthService = {
      getAccessToken: jasmine.createSpy('getAccessToken').and.returnValue(token),
      getIdentityClaims: jasmine.createSpy('getIdentityClaims').and.returnValue(null)
    };

    await TestBed.configureTestingModule({
      declarations: [SidebarComponent],
      providers: [
        { provide: OAuthService, useValue: mockOAuthService }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(SidebarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should extract role from access token', () => {
    expect(mockOAuthService.getAccessToken).toHaveBeenCalled();
    expect(component.role).toBe('Admin');
  });

  it('should extract role from identity claims if access token has no role', () => {
    mockOAuthService.getAccessToken.and.returnValue(null);
    mockOAuthService.getIdentityClaims.and.returnValue({
      realm_access: {
        roles: ['Instructor']
      }
    });

    fixture = TestBed.createComponent(SidebarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.role).toBe('Instructor');
  });

  it('should handle invalid token without crashing', () => {
    spyOn(console, 'error');
    mockOAuthService.getAccessToken.and.returnValue('invalid.token');

    fixture = TestBed.createComponent(SidebarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(console.error).toHaveBeenCalled();
  });

  it('should set empty role if no valid role found', () => {
    const payload = {
      realm_access: {
        roles: ['UnknownRole']
      }
    };

    const token =
      'header.' + btoa(JSON.stringify(payload)) + '.signature';

    mockOAuthService.getAccessToken.and.returnValue(token);

    fixture = TestBed.createComponent(SidebarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.role).toBe('');
  });
});