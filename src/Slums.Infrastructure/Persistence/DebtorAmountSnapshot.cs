namespace Slums.Infrastructure.Persistence;

public sealed class DebtorAmountSnapshot
{
    public string DebtorType { get; init; } = "Player";
    public string? NpcId { get; init; }
    public int Amount { get; init; }
}
