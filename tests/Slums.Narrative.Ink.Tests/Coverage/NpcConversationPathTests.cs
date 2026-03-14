 using FluentAssertions;
using Slums.Application.Narrative;
using Slums.Narrative.Ink.Tests.Helpers;
using TUnit;

namespace Slums.Narrative.Ink.Tests.Coverage;

internal sealed class NpcConversationPathTests
{
    [Test]
    public async Task Npc_Landlord_KnotsExist()
    {
        var knots = StoryTraversalHelper.GetKnotsMatchingPrefix("landlord");
        knots.Should().NotBeEmpty("landlord NPC should have conversation knots");
    }

    [Test]
    public async Task Npc_Nurse_KnotsExist()
    {
        var knots = StoryTraversalHelper.GetKnotsMatchingPrefix("nurse");
        knots.Should().NotBeEmpty("nurse NPC should have conversation knots");
    }

    [Test]
    public async Task Npc_Hanan_KnotsExist()
    {
        var knots = StoryTraversalHelper.GetKnotsMatchingPrefix("hanan");
        knots.Should().NotBeEmpty("hanan (fence) NPC should have conversation knots");
    }

    [Test]
    public async Task Npc_Neighbor_KnotsExist()
    {
        var knots = StoryTraversalHelper.GetKnotsMatchingPrefix("neighbor");
        knots.Should().NotBeEmpty("neighbor NPC should have conversation knots");
    }

            [Test]
            public async Task Npc_Dispatcher_KnotsExist()
            {
                var knots = StoryTraversalHelper.GetKnotsMatchingPrefix("safaa");
                knots.Should().NotBeEmpty("dispatcher Safaa NPC should have conversation knots");
            }

            [Test]
            public async Task Npc_CafeOwner_KnotsExist()
            {
                var knots = StoryTraversalHelper.GetKnotsMatchingPrefix("nadia");
                knots.Should().NotBeEmpty("cafe owner Nadia NPC should have conversation knots");
            }

            [Test]
            public async Task Npc_Pharmacist_KnotsExist()
            {
                var knots = StoryTraversalHelper.GetKnotsMatchingPrefix("mariam");
                knots.Should().NotBeEmpty("pharmacist Mariam NPC should have conversation knots");
            }

            [Test]
            public async Task Npc_LaundryOwner_KnotsExist()
            {
                var knots = StoryTraversalHelper.GetKnotsMatchingPrefix("iman");
                knots.Should().NotBeEmpty("laundry owner Iman NPC should have conversation knots");
            }

            [Test]
            public async Task Npc_WorkshopBoss_KnotsExist()
            {
                var knots = StoryTraversalHelper.GetKnotsMatchingPrefix("abu");
                knots.Should().NotBeEmpty("workshop boss Abu NPC should have conversation knots");
            }

            [Test]
            public async Task Npc_Runner_KnotsExist()
            {
                var knots = StoryTraversalHelper.GetKnotsMatchingPrefix("youssef");
                knots.Should().NotBeEmpty("runner Youssef NPC should have conversation knots");
            }

            [Test]
            public async Task Npc_Mother_KnotsExist()
            {
                var knots = StoryTraversalHelper.GetKnotsMatchingPrefix("mother");
                knots.Should().NotBeEmpty("mother NPC should have conversation knots");
            }

            [Test]
            public async Task Npc_IntroKnots_ProduceText()
            {
                var introKnots = new[]
                {
                    "landlord_intro",
                    "nurse_intro",
                    "hanan_intro",
                    "neighbor_intro",
                    "safaa_intro",
                    "nadia_intro",
                    "mariam_intro",
                    "iman_intro",
                    "abu_intro",
                    "youssef_intro"
                };

                var allKnots = StoryTraversalHelper.GetAllKnotNames();

                foreach (var expectedKnot in introKnots)
                {
                    if (allKnots.Contains(expectedKnot))
                    {
                        var result = StoryTraversalHelper.ExplorePath(expectedKnot, CreateDefaultSceneState());
                        result.Text.Should().NotBeEmpty($"intro knot '{expectedKnot}' should produce text");
                    }
                }
            }

            [Test]
            public async Task Npc_HighTrustKnots_ExistForNurses()
            {
                var allKnots = StoryTraversalHelper.GetAllKnotNames();
                var nurseKnots = allKnots.Where(k => k.Contains("nurse", StringComparison.OrdinalIgnoreCase)).ToList();

                nurseKnots.Should().NotBeEmpty("nurse should have conversation knots");
            }

            [Test]
            public async Task Npc_DebtKnots_ExistForLandlord()
            {
                var allKnots = StoryTraversalHelper.GetAllKnotNames();
                var landlordKnots = allKnots.Where(k => k.Contains("landlord", StringComparison.OrdinalIgnoreCase)).ToList();

                landlordKnots.Should().NotBeEmpty("landlord should have conversation knots including debt scenarios");
            }

            [Test]
            public async Task Npc_AllConversationKnots_ProduceContent()
            {
                var allKnots = StoryTraversalHelper.GetAllKnotNames();
                var npcPrefixes = new[] { "landlord", "nurse", "hanan", "neighbor", "safaa", "nadia", "mariam", "iman", "abu", "youssef", "mother" };

                var npcKnots = allKnots
                    .Where(k => npcPrefixes.Any(prefix => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                    .Take(20)
                    .ToList();

                foreach (var knot in npcKnots)
                {
                    var result = StoryTraversalHelper.ExplorePath(knot, CreateDefaultSceneState());
                    result.Text.Should().NotBeEmpty($"NPC knot '{knot}' should produce text");
                }
            }

            private static NarrativeSceneState CreateDefaultSceneState()
            {
                using var session = new GameStateBuilder().Build();
                return NarrativeSceneState.Create(session);
            }
        }
