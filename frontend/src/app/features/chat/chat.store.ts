import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ChatMessageDto,
  ChatSessionDetailDto,
  ChatSessionSummaryDto,
  CreateSessionResult,
  SendMessageResult,
  VehicleDto
} from '../../core/api/models';

@Injectable({ providedIn: 'root' })
export class ChatStore {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/chat`;

  private readonly _sessions = signal<ChatSessionSummaryDto[]>([]);
  private readonly _activeSessionId = signal<string | null>(null);
  private readonly _messages = signal<ChatMessageDto[]>([]);
  private readonly _resultsByMessageId = signal<Map<string, VehicleDto[]>>(new Map());
  private readonly _sessionsLoading = signal(false);
  private readonly _sessionLoading = signal(false);
  private readonly _sending = signal(false);

  readonly sessions = this._sessions.asReadonly();
  readonly activeSessionId = this._activeSessionId.asReadonly();
  readonly messages = this._messages.asReadonly();
  readonly sessionsLoading = this._sessionsLoading.asReadonly();
  readonly sessionLoading = this._sessionLoading.asReadonly();
  readonly sending = this._sending.asReadonly();
  readonly resultsByMessageId = this._resultsByMessageId.asReadonly();

  readonly activeSession = computed(() =>
    this._sessions().find((s) => s.id === this._activeSessionId()) ?? null
  );

  async loadSessions(): Promise<void> {
    this._sessionsLoading.set(true);
    try {
      const sessions = await firstValueFrom(
        this.http.get<ChatSessionSummaryDto[]>(`${this.baseUrl}/sessions`)
      );
      this._sessions.set(sessions);
    } finally {
      this._sessionsLoading.set(false);
    }
  }

  async createSession(): Promise<string> {
    const result = await firstValueFrom(
      this.http.post<CreateSessionResult>(`${this.baseUrl}/sessions`, {})
    );
    this._sessions.update((sessions) => [
      { id: result.id, title: result.title, createdAt: result.createdAt, lastMessageAt: result.lastMessageAt },
      ...sessions
    ]);
    return result.id;
  }

  async openSession(sessionId: string): Promise<void> {
    this._activeSessionId.set(sessionId);
    this._sessionLoading.set(true);
    this._messages.set([]);
    this._resultsByMessageId.set(new Map());
    try {
      const detail = await firstValueFrom(
        this.http.get<ChatSessionDetailDto>(`${this.baseUrl}/sessions/${sessionId}`)
      );
      this._messages.set(detail.messages);
      this.applyMessageResults(detail.messages);
    } finally {
      this._sessionLoading.set(false);
    }
  }

  private applyMessageResults(messages: ChatMessageDto[]): void {
    const withResults = messages.filter((m) => m.results && m.results.length > 0);
    if (withResults.length === 0) {
      return;
    }
    this._resultsByMessageId.update((map) => {
      const next = new Map(map);
      for (const message of withResults) {
        next.set(message.id, message.results!);
      }
      return next;
    });
  }

  async deleteSession(sessionId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${this.baseUrl}/sessions/${sessionId}`));
    this._sessions.update((sessions) => sessions.filter((s) => s.id !== sessionId));
    if (this._activeSessionId() === sessionId) {
      this._activeSessionId.set(null);
      this._messages.set([]);
    }
  }

  async sendMessage(sessionId: string, content: string): Promise<void> {
    const userMessage: ChatMessageDto = {
      id: `pending-${Date.now()}`,
      role: 'User',
      content,
      criteriaJson: null,
      resultVehicleIdsJson: null,
      modelUsed: null,
      createdAt: new Date().toISOString()
    };
    this._messages.update((messages) => [...messages, userMessage]);
    this._sending.set(true);
    try {
      const result = await firstValueFrom(
        this.http.post<SendMessageResult>(`${this.baseUrl}/sessions/${sessionId}/messages`, {
          content
        })
      );
      this._messages.update((messages) => [...messages, result.assistantMessage]);
      if (result.results.length > 0) {
        this.applyMessageResults([{ ...result.assistantMessage, results: result.results }]);
      }
      await this.loadSessions();
    } finally {
      this._sending.set(false);
    }
  }

  reset(): void {
    this._sessions.set([]);
    this._activeSessionId.set(null);
    this._messages.set([]);
    this._resultsByMessageId.set(new Map());
  }
}
