namespace AutoFinderAI.Infrastructure.Crawling;

/// <summary>Serves HTML from local fixture files instead of the network. Used by tests so no live
/// HTTP call is ever made against otomoto.pl.</summary>
public sealed class FileHtmlFetcher : IHtmlFetcher
{
    private readonly IReadOnlyDictionary<string, string> _htmlByUrl;

    public FileHtmlFetcher(IReadOnlyDictionary<string, string> htmlByUrl)
    {
        _htmlByUrl = htmlByUrl;
    }

    public Task<string?> FetchAsync(string url, CancellationToken cancellationToken)
        => Task.FromResult(_htmlByUrl.TryGetValue(url, out var html) ? html : null);
}
