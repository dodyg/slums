using System.Reflection;
using FluentAssertions;
using Slums.Application.Narrative;
using Slums.Core.Economy;
using Slums.Core.Relationships;
using TUnit;

namespace Slums.Narrative.Ink.Tests;

internal sealed class InkNarrativeTagParserTests
{
    [Test]
    [Arguments("FAVOR:999")]
    [Arguments("NPC_TRUST:999,-5")]
    [Arguments("FACTION_REP:999,3")]
    [Arguments("DEBT:999,true")]
    [Arguments("EMBARRASSED:999,true")]
    [Arguments("HELPED:999,false")]
    public void UndefinedNpcOrFactionPayload_ShouldBeRejected(string tag)
    {
        var act = () => InvokeParser(tag);

        act.Should().Throw<InvalidOperationException>().Which.Message.Should().Contain(tag);
    }

    [Test]
    [Arguments("DEBT_PAYMENT:999,10")]
    [Arguments("DEBT_DUE_EXTENSION:999,3")]
    public void UndefinedDebtSourcePayload_ShouldBeRejected(string tag)
    {
        var act = () => InvokeParser(tag);

        act.Should().Throw<InvalidOperationException>().Which.Message.Should().Contain(tag);
    }

    [Test]
    public void DefinedEnumPayloads_ShouldStillParse()
    {
        InvokeParser("NPC_TRUST:OfficerKhalid,2").Should().BeOfType<NpcTrustEffect>();
        InvokeParser("FAVOR:OfficerKhalid").Should().Be(NpcId.OfficerKhalid);
        InvokeParser("FACTION_REP:ImbabaCrew,3").Should().BeOfType<FactionReputationEffect>();
        InvokeParser("DEBT:FixerUmmKarim,true").Should().BeOfType<DebtEffect>();
        InvokeParser("EMBARRASSED:NeighborMona,true").Should().BeOfType<EmbarrassedEffect>();
        InvokeParser("HELPED:NeighborMona,false").Should().BeOfType<HelpedEffect>();
        InvokeParser("DEBT_PAYMENT:LoanShark,10").Should().BeOfType<DebtPaymentEffect>();
        InvokeParser("DEBT_DUE_EXTENSION:LandlordAdvance,3").Should().BeOfType<DebtDueExtensionEffect>();
    }

    private static object InvokeParser(string tag)
    {
        var serviceType = typeof(InkNarrativeService);
        var separator = tag.IndexOf(':', StringComparison.Ordinal);
        var key = tag[..separator];
        var value = tag[(separator + 1)..];

        return key switch
        {
            "NPC_TRUST" => Invoke(serviceType, "ParseNpcTrustEffect", tag, value),
            "FACTION_REP" => Invoke(serviceType, "ParseFactionReputationEffect", tag, value),
            "FAVOR" => Invoke(serviceType, "ParseNpcTarget", tag, value),
            "DEBT" => Invoke(serviceType, "ParseDebtEffect", tag, value),
            "EMBARRASSED" => Invoke(serviceType, "ParseBoolStateEffect", tag, value, new Func<NpcId, bool, NarrativeEffect>((npc, state) => new EmbarrassedEffect(npc, state))),
            "HELPED" => Invoke(serviceType, "ParseBoolStateEffect", tag, value, new Func<NpcId, bool, NarrativeEffect>((npc, state) => new HelpedEffect(npc, state))),
            "DEBT_PAYMENT" => InvokeDebtAmountParser("DebtPaymentEffect", tag, value),
            "DEBT_DUE_EXTENSION" => InvokeDebtAmountParser("DebtDueExtensionEffect", tag, value),
            _ => throw new ArgumentException($"Unsupported test tag '{tag}'.", nameof(tag))
        };
    }

    private static object InvokeDebtAmountParser(string effectTypeName, string tag, string value)
    {
        var effectType = typeof(DebtPaymentEffect).Assembly
            .GetType($"Slums.Application.Narrative.{effectTypeName}")
            ?? throw new InvalidOperationException($"Could not find effect type '{effectTypeName}'.");
        var parser = typeof(InkNarrativeService).GetMethod("ParseDebtAmountEffect", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("Could not find the debt amount parser.");
        var closedParser = parser.MakeGenericMethod(effectType);
        object factory = effectTypeName switch
        {
            "DebtPaymentEffect" => new Func<DebtSource, int, DebtPaymentEffect>((source, amount) => new DebtPaymentEffect(source, amount)),
            "DebtDueExtensionEffect" => new Func<DebtSource, int, DebtDueExtensionEffect>((source, amount) => new DebtDueExtensionEffect(source, amount)),
            _ => throw new ArgumentException($"Unsupported effect type '{effectTypeName}'.", nameof(effectTypeName))
        };

        return InvokeMethod(closedParser, tag, value, factory);
    }

    private static object Invoke(Type serviceType, string methodName, params object[] arguments)
    {
        var method = serviceType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException($"Could not find parser '{methodName}'.");

        return InvokeMethod(method, arguments);
    }

    private static object InvokeMethod(MethodInfo method, params object[] arguments)
    {
        try
        {
            return method.Invoke(null, arguments)!;
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            throw exception.InnerException;
        }
    }
}
