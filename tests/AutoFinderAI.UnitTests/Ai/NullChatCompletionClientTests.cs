using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Infrastructure.Ai;
using FluentAssertions;

namespace AutoFinderAI.UnitTests.Ai;

public class NullChatCompletionClientTests
{
    [Fact]
    public void IsAvailable_IsFalse()
    {
        new NullChatCompletionClient().IsAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task CompleteAsync_ReturnsTypedFailure_WithoutThrowing()
    {
        var client = new NullChatCompletionClient();

        var result = await client.CompleteAsync(new ChatCompletionRequest("system", "user"), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Response.Should().BeNull();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }
}
