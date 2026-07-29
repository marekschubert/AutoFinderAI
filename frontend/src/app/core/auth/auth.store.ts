import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResult, LoginRequest, RegisterRequest } from '../api/models';
import { ChatStore } from '../../features/chat/chat.store';

const TOKEN_KEY = 'autofinder_token';
const EMAIL_KEY = 'autofinder_email';

@Injectable({ providedIn: 'root' })
export class AuthStore {
  private readonly http = inject(HttpClient);
  private readonly chatStore = inject(ChatStore);

  private readonly _token = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  private readonly _email = signal<string | null>(localStorage.getItem(EMAIL_KEY));

  readonly token = this._token.asReadonly();
  readonly email = this._email.asReadonly();
  readonly isAuthenticated = computed(() => this._token() !== null);

  async login(request: LoginRequest): Promise<void> {
    const result = await firstValueFrom(
      this.http.post<AuthResult>(`${environment.apiBaseUrl}/auth/login`, request)
    );
    this.setSession(result);
  }

  async register(request: RegisterRequest): Promise<void> {
    const result = await firstValueFrom(
      this.http.post<AuthResult>(`${environment.apiBaseUrl}/auth/register`, request)
    );
    this.setSession(result);
  }

  logout(): void {
    this._token.set(null);
    this._email.set(null);
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(EMAIL_KEY);
    // Critical: clear the previous user's chat state (sessions/messages/results) so the next
    // person to log in on this browser never sees a stale conversation from another account.
    this.chatStore.reset();
  }

  private setSession(result: AuthResult): void {
    this._token.set(result.token);
    this._email.set(result.email);
    localStorage.setItem(TOKEN_KEY, result.token);
    localStorage.setItem(EMAIL_KEY, result.email);
  }
}
