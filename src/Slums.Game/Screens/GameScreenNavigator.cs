using SadConsole;
using Slums.Application.Activities;
using Slums.Application.HouseholdAssets;
using Slums.Application.Investments;
using Slums.Core.State;

namespace Slums.Game.Screens;

internal sealed class GameScreenNavigator
{
    private readonly GameRuntime _runtime;
    private readonly GameSession _gameState;
    private readonly GameScreen _parentScreen;
    private readonly WorkMenuStatusQuery _workMenuStatusQuery = new();
    private readonly CrimeMenuStatusQuery _crimeMenuStatusQuery = new();
    private readonly TalkNpcStatusQuery _talkNpcStatusQuery = new();
    private readonly ClinicTravelMenuQuery _clinicTravelMenuQuery = new();
    private readonly EntertainmentMenuQuery _entertainmentMenuQuery = new();
    private readonly TrainingMenuQuery _trainingMenuQuery = new();
    private readonly InvestmentMenuQuery _investmentMenuQuery = new();
    private readonly HouseholdAssetsMenuQuery _householdAssetsMenuQuery = new();
    private readonly CommunityEventMenuQuery _communityEventMenuQuery = new();

    public GameScreenNavigator(GameRuntime runtime, GameSession gameState, GameScreen parentScreen)
    {
        _runtime = runtime;
        _gameState = gameState;
        _parentScreen = parentScreen;
    }

    public bool ShowMenu(GameActionId actionId)
    {
        return actionId switch
        {
            GameActionId.Work => ShowWorkMenu(),
            GameActionId.Crime => ShowCrimeMenu(),
            GameActionId.Talk => ShowTalkMenu(),
            GameActionId.Entertainment => ShowEntertainmentMenu(),
            GameActionId.Train => ShowTrainingMenu(),
            GameActionId.HomeImprovement => ShowHomeUpgradeMenu(),
            GameActionId.CommunityEvent => ShowCommunityEventMenu(),
            GameActionId.Invest => ShowInvestmentMenu(),
            GameActionId.Shop => ShowShopMenu(),
            GameActionId.HouseholdAssets => ShowHouseholdAssetsMenu(),
            GameActionId.TakeMotherToClinic => ShowClinicTravelMenu(),
            GameActionId.Travel => ShowTravelMenu(),
            GameActionId.Phone => ShowPhoneMenu(),
            GameActionId.SaveGame => ShowSaveMenu(),
            _ => false
        };
    }

    private bool ShowWorkMenu()
    {
        var workContext = WorkMenuContext.Create(_gameState);
        var jobs = _workMenuStatusQuery.GetStatuses(workContext).ToList();
        if (jobs.Count == 0)
        {
            _parentScreen.AddEventLogEntry("No work available here.");
            return false;
        }

        NavigateTo(new WorkScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _gameState, workContext, jobs, _parentScreen));
        return true;
    }

    private bool ShowCrimeMenu()
    {
        var crimeContext = CrimeMenuContext.Create(_gameState);
        var crimes = _crimeMenuStatusQuery.GetStatuses(crimeContext).ToList();
        if (crimes.Count == 0)
        {
            _parentScreen.AddEventLogEntry("No crime opportunities here.");
            return false;
        }

        NavigateTo(new CrimeScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime, _gameState, crimeContext, crimes, _parentScreen));
        return true;
    }

    private bool ShowTalkMenu()
    {
        var talkContext = TalkNpcContext.Create(_gameState);
        var npcStatuses = _talkNpcStatusQuery.GetStatuses(talkContext);
        if (npcStatuses.Count == 0)
        {
            _parentScreen.AddEventLogEntry("No one is available to talk.");
            return false;
        }

        NavigateTo(new TalkScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime, _gameState, talkContext, npcStatuses, _parentScreen));
        return true;
    }

    private bool ShowShopMenu()
    {
        var shopContext = ShopMenuContext.Create(_gameState);
        NavigateTo(new ShopScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _gameState, shopContext, _parentScreen));
        return true;
    }

    public bool ShowTravelMenu()
    {
        var locations = _gameState.World.GetTravelableLocations().ToList();
        if (locations.Count == 0)
        {
            _parentScreen.AddEventLogEntry("No travel destinations available");
            return false;
        }

        NavigateTo(new TravelScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _gameState, locations, _parentScreen));
        return true;
    }

    private bool ShowPhoneMenu()
    {
        var phoneContext = PhoneMenuContext.Create(_gameState);
        NavigateTo(new PhoneScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _gameState, phoneContext, _parentScreen));
        return true;
    }

    public bool ShowSaveMenu()
    {
        NavigateTo(new SaveGameScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _runtime, _gameState, _parentScreen));
        return true;
    }

    public bool ShowEventLogViewer()
    {
        NavigateTo(new EventLogViewerScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _parentScreen, _parentScreen.GetEventLogEntries()));
        return true;
    }

    private bool ShowClinicTravelMenu()
    {
        var clinicContext = ClinicTravelMenuContext.Create(_gameState);
        var clinics = _clinicTravelMenuQuery.GetStatuses(clinicContext).ToList();
        if (clinics.Count == 0)
        {
            _parentScreen.AddEventLogEntry("No clinics available.");
            return false;
        }

        NavigateTo(new ClinicTravelScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _gameState, clinicContext, clinics, _parentScreen));
        return true;
    }

    private bool ShowEntertainmentMenu()
    {
        var entertainmentContext = EntertainmentMenuContext.Create(_gameState);
        var activities = _entertainmentMenuQuery.GetStatuses(entertainmentContext).ToList();
        if (activities.Count == 0)
        {
            _parentScreen.AddEventLogEntry("No entertainment available here.");
            return false;
        }

        NavigateTo(new EntertainmentScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _gameState, entertainmentContext, activities, _parentScreen));
        return true;
    }

    private bool ShowTrainingMenu()
    {
        var trainingContext = TrainingMenuContext.Create(_gameState);
        var activities = _trainingMenuQuery.GetStatuses(trainingContext).ToList();
        if (activities.Count == 0)
        {
            _parentScreen.AddEventLogEntry("No training available right now.");
            return false;
        }

        NavigateTo(new TrainingScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _gameState, trainingContext, activities, _parentScreen));
        return true;
    }

    private bool ShowHomeUpgradeMenu()
    {
        var availableUpgrades = _gameState.GetAvailableHomeUpgrades();
        if (availableUpgrades.Count == 0)
        {
            _parentScreen.AddEventLogEntry("No home upgrades available.");
            return false;
        }

        NavigateTo(new HomeUpgradeScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _gameState, _parentScreen));
        return true;
    }

    private bool ShowCommunityEventMenu()
    {
        var context = CommunityEventMenuContext.Create(_gameState);
        var events = _communityEventMenuQuery.GetStatuses(context).ToList();
        if (events.Count == 0)
        {
            _parentScreen.AddEventLogEntry("No community events available right now.");
            return false;
        }

        NavigateTo(new CommunityEventScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _gameState, context, events, _parentScreen));
        return true;
    }

    private bool ShowInvestmentMenu()
    {
        var investmentContext = InvestmentMenuContext.Create(_gameState);
        var investments = _investmentMenuQuery.GetStatuses(investmentContext).ToList();
        if (investments.Count == 0)
        {
            _parentScreen.AddEventLogEntry("No investment opportunities are being offered here.");
            return false;
        }

        NavigateTo(new InvestmentMenuScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _gameState, investmentContext, investments, _parentScreen));
        return true;
    }

    private bool ShowHouseholdAssetsMenu()
    {
        var householdContext = HouseholdAssetsMenuContext.Create(_gameState);
        var statuses = _householdAssetsMenuQuery.GetStatuses(householdContext).ToList();
        if (statuses.Count == 0)
        {
            _parentScreen.AddEventLogEntry("Nothing in the household-assets flow is available right now.");
            return false;
        }

        NavigateTo(new HouseholdAssetsScreen(GameRuntime.ScreenWidth, GameRuntime.ScreenHeight, _gameState, householdContext, statuses, _parentScreen));
        return true;
    }

    private void NavigateTo(ScreenSurface screen)
    {
        _parentScreen.IsFocused = false;
        ScreenTransition.FadeTo(screen);
    }
}
