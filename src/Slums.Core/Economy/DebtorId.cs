using Slums.Core.Relationships;

namespace Slums.Core.Economy;

public abstract record DebtorId
{
    internal sealed record NpcDebtor(NpcId Npc) : DebtorId;
    internal sealed record PlayerDebtor : DebtorId;
    public static DebtorId Player => new PlayerDebtor();

    public bool IsNpc => this is NpcDebtor;
    public bool IsPlayer => this is PlayerDebtor;

    public NpcId? TryGetNpcId() => this is NpcDebtor npc ? npc.Npc : null;

    public static DebtorId FromNpc(NpcId npc) => new NpcDebtor(npc);
}
