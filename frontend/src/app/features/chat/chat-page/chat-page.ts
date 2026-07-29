import { ChangeDetectionStrategy, Component, ElementRef, OnInit, ViewChild, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { BreakpointObserver } from '@angular/cdk/layout';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MarkdownComponent } from 'ngx-markdown';
import { AuthStore } from '../../../core/auth/auth.store';
import { AiService } from '../../../core/api/ai.service';
import { CrawlService } from '../../../core/api/crawl.service';
import { NotificationService } from '../../../core/http/notification.service';
import { ChatStore } from '../chat.store';
import { VehicleResults } from '../vehicle-results/vehicle-results';
import { RelativeTimePipe } from '../../../shared/relative-time.pipe';
import { ChatMessageDto, VehicleDto } from '../../../core/api/models';

const EXAMPLE_PROMPTS = [
  'Szukam kombi, rocznik 2012+, do 160 000 zł',
  'Automat benzynowy do miasta, przebieg do 150000 km',
  'Znajdź auta na diesla o mocy powyżej 100 KM'
];

@Component({
  selector: 'app-chat-page',
  standalone: true,
  imports: [
    FormsModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatSelectModule,
    MatSidenavModule,
    MatListModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    MarkdownComponent,
    VehicleResults,
    RelativeTimePipe
  ],
  templateUrl: './chat-page.html',
  styleUrl: './chat-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChatPage implements OnInit {
  protected readonly authStore = inject(AuthStore);
  protected readonly chatStore = inject(ChatStore);
  private readonly aiService = inject(AiService);
  private readonly crawlService = inject(CrawlService);
  private readonly notifications = inject(NotificationService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly breakpointObserver = inject(BreakpointObserver);

  @ViewChild('messagesEnd') private messagesEnd?: ElementRef<HTMLDivElement>;

  protected readonly examplePrompts = EXAMPLE_PROMPTS;
  protected readonly draft = signal('');
  protected readonly isHandset = toSignal(
    this.breakpointObserver.observe('(max-width: 768px)').pipe(map((state) => state.matches)),
    { initialValue: false }
  );
  protected readonly sidebarOpen = signal(true);
  protected readonly models = signal<string[]>([]);
  protected readonly selectedModel = signal<string | null>(null);
  protected readonly aiAvailable = signal(true);
  protected readonly crawling = signal(false);

  protected readonly isEmpty = computed(() => this.chatStore.messages().length === 0);
  protected readonly sidenavMode = computed(() => (this.isHandset() ? 'over' : 'side'));

  constructor() {
    effect(() => {
      this.chatStore.messages();
      queueMicrotask(() => this.scrollToBottom());
    });
  }

  async ngOnInit(): Promise<void> {
    await this.chatStore.loadSessions();

    const sessionId = this.route.snapshot.paramMap.get('sessionId');
    if (sessionId) {
      await this.chatStore.openSession(sessionId);
    }

    try {
      const [status, models] = await Promise.all([
        this.aiService.getStatus(),
        this.aiService.getModels()
      ]);
      this.aiAvailable.set(status.available);
      this.models.set(models.models);
      this.selectedModel.set(models.defaultModel);
    } catch {
      this.aiAvailable.set(false);
    }
  }

  protected resultsFor(message: ChatMessageDto): VehicleDto[] | undefined {
    return this.chatStore.resultsByMessageId().get(message.id);
  }

  protected toggleSidebar(): void {
    this.sidebarOpen.update((open) => !open);
  }

  protected async newChat(): Promise<void> {
    const id = await this.chatStore.createSession();
    await this.router.navigate(['/chat', id]);
    await this.chatStore.openSession(id);
    this.closeOnMobile();
  }

  protected async selectSession(sessionId: string): Promise<void> {
    await this.router.navigate(['/chat', sessionId]);
    await this.chatStore.openSession(sessionId);
    this.closeOnMobile();
  }

  private closeOnMobile(): void {
    if (this.isHandset()) {
      this.sidebarOpen.set(false);
    }
  }

  protected async deleteSession(sessionId: string, event: Event): Promise<void> {
    event.stopPropagation();
    if (!confirm('Usunąć tę sesję czatu?')) {
      return;
    }
    await this.chatStore.deleteSession(sessionId);
    if (this.chatStore.activeSessionId() === null) {
      await this.router.navigate(['/chat']);
    }
  }

  protected async send(): Promise<void> {
    const content = this.draft().trim();
    if (!content || this.chatStore.sending()) {
      return;
    }

    let sessionId = this.chatStore.activeSessionId();
    if (!sessionId) {
      sessionId = await this.chatStore.createSession();
      await this.router.navigate(['/chat', sessionId]);
      await this.chatStore.openSession(sessionId);
    }

    this.draft.set('');
    await this.chatStore.sendMessage(sessionId, content);
  }

  protected useExample(prompt: string): void {
    this.draft.set(prompt);
  }

  protected onComposerKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.send();
    }
  }

  protected async triggerCrawl(): Promise<void> {
    this.crawling.set(true);
    try {
      const result = await this.crawlService.runCrawl();
      this.notifications.info(
        `Crawl ${result.status}: znaleziono ${result.itemsFound}, zapisano ${result.itemsSaved}.`
      );
    } catch {
      // errorInterceptor already surfaces a snackbar
    } finally {
      this.crawling.set(false);
    }
  }

  protected logout(): void {
    this.authStore.logout();
    this.router.navigate(['/login']);
  }

  private scrollToBottom(): void {
    this.messagesEnd?.nativeElement.scrollIntoView({ behavior: 'smooth' });
  }
}
