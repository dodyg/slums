using Slums.Core.Phone;
using TUnit.Core;

namespace Slums.Core.Tests.Phone;

internal sealed class PhoneStateTests
{
    [Test]
    public async Task PhoneState_Initial_IsOperational()
    {
        var phone = new PhoneState();
        await Assert.That(phone.IsOperational()).IsTrue();
    }

    [Test]
    public async Task PhoneState_Initial_HasSevenDaysCredit()
    {
        var phone = new PhoneState();
        await Assert.That(phone.CreditRemaining).IsEqualTo(7);
    }

    [Test]
    public async Task PhoneState_DailyCreditDrain_TracksCorrectly()
    {
        var phone = new PhoneState();

        for (var i = 0; i < 6; i++)
        {
            phone.DailyCreditDrain();
        }

        await Assert.That(phone.CreditRemaining).IsEqualTo(1);
        await Assert.That(phone.IsOperational()).IsTrue();
    }

    [Test]
    public async Task PhoneState_DailyCreditDrain_ExpiresAfterSevenDays()
    {
        var phone = new PhoneState();

        for (var i = 0; i < 7; i++)
        {
            phone.DailyCreditDrain();
        }

        await Assert.That(phone.CreditRemaining).IsEqualTo(0);
        await Assert.That(phone.IsOperational()).IsFalse();
    }

    [Test]
    public async Task PhoneState_RefillCredit_ResetsCredit()
    {
        var phone = new PhoneState();

        for (var i = 0; i < 7; i++)
        {
            phone.DailyCreditDrain();
        }

        await Assert.That(phone.IsOperational()).IsFalse();

        phone.RefillCredit();

        await Assert.That(phone.CreditRemaining).IsEqualTo(7);
        await Assert.That(phone.DaysSinceCreditRefill).IsEqualTo(0);
        await Assert.That(phone.IsOperational()).IsTrue();
    }

    [Test]
    public async Task PhoneState_LosePhone_DisablesPhone()
    {
        var phone = new PhoneState();
        phone.LosePhone(5);

        await Assert.That(phone.PhoneLost).IsTrue();
        await Assert.That(phone.PhoneLostDay).IsEqualTo(5);
        await Assert.That(phone.IsOperational()).IsFalse();
    }

    [Test]
    public async Task PhoneState_RecoverPhone_RestoresFunctionality()
    {
        var phone = new PhoneState();
        phone.LosePhone(5);
        phone.RecoverPhone();

        await Assert.That(phone.PhoneLost).IsFalse();
        await Assert.That(phone.PhoneLostDay).IsNull();
        await Assert.That(phone.PhoneRecovered).IsTrue();
        await Assert.That(phone.IsOperational()).IsTrue();
    }

    [Test]
    public async Task PhoneState_ReplacePhone_RestoresFunctionality()
    {
        var phone = new PhoneState();
        phone.LosePhone(5);
        phone.ReplacePhone();

        await Assert.That(phone.PhoneLost).IsFalse();
        await Assert.That(phone.PhoneRecovered).IsFalse();
        await Assert.That(phone.CreditRemaining).IsEqualTo(7);
        await Assert.That(phone.IsOperational()).IsTrue();
    }

    [Test]
    public async Task PhoneState_RefillCredit_FailsWhenPhoneLost()
    {
        var phone = new PhoneState();
        phone.LosePhone(3);

        var result = phone.RefillCredit();
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task PhoneState_DailyCreditDrain_DoesNothingWhenLost()
    {
        var phone = new PhoneState();
        phone.LosePhone(3);
        phone.DailyCreditDrain();

        await Assert.That(phone.CreditRemaining).IsEqualTo(7);
    }

    [Test]
    public async Task PhoneState_Restore_RestoresAllFields()
    {
        var phone = new PhoneState();
        phone.Restore(false, 2, 3, true, 10, true);

        await Assert.That(phone.HasPhone).IsFalse();
        await Assert.That(phone.CreditRemaining).IsEqualTo(2);
        await Assert.That(phone.DaysSinceCreditRefill).IsEqualTo(3);
        await Assert.That(phone.PhoneLost).IsTrue();
        await Assert.That(phone.PhoneLostDay).IsEqualTo(10);
        await Assert.That(phone.PhoneRecovered).IsTrue();
    }
}
