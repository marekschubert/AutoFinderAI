import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AiModels, AiStatus } from './models';

@Injectable({ providedIn: 'root' })
export class AiService {
  private readonly http = inject(HttpClient);

  getStatus(): Promise<AiStatus> {
    return firstValueFrom(this.http.get<AiStatus>(`${environment.apiBaseUrl}/ai/status`));
  }

  getModels(): Promise<AiModels> {
    return firstValueFrom(this.http.get<AiModels>(`${environment.apiBaseUrl}/ai/models`));
  }
}
