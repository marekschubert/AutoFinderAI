using FluentValidation;

namespace AutoFinderAI.Application.Features.Chat.DeleteSession;

public sealed class DeleteSessionCommandValidator : AbstractValidator<DeleteSessionCommand>
{
    public DeleteSessionCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
    }
}
