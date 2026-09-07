namespace Slums.Core.Economy;

/// <summary>Result of processing a daily loan-shark collection attempt.</summary>
public sealed record DebtEscalationResult(string Message, bool TriggersDestitution)
{
    public static DebtEscalationResult None { get; } = new(string.Empty, false);
}
