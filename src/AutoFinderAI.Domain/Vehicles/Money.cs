namespace AutoFinderAI.Domain.Vehicles;

public sealed class Money
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "PLN";

    private Money() { }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Price amount cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required.", nameof(currency));
        }

        return new Money(amount, currency.Trim().ToUpperInvariant());
    }
}
