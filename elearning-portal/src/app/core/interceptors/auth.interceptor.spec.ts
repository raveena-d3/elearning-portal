import { TestBed } from '@angular/core/testing';
import {
  HttpClientTestingModule,
  HttpTestingController
} from '@angular/common/http/testing';
import {
  HTTP_INTERCEPTORS,
  HttpClient
} from '@angular/common/http';

import { OAuthService } from 'angular-oauth2-oidc';
import { AuthInterceptor } from './auth.interceptor';

describe('AuthInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;

  let mockOAuthService: any;

  beforeEach(() => {
    mockOAuthService = {
      getAccessToken: jasmine.createSpy('getAccessToken')
    };

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        { provide: OAuthService, useValue: mockOAuthService },
        {
          provide: HTTP_INTERCEPTORS,
          useClass: AuthInterceptor,
          multi: true
        }
      ]
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should add Authorization header when token exists', () => {
    mockOAuthService.getAccessToken.and.returnValue('test-token');

    http.get('/test').subscribe();

    const req = httpMock.expectOne('/test');

    expect(req.request.headers.has('Authorization')).toBeTrue();
    expect(req.request.headers.get('Authorization'))
      .toBe('Bearer test-token');

    req.flush({});
  });

  it('should NOT add Authorization header when token is null', () => {
    mockOAuthService.getAccessToken.and.returnValue(null);

    http.get('/test').subscribe();

    const req = httpMock.expectOne('/test');

    expect(req.request.headers.has('Authorization')).toBeFalse();

    req.flush({});
  });
});