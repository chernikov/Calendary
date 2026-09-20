import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { beforeEach, afterEach, describe, expect, it } from 'vitest';
import { AuthService } from './auth.service';
import { UserDto } from './models';

const STORAGE_KEY = 'calendary.session';

const USER: UserDto = {
  id: 'user-1',
  displayName: 'Тест',
  email: 'test@example.com',
  emailConfirmed: true,
  role: 'Customer',
  lastDelivery: null,
  verifiedPhone: null,
};

describe('AuthService', () => {
  let httpMock: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('starts unauthenticated with no stored session', () => {
    const service = TestBed.inject(AuthService);
    expect(service.isAuthenticated()).toBe(false);
    expect(service.user()).toBeNull();
  });

  it('restores a session from localStorage on construction', () => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify({ bearerToken: 'stored-token', user: USER }));
    const service = TestBed.inject(AuthService);
    expect(service.isAuthenticated()).toBe(true);
    expect(service.bearerToken).toBe('stored-token');
    expect(service.user()).toEqual(USER);
  });

  it('treats corrupt localStorage content as no session rather than throwing', () => {
    localStorage.setItem(STORAGE_KEY, '{not valid json');
    const service = TestBed.inject(AuthService);
    expect(service.isAuthenticated()).toBe(false);
  });

  it('login() stores the returned session and persists it to localStorage', () => {
    const service = TestBed.inject(AuthService);

    let result: unknown;
    service.login('test@example.com', 'password123').subscribe((res) => (result = res));

    const req = httpMock.expectOne('/api/auth/login');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ email: 'test@example.com', password: 'password123' });
    req.flush({ bearerToken: 'new-token', user: USER });

    expect(result).toEqual({ bearerToken: 'new-token', user: USER });
    expect(service.isAuthenticated()).toBe(true);
    expect(service.bearerToken).toBe('new-token');
    expect(JSON.parse(localStorage.getItem(STORAGE_KEY)!)).toEqual({ bearerToken: 'new-token', user: USER });
  });

  it('register() posts displayName/email/password and stores the session', () => {
    const service = TestBed.inject(AuthService);

    service.register('test@example.com', 'password123', 'Тест').subscribe();

    const req = httpMock.expectOne('/api/auth/register');
    expect(req.request.body).toEqual({ email: 'test@example.com', password: 'password123', displayName: 'Тест' });
    req.flush({ bearerToken: 'reg-token', user: USER });

    expect(service.bearerToken).toBe('reg-token');
  });

  it('logout() clears the signal and localStorage', () => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify({ bearerToken: 'stored-token', user: USER }));
    const service = TestBed.inject(AuthService);
    expect(service.isAuthenticated()).toBe(true);

    service.logout();

    expect(service.isAuthenticated()).toBe(false);
    expect(service.bearerToken).toBeNull();
    expect(localStorage.getItem(STORAGE_KEY)).toBeNull();
  });

  it('needsEmailConfirmation reflects the stored user emailConfirmed flag', () => {
    localStorage.setItem(
      STORAGE_KEY,
      JSON.stringify({ bearerToken: 'token', user: { ...USER, emailConfirmed: false } }),
    );
    const service = TestBed.inject(AuthService);
    expect(service.needsEmailConfirmation()).toBe(true);
  });

  it('confirmEmail() updates the stored user in place without changing the bearer token', () => {
    localStorage.setItem(
      STORAGE_KEY,
      JSON.stringify({ bearerToken: 'token', user: { ...USER, emailConfirmed: false } }),
    );
    const service = TestBed.inject(AuthService);

    service.confirmEmail('1234').subscribe();

    const req = httpMock.expectOne('/api/auth/confirm-email');
    expect(req.request.body).toEqual({ code: '1234' });
    req.flush({ ...USER, emailConfirmed: true });

    expect(service.needsEmailConfirmation()).toBe(false);
    expect(service.bearerToken).toBe('token');
  });
});
