namespace Slums.Core.Relationships;

public static class RelationshipService
{
    public static string? ModifyTrust(RelationshipState state, NpcId npcId, int delta, int currentDay)
    {
        ArgumentNullException.ThrowIfNull(state);

        var existing = state.GetNpcRelationship(npcId);
        var previousTrust = existing.Trust;
        var updatedTrust = Math.Clamp(previousTrust + delta, -100, 100);
        state.SetNpcRelationship(npcId, updatedTrust, currentDay);

        if (previousTrust > -50 && updatedTrust <= -50)
        {
            return npcId switch
            {
                NpcId.LandlordHajjMahmoud => "Hajj Mahmoud no longer trusts you. He may raise the rent.",
                NpcId.FixerUmmKarim => "Umm Karim turns cold. Street work will be harder to find.",
                NpcId.OfficerKhalid => "Officer Khalid marks your face. Checkpoints will feel tighter now.",
                NpcId.NeighborMona => "Mona stops covering for you in the building.",
                NpcId.NurseSalma => "Nurse Salma cuts the conversation short. The clinic feels less welcoming.",
                NpcId.WorkshopBossAbuSamir => "Abu Samir decides you are more trouble than labor.",
                NpcId.CafeOwnerNadia => "Nadia cools toward you. Even tea comes with less warmth.",
                NpcId.FenceHanan => "Hanan decides you bring more heat than profit.",
                NpcId.RunnerYoussef => "Youssef stops passing you the useful rumors.",
                _ => null
            };
        }

        if (previousTrust < 50 && updatedTrust >= 50)
        {
            return npcId switch
            {
                NpcId.LandlordHajjMahmoud => "Hajj Mahmoud softens toward you. He gives you a little breathing room.",
                NpcId.FixerUmmKarim => "Umm Karim begins to trust you with more dangerous conversations.",
                NpcId.OfficerKhalid => "Officer Khalid starts to see you as more than another name in a notebook.",
                NpcId.NeighborMona => "Mona starts looking out for you like family.",
                NpcId.NurseSalma => "Nurse Salma starts slipping you practical advice between patients.",
                NpcId.WorkshopBossAbuSamir => "Abu Samir starts keeping your spot open when shifts get thin.",
                NpcId.CafeOwnerNadia => "Nadia begins trusting you with the front tables and the better customers.",
                NpcId.FenceHanan => "Hanan starts treating you like someone who understands risk.",
                NpcId.RunnerYoussef => "Youssef starts waving you into conversations he used to keep closed.",
                _ => null
            };
        }

        return null;
    }

    public static string? ModifyReputation(RelationshipState state, FactionId factionId, int delta)
    {
        ArgumentNullException.ThrowIfNull(state);

        var existing = state.GetFactionStanding(factionId);
        var previousReputation = existing.Reputation;
        var updatedReputation = Math.Clamp(previousReputation + delta, -100, 100);
        state.SetFactionStanding(factionId, updatedReputation);

        if (previousReputation < 50 && updatedReputation >= 50)
        {
            return factionId switch
            {
                FactionId.ImbabaCrew => "The Imbaba crew starts using your name with respect.",
                FactionId.DokkiThugs => "The Dokki thugs stop treating you like an outsider.",
                FactionId.ExPrisonerNetwork => "The ex-prisoner network closes ranks around you.",
                _ => null
            };
        }

        if (previousReputation > -50 && updatedReputation <= -50)
        {
            return factionId switch
            {
                FactionId.ImbabaCrew => "Word spreads through Imbaba that you cannot be trusted.",
                FactionId.DokkiThugs => "The Dokki boys start watching you with open suspicion.",
                FactionId.ExPrisonerNetwork => "The ex-prisoner network keeps its distance from you.",
                _ => null
            };
        }

        return null;
    }
}