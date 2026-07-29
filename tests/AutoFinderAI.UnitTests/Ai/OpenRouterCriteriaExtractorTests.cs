using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Application.Ai.CriteriaExtraction;
using NSubstitute;
using FluentAssertions;

namespace AutoFinderAI.UnitTests.Ai;

public class OpenRouterCriteriaExtractorTests
{
    private sealed class FakeAiSearchOptions : IAiSearchOptions
    {
        public int MaxCandidates => 200;
        public int DefaultLimit => 10;
        public int MaxLimit => 50;
        public int MaxRepairRetries => 1;
    }

    private const string ValidJson =
        """
        {"make":"BMW","model":null,"yearFrom":2015,"yearTo":null,"priceFrom":null,"priceTo":50000,
        "mileageMax":null,"fuelType":"Diesel","transmission":null,"bodyType":null,
        "enginePowerHpFrom":null,"enginePowerHpTo":null,"seatsMin":null,"excludeDamaged":true,
        "locationContains":null,"keywords":[],"softPreferences":["reliable"],"sortBy":"PriceAsc",
        "limit":5,"clarificationQuestion":null,"intro":"Looking for a reliable BMW."}
        """;

    [Fact]
    public async Task ExtractAsync_ValidJson_ReturnsSanitizedCriteria()
    {
        var chatClient = Substitute.For<IChatCompletionClient>();
        chatClient.IsAvailable.Returns(true);
        chatClient.CompleteAsync(Arg.Any<ChatCompletionRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ChatCompletionResult(true, new ChatCompletionResponse(ValidJson, "test-model", 10, 20, 100), null));

        var extractor = new OpenRouterCriteriaExtractor(chatClient, new FakeAiSearchOptions());

        var result = await extractor.ExtractAsync("I want a reliable BMW diesel under 50000", Array.Empty<ChatTurn>(), CancellationToken.None);

        result.Criteria.Should().NotBeNull();
        result.Criteria!.Make.Should().Be("BMW");
        result.Criteria.MaxPrice.Should().Be(50000);
        result.Criteria.SortBy.Should().Be(VehicleSortBy.PriceAsc);
        result.Criteria.Limit.Should().Be(5);
        result.ModelUsed.Should().Be("test-model");
        result.Intro.Should().Be("Looking for a reliable BMW.");
    }

    [Fact]
    public async Task ExtractAsync_MalformedJsonThenValid_RetriesOnceAndSucceeds()
    {
        var chatClient = Substitute.For<IChatCompletionClient>();
        chatClient.IsAvailable.Returns(true);
        chatClient.CompleteAsync(Arg.Any<ChatCompletionRequest>(), Arg.Any<CancellationToken>())
            .Returns(
                new ChatCompletionResult(true, new ChatCompletionResponse("not json", "test-model", null, null, 10), null),
                new ChatCompletionResult(true, new ChatCompletionResponse("""{"make":"Audi"}""", "test-model", null, null, 10), null));

        var extractor = new OpenRouterCriteriaExtractor(chatClient, new FakeAiSearchOptions());

        var result = await extractor.ExtractAsync("Audi please", Array.Empty<ChatTurn>(), CancellationToken.None);

        result.Criteria.Should().NotBeNull();
        result.Criteria!.Make.Should().Be("Audi");
        await chatClient.Received(2).CompleteAsync(Arg.Any<ChatCompletionRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExtractAsync_PersistentlyMalformedJson_ReturnsClarificationFailure()
    {
        var chatClient = Substitute.For<IChatCompletionClient>();
        chatClient.IsAvailable.Returns(true);
        chatClient.CompleteAsync(Arg.Any<ChatCompletionRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ChatCompletionResult(true, new ChatCompletionResponse("not json at all", "test-model", null, null, 10), null));

        var extractor = new OpenRouterCriteriaExtractor(chatClient, new FakeAiSearchOptions());

        var result = await extractor.ExtractAsync("gibberish", Array.Empty<ChatTurn>(), CancellationToken.None);

        result.Criteria.Should().BeNull();
        result.ClarificationQuestion.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ExtractAsync_UnavailableClient_ReturnsDegradedClarificationWithoutCallingProvider()
    {
        var chatClient = Substitute.For<IChatCompletionClient>();
        chatClient.IsAvailable.Returns(false);

        var extractor = new OpenRouterCriteriaExtractor(chatClient, new FakeAiSearchOptions());

        var result = await extractor.ExtractAsync("anything", Array.Empty<ChatTurn>(), CancellationToken.None);

        result.Criteria.Should().BeNull();
        result.ClarificationQuestion.Should().Contain("unavailable");
        await chatClient.DidNotReceive().CompleteAsync(Arg.Any<ChatCompletionRequest>(), Arg.Any<CancellationToken>());
    }
}
