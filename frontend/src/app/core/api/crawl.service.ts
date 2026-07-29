import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CrawlRunDto } from './models';

export interface RunCrawlResult {
  crawlRunId: string;
  status: string;
  itemsFound: number;
  itemsSaved: number;
  error: string | null;
}

@Injectable({ providedIn: 'root' })
export class CrawlService {
  private readonly http = inject(HttpClient);

  runCrawl(): Promise<RunCrawlResult> {
    return firstValueFrom(
      this.http.post<RunCrawlResult>(`${environment.apiBaseUrl}/crawl/runs`, {})
    );
  }

  getRuns(take = 10): Promise<CrawlRunDto[]> {
    return firstValueFrom(
      this.http.get<CrawlRunDto[]>(`${environment.apiBaseUrl}/crawl/runs?take=${take}`)
    );
  }
}
