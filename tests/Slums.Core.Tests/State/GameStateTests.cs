        await Assert.That(state.PolicePressure).IsEqualTo(23);
    }
    
    [Test]
    public async Task EndDay_MotherInCrisisWithoutCheck_ShouldIncreasePlayerStress()
    {
        using var state = new GameSession();
        state.Player.Household.SetMotherCondition(MotherCondition.Crisis);
        state.Player.Household.SetMedicineStock(1);
        state.EndDay();

        await Assert.That(state.Player.Stats.Stress).IsEqualTo(28);
    }
    
    [Test]
    public async Task EndDay_MotherFragileWithCheckOnMother_ShouldIncreasePlayerStress()
    {
        using var state = new GameSession();
        state.Player.Household.SetMotherHealth(25);
        state.Player.Household.GiveMedicine();
        state.EndDay();

        await Assert.That(state.Player.Stats.Stress).IsEqualTo(28);
    }
    
    [Test]
    public async Task EndDay_MotherInCrisisWithoutCheck_ShouldLoseHealth()
    {
        using var state = new GameSession();
        state.EndDay();

        await Assert.That(state.Player.Stats.Energy).IsEqualTo(75);
    }
    
    [Test]
    public async Task EndDay_MotherCrisis_WithoutCheck_ShouldIncreasePlayerStress()
    {
        using var state = new GameSession();
        state.EndDay();

        await Assert.That(state.Player.Stats.Health).IsEqualTo(65);
    }
}