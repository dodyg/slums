using Slums.Core.Crimes;

namespace Slums.Core.Narrative;

public static class StoryFlags
{
    public const string CrimeFirstSuccess = "crime_first_success";
    public const string CrimeWarning = "crime_warning";
    public const string MotherClinicFirstVisit = "mother_clinic_first_visit";
    public const string EventPublicWorkHeatSeen = "event_public_work_heat_seen";
    public const string BackgroundMedicalClinicSeen = "background_medical_clinic_seen";
    public const string EventMotherWrongMoneySeen = "event_mother_wrong_money_seen";
    public const string EventNeighborWatchSeen = "event_neighbor_watch_seen";
    public const string EventRentFinalWarningSeen = "event_rent_final_warning_seen";
    public const string EventInvestmentSuspensionSeen = "event_investment_suspension_seen";
    public const string BackgroundPrisonerHeatSeen = "background_prisoner_heat_seen";
    public const string BackgroundSudaneseSolidaritySeen = "background_sudanese_solidarity_seen";

    public const string CrimeHananFenceSuccessSeen = "crime_hanan_fence_success_seen";
    public const string CrimeHananFenceDetectedSeen = "crime_hanan_fence_detected_seen";
    public const string CrimeHananFenceFailureSeen = "crime_hanan_fence_failure_seen";
    public const string CrimeYoussefDropSuccessSeen = "crime_youssef_drop_success_seen";
    public const string CrimeYoussefDropDetectedSeen = "crime_youssef_drop_detected_seen";
    public const string CrimeYoussefDropFailureSeen = "crime_youssef_drop_failure_seen";
    public const string CrimeUmmKarimErrandSuccessSeen = "crime_ummkarim_errand_success_seen";
    public const string CrimeUmmKarimErrandDetectedSeen = "crime_ummkarim_errand_detected_seen";
    public const string CrimeUmmKarimErrandFailureSeen = "crime_ummkarim_errand_failure_seen";
    public const string CrimeSafaaSkimSuccessSeen = "crime_safaa_skim_success_seen";
    public const string CrimeSafaaSkimDetectedSeen = "crime_safaa_skim_detected_seen";
    public const string CrimeSafaaSkimFailureSeen = "crime_safaa_skim_failure_seen";
    public const string CrimeImanBundleSuccessSeen = "crime_iman_bundle_success_seen";
    public const string CrimeImanBundleDetectedSeen = "crime_iman_bundle_detected_seen";
    public const string CrimeImanBundleFailureSeen = "crime_iman_bundle_failure_seen";
    public const string CrimeHananCoverSeen = "crime_hanan_cover_seen";
    public const string CrimeHananSalvageSeen = "crime_hanan_salvage_seen";
    public const string CrimeYoussefTipoffSeen = "crime_youssef_tipoff_seen";
    public const string CrimeYoussefEscapeSeen = "crime_youssef_escape_seen";
    public const string CrimeSafaaRerouteSeen = "crime_safaa_reroute_seen";
    public const string CrimeSafaaSalvageSeen = "crime_safaa_salvage_seen";
    public const string CrimeImanCoverSeen = "crime_iman_cover_seen";
    public const string CrimeImanExitSeen = "crime_iman_exit_seen";

    public static IReadOnlyList<string> GetCrimeRouteAftermathFlags(CrimeType crimeType)
    {
        return crimeType switch
        {
            CrimeType.MarketFencing => [CrimeHananFenceSuccessSeen, CrimeHananFenceDetectedSeen, CrimeHananFenceFailureSeen],
            CrimeType.DokkiDrop => [CrimeYoussefDropSuccessSeen, CrimeYoussefDropDetectedSeen, CrimeYoussefDropFailureSeen],
            CrimeType.NetworkErrand => [CrimeUmmKarimErrandSuccessSeen, CrimeUmmKarimErrandDetectedSeen, CrimeUmmKarimErrandFailureSeen],
            CrimeType.DepotFareSkim => [CrimeSafaaSkimSuccessSeen, CrimeSafaaSkimDetectedSeen, CrimeSafaaSkimFailureSeen],
            CrimeType.ShubraBundleLift => [CrimeImanBundleSuccessSeen, CrimeImanBundleDetectedSeen, CrimeImanBundleFailureSeen],
            _ => []
        };
    }
}
