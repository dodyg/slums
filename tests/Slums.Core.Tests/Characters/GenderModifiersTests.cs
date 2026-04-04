using Slums.Core.Characters;
using Slums.Core.Crimes;
using Slums.Core.Jobs;
using Slums.Core.Relationships;
using TUnit.Core;

namespace Slums.Core.Tests.Characters;

internal sealed class GenderModifiersTests
{
    [Test]
    public async Task JobPayModifier_FemaleMicrobusDispatch_ShouldReturnMinus4()
    {
        await Assert.That(GenderModifiers.JobPayModifier(Gender.Female, JobType.MicrobusDispatch)).IsEqualTo(-4);
    }

    [Test]
    public async Task JobPayModifier_FemaleMarketPorter_ShouldReturnMinus3()
    {
        await Assert.That(GenderModifiers.JobPayModifier(Gender.Female, JobType.MarketPorter)).IsEqualTo(-3);
    }

    [Test]
    public async Task JobPayModifier_FemaleOtherJobs_ShouldReturnZero()
    {
        await Assert.That(GenderModifiers.JobPayModifier(Gender.Female, JobType.HouseCleaning)).IsEqualTo(0);
        await Assert.That(GenderModifiers.JobPayModifier(Gender.Female, JobType.WorkshopSewing)).IsEqualTo(0);
    }

    [Test]
    public async Task JobPayModifier_MaleHouseCleaning_ShouldReturnMinus2()
    {
        await Assert.That(GenderModifiers.JobPayModifier(Gender.Male, JobType.HouseCleaning)).IsEqualTo(-2);
    }

    [Test]
    public async Task JobPayModifier_MaleWorkshopSewing_ShouldReturnMinus2()
    {
        await Assert.That(GenderModifiers.JobPayModifier(Gender.Male, JobType.WorkshopSewing)).IsEqualTo(-2);
    }

    [Test]
    public async Task JobPayModifier_MaleOtherJobs_ShouldReturnZero()
    {
        await Assert.That(GenderModifiers.JobPayModifier(Gender.Male, JobType.MicrobusDispatch)).IsEqualTo(0);
    }

    [Test]
    public async Task JobStressModifier_FemaleMicrobusDispatch_ShouldReturn3()
    {
        await Assert.That(GenderModifiers.JobStressModifier(Gender.Female, JobType.MicrobusDispatch)).IsEqualTo(3);
    }

    [Test]
    public async Task JobStressModifier_MaleHouseCleaning_ShouldReturn2()
    {
        await Assert.That(GenderModifiers.JobStressModifier(Gender.Male, JobType.HouseCleaning)).IsEqualTo(2);
    }

    [Test]
    public async Task CrimeDetectionModifier_FemalePettyTheft_ShouldReturnMinus3()
    {
        await Assert.That(GenderModifiers.CrimeDetectionModifier(Gender.Female, CrimeType.PettyTheft)).IsEqualTo(-3);
    }

    [Test]
    public async Task CrimeDetectionModifier_FemaleBulaqProtectionRacket_ShouldReturn8()
    {
        await Assert.That(GenderModifiers.CrimeDetectionModifier(Gender.Female, CrimeType.BulaqProtectionRacket)).IsEqualTo(8);
    }

    [Test]
    public async Task CrimeDetectionModifier_MaleRobbery_ShouldReturn3()
    {
        await Assert.That(GenderModifiers.CrimeDetectionModifier(Gender.Male, CrimeType.Robbery)).IsEqualTo(3);
    }

    [Test]
    public async Task CrimeDetectionModifier_MaleNetworkErrand_ShouldReturnMinus3()
    {
        await Assert.That(GenderModifiers.CrimeDetectionModifier(Gender.Male, CrimeType.NetworkErrand)).IsEqualTo(-3);
    }

    [Test]
    public async Task NpcStartingTrustModifier_FemaleNeighborMona_ShouldReturn5()
    {
        await Assert.That(GenderModifiers.NpcStartingTrustModifier(Gender.Female, NpcId.NeighborMona)).IsEqualTo(5);
    }

    [Test]
    public async Task NpcStartingTrustModifier_MaleNeighborMona_ShouldReturnMinus5()
    {
        await Assert.That(GenderModifiers.NpcStartingTrustModifier(Gender.Male, NpcId.NeighborMona)).IsEqualTo(-5);
    }

    [Test]
    public async Task NpcStartingTrustModifier_MaleWorkshopBossAbuSamir_ShouldReturn5()
    {
        await Assert.That(GenderModifiers.NpcStartingTrustModifier(Gender.Male, NpcId.WorkshopBossAbuSamir)).IsEqualTo(5);
    }

    [Test]
    public async Task NpcStartingTrustModifier_FemaleWorkshopBossAbuSamir_ShouldReturnMinus3()
    {
        await Assert.That(GenderModifiers.NpcStartingTrustModifier(Gender.Female, NpcId.WorkshopBossAbuSamir)).IsEqualTo(-3);
    }

    [Test]
    public async Task DailyStressModifier_Female_ShouldReturn1()
    {
        await Assert.That(GenderModifiers.DailyStressModifier(Gender.Female)).IsEqualTo(1);
    }

    [Test]
    public async Task DailyStressModifier_Male_ShouldReturn0()
    {
        await Assert.That(GenderModifiers.DailyStressModifier(Gender.Male)).IsEqualTo(0);
    }

    [Test]
    public async Task PhysicalJobEnergyDrain_Male_ShouldReturn2()
    {
        await Assert.That(GenderModifiers.PhysicalJobEnergyDrain(Gender.Male)).IsEqualTo(2);
    }

    [Test]
    public async Task PhysicalJobEnergyDrain_Female_ShouldReturn0()
    {
        await Assert.That(GenderModifiers.PhysicalJobEnergyDrain(Gender.Female)).IsEqualTo(0);
    }

    [Test]
    public async Task DefaultName_Female_ShouldReturnAmira()
    {
        await Assert.That(GenderModifiers.DefaultName(Gender.Female)).IsEqualTo("Amira");
    }

    [Test]
    public async Task DefaultName_Male_ShouldReturnKarim()
    {
        await Assert.That(GenderModifiers.DefaultName(Gender.Male)).IsEqualTo("Karim");
    }
}
