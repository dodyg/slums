using Slums.Application.Narrative;
using Slums.Core.Relationships;
using Slums.Core.State;

namespace Slums.Narrative.Ink;

internal static class FallbackNarrativeCatalog
{
    public static bool TryCreateSession(string knotName, GameState gameState, out FallbackSceneSession? session)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        var definition = knotName switch
        {
            "crime_first_success" => CreateSingleNodeScene(
                static state => "The cash in your pocket feels warmer than it should. Cairo rewards nerve, but it also remembers faces.",
                new NarrativeOutcome { StressChange = 5, Message = "The first successful crime leaves your nerves rattling." }),
            "crime_warning" => CreateSingleNodeScene(
                static state => "By maghrib, the talk in the street is already changing. Too many glances linger on you. Too many questions are being asked.",
                new NarrativeOutcome { StressChange = 8, Message = "The street is starting to feel unsafe." }),
            "event_mother_health_scare" => CreateSingleNodeScene(
                static state => "Your mother tries to smile through the dizziness, but the way she grips the bedframe tells the truth.",
                null),
            "event_neighborhood_rumor" => CreateSingleNodeScene(
                static state => "A neighbour lowers her voice and says the police were seen asking for names two alleys over.",
                null),
            "landlord_rent_negotiation" => CreateLandlordScene(
                "Hajj Mahmoud waits at the stairwell, prayer beads in hand. He asks for the rent without raising his voice, which somehow makes it worse.",
                8,
                -12),
            "landlord_rent_negotiation_warm" => CreateLandlordScene(
                "Hajj Mahmoud looks tired more than angry. He reminds you that everyone in the building is struggling, then asks what you can manage this week.",
                12,
                -10),
            "landlord_rent_negotiation_hostile" => CreateLandlordScene(
                "Hajj Mahmoud does not invite excuses. His jaw tightens before you say a word and the whole stairwell feels narrow.",
                5,
                -18),
            "fixer_first_contact" => CreateFixerFirstContactScene(),
            "fixer_repeat_contact" => CreateFixerRepeatScene(),
            "officer_checkpoint" => CreateOfficerScene(
                "Officer Khalid stops you near the square. His tone is almost casual, but his eyes keep inventory.",
                6,
                -6),
            "officer_checkpoint_hot" => CreateOfficerScene(
                "Officer Khalid is no longer making conversation. He asks where you were last night and does not pretend the question is routine.",
                3,
                -12),
            "ending_stability" => CreateSingleNodeScene(
                static state => "It is not victory. It is rent paid, bread on the table, and mornings that do not begin with panic. In Cairo, that counts for something close to grace.",
                null),
            "ending_luxor" => CreateSingleNodeScene(
                static state => "The train south feels unreal at first. Luxor is not a miracle, only a place where the air loosens around your chest and the future stops looking like a trap.",
                null),
            "ending_arrested" => CreateSingleNodeScene(
                static state => "The holding cell smells of heat and metal. Somewhere above you, Cairo keeps bargaining, praying, hustling, and surviving without pause.",
                null),
            _ => null
        };

        if (definition is null)
        {
            session = null;
            return false;
        }

        session = new FallbackSceneSession(definition, gameState);
        return true;
    }

    private static FallbackSceneDefinition CreateSingleNodeScene(Func<GameState, string> textFactory, NarrativeOutcome? outcome)
    {
        var nodes = new Dictionary<string, FallbackNode>
        {
            ["start"] = new FallbackNode("start", textFactory, [])
        };

        if (outcome is not null)
        {
            nodes["start"] = new FallbackNode("start", textFactory, [new FallbackChoice("Continue", "end", outcome)]);
            nodes["end"] = new FallbackNode("end", static _ => string.Empty, []);
        }

        return new FallbackSceneDefinition("start", nodes);
    }

    private static FallbackSceneDefinition CreateLandlordScene(string openingText, int politeTrustGain, int defiantTrustChange)
    {
        return new FallbackSceneDefinition(
            "start",
            new Dictionary<string, FallbackNode>
            {
                ["start"] = new(
                    "start",
                    _ => openingText,
                    [
                        new FallbackChoice(
                            "Answer politely and ask for time",
                            "polite",
                            new NarrativeOutcome
                            {
                                StressChange = 5,
                                NpcTrustTarget = NpcId.LandlordHajjMahmoud,
                                NpcTrustChange = politeTrustGain,
                                Message = "Hajj Mahmoud gives you a little room to breathe."
                            }),
                        new FallbackChoice(
                            "Answer defiantly",
                            "defiant",
                            new NarrativeOutcome
                            {
                                StressChange = 10,
                                NpcTrustTarget = NpcId.LandlordHajjMahmoud,
                                NpcTrustChange = defiantTrustChange,
                                Message = "The exchange hardens. The rent feels heavier already."
                            })
                    ]),
                ["polite"] = new(
                    "polite",
                    static _ => "He mutters that sabr has limits, but he steps aside and lets the matter rest for today.",
                    []),
                ["defiant"] = new(
                    "defiant",
                    static _ => "He says nothing for a moment. Then he nods once, the way people do when they decide not to forget.",
                    [])
            });
    }

    private static FallbackSceneDefinition CreateFixerFirstContactScene()
    {
        return new FallbackSceneDefinition(
            "start",
            new Dictionary<string, FallbackNode>
            {
                ["start"] = new(
                    "start",
                    static _ => "Umm Karim watches the crowd instead of you. She says there are always errands for women who keep their mouths shut and their feet moving.",
                    [
                        new FallbackChoice(
                            "Listen carefully",
                            "listen",
                            new NarrativeOutcome
                            {
                                NpcTrustTarget = NpcId.FixerUmmKarim,
                                NpcTrustChange = 8,
                                FactionTarget = FactionId.ImbabaCrew,
                                FactionReputationChange = 6,
                                SetFlag = "fixer_met",
                                Message = "Umm Karim decides you may be useful."
                            }),
                        new FallbackChoice(
                            "Refuse and leave",
                            "leave",
                            new NarrativeOutcome
                            {
                                StressChange = -2,
                                NpcTrustTarget = NpcId.FixerUmmKarim,
                                NpcTrustChange = -6,
                                Message = "You walk away, but the offer lingers in your mind."
                            })
                    ]),
                ["listen"] = new(
                    "listen",
                    static _ => "She gives you no names and no promises. Only times, corners, and the warning that loose talk gets people buried socially if not literally.",
                    []),
                ["leave"] = new(
                    "leave",
                    static _ => "Umm Karim shrugs as if she expected nothing more. In Cairo, another desperate person is never far away.",
                    [])
            });
    }

    private static FallbackSceneDefinition CreateFixerRepeatScene()
    {
        return new FallbackSceneDefinition(
            "start",
            new Dictionary<string, FallbackNode>
            {
                ["start"] = new(
                    "start",
                    static _ => "Umm Karim does not waste greetings. She asks whether you came for real work or only stories.",
                    [
                        new FallbackChoice(
                            "Ask for more serious work",
                            "serious",
                            new NarrativeOutcome
                            {
                                NpcTrustTarget = NpcId.FixerUmmKarim,
                                NpcTrustChange = 5,
                                FactionTarget = FactionId.ImbabaCrew,
                                FactionReputationChange = 4,
                                StressChange = 4,
                                Message = "Umm Karim starts measuring you against tougher jobs."
                            }),
                        new FallbackChoice(
                            "Keep it small for now",
                            "small",
                            new NarrativeOutcome
                            {
                                NpcTrustTarget = NpcId.FixerUmmKarim,
                                NpcTrustChange = 2,
                                StressChange = -2,
                                Message = "You keep the conversation cautious."
                            })
                    ]),
                ["serious"] = new(
                    "serious",
                    static _ => "She says ambition is cheap and silence is expensive. Then she tells you where to be after sunset.",
                    []),
                ["small"] = new(
                    "small",
                    static _ => "She smirks. Survival first, empire later.",
                    [])
            });
    }

    private static FallbackSceneDefinition CreateOfficerScene(string openingText, int calmTrustGain, int silentTrustChange)
    {
        return new FallbackSceneDefinition(
            "start",
            new Dictionary<string, FallbackNode>
            {
                ["start"] = new(
                    "start",
                    _ => openingText,
                    [
                        new FallbackChoice(
                            "Answer calmly",
                            "calm",
                            new NarrativeOutcome
                            {
                                StressChange = 6,
                                NpcTrustTarget = NpcId.OfficerKhalid,
                                NpcTrustChange = calmTrustGain,
                                Message = "Khalid lets you pass, though not warmly."
                            }),
                        new FallbackChoice(
                            "Offer a small bribe",
                            "bribe",
                            new NarrativeOutcome
                            {
                                MoneyChange = -15,
                                StressChange = 8,
                                NpcTrustTarget = NpcId.OfficerKhalid,
                                NpcTrustChange = 2,
                                Message = "The bill disappears as neatly as the conversation."
                            }),
                        new FallbackChoice(
                            "Stay silent and hard",
                            "silent",
                            new NarrativeOutcome
                            {
                                StressChange = 12,
                                NpcTrustTarget = NpcId.OfficerKhalid,
                                NpcTrustChange = silentTrustChange,
                                Message = "Silence keeps your pride intact, not your comfort."
                            })
                    ]),
                ["calm"] = new(
                    "calm",
                    static _ => "He warns you to be home earlier and waves you on without apology.",
                    []),
                ["bribe"] = new(
                    "bribe",
                    static _ => "He folds the money away with a face so blank it feels rehearsed.",
                    []),
                ["silent"] = new(
                    "silent",
                    static _ => "He keeps you there long enough to make the point, then sends you on under a stare.",
                    [])
            });
    }
}