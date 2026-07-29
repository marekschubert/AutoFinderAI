using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Application.Ai.Composition;
using AutoFinderAI.Application.Ai.CriteriaExtraction;
using AutoFinderAI.Application.Ai.Ranking;
using AutoFinderAI.Infrastructure.Ai;
using AutoFinderAI.Infrastructure.Common;
using AutoFinderAI.Infrastructure.Crawling;
using AutoFinderAI.Infrastructure.Crawling.Otomoto;
using AutoFinderAI.Infrastructure.Identity;
using AutoFinderAI.Infrastructure.Options;
using AutoFinderAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AutoFinderAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default") ?? "Data Source=autofinder.db";

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        services.AddOptions<AiOptions>()
            .Bind(configuration.GetSection(AiOptions.SectionName))
            .PostConfigure(options =>
            {
                // Rule: the API key is never sourced from appsettings.json. It comes from the
                // OPENROUTER_API_KEY environment variable, itself populated from a git-ignored
                // .env file locally (see Program.cs) or real env/secret injection in deployment.
                if (string.IsNullOrWhiteSpace(options.ApiKey))
                {
                    options.ApiKey = configuration["OPENROUTER_API_KEY"] ?? string.Empty;
                }
            });
        services.AddOptions<JwtOptions>().Bind(configuration.GetSection(JwtOptions.SectionName));
        services.AddOptions<CrawlerOptions>().Bind(configuration.GetSection(CrawlerOptions.SectionName));

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        // Identity / auth
        services.AddSingleton<IPasswordHasher, PasswordHasherAdapter>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Persistence-backed repositories / queries
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IVehicleQueries, VehicleQueries>();
        services.AddScoped<ICrawlRunRepository, CrawlRunRepository>();
        services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
        services.AddScoped<IChatQueries, ChatQueries>();

        // Crawler
        services.AddScoped<IHtmlFetcher, HttpHtmlFetcher>();
        services.AddScoped<IListingSourceAdapter, OtomotoCarSourceAdapter>();

        // AI subsystem: OpenRouter-backed criteria extraction, deterministic ranking and response
        // composition. IChatCompletionClient falls back to NullChatCompletionClient (no network
        // calls, typed "AI unavailable" result) when no API key is configured.
        services.AddSingleton<IAiSearchOptions, AiSearchOptionsAccessor>();
        services.AddScoped<ICriteriaExtractor, OpenRouterCriteriaExtractor>();
        services.AddSingleton<IVehicleRanker, VehicleRanker>();
        services.AddSingleton<IResponseComposer, MarkdownResponseComposer>();

        services.AddHttpClient<OpenRouterChatCompletionClient>((sp, client) =>
        {
            var aiOptions = sp.GetRequiredService<IOptions<AiOptions>>().Value;
            client.BaseAddress = new Uri("https://openrouter.ai/api/v1/");
            client.Timeout = TimeSpan.FromSeconds(aiOptions.TimeoutSeconds > 0 ? aiOptions.TimeoutSeconds : 30);
            client.DefaultRequestHeaders.Add("HTTP-Referer", "https://autofinderai.local");
            client.DefaultRequestHeaders.Add("X-Title", "AutoFinderAI");
            if (aiOptions.HasApiKey)
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", aiOptions.ApiKey);
            }
        });

        services.AddSingleton<IChatCompletionClient>(sp =>
        {
            var aiOptions = sp.GetRequiredService<IOptions<AiOptions>>().Value;
            return aiOptions.HasApiKey
                ? sp.GetRequiredService<OpenRouterChatCompletionClient>()
                : new NullChatCompletionClient();
        });

        services.AddHttpClient("Crawler", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        }).AddStandardResilienceHandler();

        return services;
    }
}

