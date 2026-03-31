using Slums.Core.Economy;
using Slums.Core.Relationships;
using TUnit.Core;

namespace Slums.Core.Tests.Economy;

internal sealed class NpcEconomyTests
{
    [Test]
    public async Task NpcEconomyState_Initialize_CreatesAllNpcs()
    {
        var state = new NpcEconomyState();
        state.Initialize();

        foreach (NpcId npc in Enum.GetValues<NpcId>())
        {
            var economy = state.GetEconomy(npc);
            await Assert.That(economy.Npc).IsEqualTo(npc);
        }
    }

    [Test]
    public async Task NpcEconomyState_Initialize_SetsStartingWealth()
    {
        var state = new NpcEconomyState();
        state.Initialize();

        var hajj = state.GetEconomy(NpcId.LandlordHajjMahmoud);
        await Assert.That(hajj.WealthLevel).IsEqualTo(NpcWealthLevel.Comfortable);
        await Assert.That(hajj.Generosity).IsEqualTo(3);

        var mona = state.GetEconomy(NpcId.NeighborMona);
        await Assert.That(mona.WealthLevel).IsEqualTo(NpcWealthLevel.Poor);
        await Assert.That(mona.Generosity).IsEqualTo(7);
    }

    [Test]
    public async Task NpcEconomyState_SetWealthLevel_UpdatesEconomy()
    {
        var state = new NpcEconomyState();
        state.Initialize();

        state.SetWealthLevel(NpcId.LandlordHajjMahmoud, NpcWealthLevel.Struggling);

        await Assert.That(state.GetEconomy(NpcId.LandlordHajjMahmoud).WealthLevel).IsEqualTo(NpcWealthLevel.Struggling);
    }

    [Test]
    public async Task NpcEconomyState_GetStrugglingNpcs_ReturnsCorrectNpcs()
    {
        var state = new NpcEconomyState();
        state.Initialize();

        state.SetWealthLevel(NpcId.LandlordHajjMahmoud, NpcWealthLevel.Struggling);
        state.SetWealthLevel(NpcId.NeighborMona, NpcWealthLevel.Struggling);

        var struggling = state.GetStrugglingNpcs();
        await Assert.That(struggling).Contains(NpcId.LandlordHajjMahmoud);
        await Assert.That(struggling).Contains(NpcId.NeighborMona);
    }

    [Test]
    public async Task NpcEconomyState_GetComfortableNpcs_ReturnsCorrectNpcs()
    {
        var state = new NpcEconomyState();
        state.Initialize();

        var comfortable = state.GetComfortableNpcs();
        await Assert.That(comfortable).Contains(NpcId.LandlordHajjMahmoud);
        await Assert.That(comfortable).Contains(NpcId.CafeOwnerNadia);
    }

    [Test]
    public async Task NpcEconomyState_AddDebt_TracksNpcToNpcDebt()
    {
        var state = new NpcEconomyState();
        state.Initialize();

        var from = DebtorId.FromNpc(NpcId.NeighborMona);
        var to = DebtorId.FromNpc(NpcId.LandlordHajjMahmoud);
        state.AddDebt(from, to, 30);

        var monaEcon = state.GetEconomy(NpcId.NeighborMona);
        var hajjEcon = state.GetEconomy(NpcId.LandlordHajjMahmoud);

        await Assert.That(monaEcon.MoneyOwedTo[to]).IsEqualTo(30);
        await Assert.That(hajjEcon.MoneyOwedBy[from]).IsEqualTo(30);
    }

    [Test]
    public async Task NpcEconomyState_ResolveDebt_ClearsDebt()
    {
        var state = new NpcEconomyState();
        state.Initialize();

        var from = DebtorId.FromNpc(NpcId.NeighborMona);
        var to = DebtorId.FromNpc(NpcId.LandlordHajjMahmoud);
        state.AddDebt(from, to, 30);
        state.ResolveDebt(from, to);

        var monaEcon = state.GetEconomy(NpcId.NeighborMona);
        await Assert.That(monaEcon.MoneyOwedTo.ContainsKey(to)).IsFalse();
    }

    [Test]
    public async Task NpcEconomyState_RestoreEntry_RestoresFullState()
    {
        var state = new NpcEconomyState();
        state.Initialize();

        var owedTo = new Dictionary<DebtorId, int> { { DebtorId.FromNpc(NpcId.NurseSalma), 20 } };
        var owedBy = new Dictionary<DebtorId, int> { { DebtorId.Player, 50 } };

        state.RestoreEntry(NpcId.LandlordHajjMahmoud, NpcWealthLevel.Poor, 5, owedTo, owedBy, 10, 20, 30);

        var econ = state.GetEconomy(NpcId.LandlordHajjMahmoud);
        await Assert.That(econ.WealthLevel).IsEqualTo(NpcWealthLevel.Poor);
        await Assert.That(econ.Generosity).IsEqualTo(5);
        await Assert.That(econ.LastHardshipDay).IsEqualTo(10);
        await Assert.That(econ.LastWindfallDay).IsEqualTo(20);
        await Assert.That(econ.GenerousUntilDay).IsEqualTo(30);
    }

    [Test]
    public async Task NpcEconomy_WithHardship_DecreasesWealthLevel()
    {
        var economy = new NpcEconomy
        {
            Npc = NpcId.NeighborMona,
            WealthLevel = NpcWealthLevel.Stable,
            Generosity = 5
        };

        var result = economy.WithHardship(10);
        await Assert.That(result.WealthLevel).IsEqualTo(NpcWealthLevel.Poor);
        await Assert.That(result.LastHardshipDay).IsEqualTo(10);
    }

    [Test]
    public async Task NpcEconomy_WithHardship_ClampsToStruggling()
    {
        var economy = new NpcEconomy
        {
            Npc = NpcId.NeighborMona,
            WealthLevel = NpcWealthLevel.Struggling,
            Generosity = 5
        };

        var result = economy.WithHardship(5);
        await Assert.That(result.WealthLevel).IsEqualTo(NpcWealthLevel.Struggling);
    }

    [Test]
    public async Task NpcEconomy_WithWindfall_IncreasesWealthLevel()
    {
        var economy = new NpcEconomy
        {
            Npc = NpcId.NeighborMona,
            WealthLevel = NpcWealthLevel.Poor,
            Generosity = 5
        };

        var result = economy.WithWindfall(10, 15);
        await Assert.That(result.WealthLevel).IsEqualTo(NpcWealthLevel.Stable);
        await Assert.That(result.LastWindfallDay).IsEqualTo(10);
        await Assert.That(result.GenerousUntilDay).IsEqualTo(15);
    }

    [Test]
    public async Task NpcEconomy_WithWindfall_ClampsToComfortable()
    {
        var economy = new NpcEconomy
        {
            Npc = NpcId.LandlordHajjMahmoud,
            WealthLevel = NpcWealthLevel.Comfortable,
            Generosity = 3
        };

        var result = economy.WithWindfall(10, 15);
        await Assert.That(result.WealthLevel).IsEqualTo(NpcWealthLevel.Comfortable);
    }

    [Test]
    public async Task DebtorId_FromNpc_CreatesNpcDebtor()
    {
        var debtor = DebtorId.FromNpc(NpcId.NeighborMona);
        await Assert.That(debtor.IsNpc).IsTrue();
        await Assert.That(debtor.IsPlayer).IsFalse();
        await Assert.That(debtor.TryGetNpcId()).IsEqualTo(NpcId.NeighborMona);
    }

    [Test]
    public async Task DebtorId_Player_CreatesPlayerDebtor()
    {
        var debtor = DebtorId.Player;
        await Assert.That(debtor.IsPlayer).IsTrue();
        await Assert.That(debtor.IsNpc).IsFalse();
        await Assert.That(debtor.TryGetNpcId()).IsNull();
    }

    [Test]
    public async Task NpcEconomyDefinitions_All_HasTwelveEntries()
    {
        await Assert.That(NpcEconomyDefinitions.All).Count().IsEqualTo(12);
    }

    [Test]
    public async Task NpcEconomyState_GetEconomy_ReturnsDefaultForUnknownNpc()
    {
        var state = new NpcEconomyState();
        var economy = state.GetEconomy(NpcId.RunnerYoussef);

        await Assert.That(economy.WealthLevel).IsEqualTo(NpcWealthLevel.Stable);
        await Assert.That(economy.Generosity).IsEqualTo(5);
    }
}
