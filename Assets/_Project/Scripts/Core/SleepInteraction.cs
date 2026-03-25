using UnityEngine;

/// <summary>
/// Attach to any bed or farmhouse object.
/// When the player is within range and presses E, triggers the day transition.
///
/// Setup:
///   1. Add this component to your bed/farmhouse GameObject
///   2. Optionally add a DayTransition GameObject to the scene
///      (one is auto-created at runtime if missing)
///   3. Make sure GameTimeManager and EnergyManager are in the scene
/// </summary>
public class SleepInteraction : MonoBehaviour
{
    [SerializeField] private float interactRadius = 2.5f;
    [SerializeField] private string promptText    = "Press E to sleep";

    private Transform playerTransform;
    private bool playerNearby;

    private void Start()
    {
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null) playerTransform = playerGO.transform;

        // Auto-create DayTransition if not in scene
        if (DayTransition.Instance == null)
            new GameObject("DayTransition").AddComponent<DayTransition>();
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        playerNearby = dist <= interactRadius;

        if (playerNearby)
        {
            HUDManager.Instance?.SetContextHint(promptText);

            if (Input.GetKeyDown(KeyCode.E))
                Sleep();
        }
    }

    private void Sleep()
    {
        if (GameTimeManager.Instance == null) return;

        var time   = GameTimeManager.Instance;
        int nextDay = time.CurrentDay + 1 > GameTimeManager.DaysPerSeason ? 1 : time.CurrentDay + 1;

        // Work out what season the morning message should show
        // (season change fires inside AdvanceDay, so compute the display beforehand)
        string morning = BuildMorningMessage(time);

        DayTransition.Instance.Play(morning, () =>
        {
            GameTimeManager.Instance.AdvanceDay();
            GameManager.Instance?.SaveManager?.SaveGame();
        });
    }

    private string BuildMorningMessage(GameTimeManager time)
    {
        int nextDay = time.CurrentDay + 1;

        if (nextDay > GameTimeManager.DaysPerSeason)
        {
            // Season is about to change
            int nextSeasonIndex = ((int)time.CurrentSeason + 1) % 4;
            Season nextSeason   = (Season)nextSeasonIndex;
            int nextYear        = nextSeasonIndex == 0 ? time.CurrentYear + 1 : time.CurrentYear;
            string yearStr      = nextSeasonIndex == 0 ? $"  Year {nextYear}" : "";
            return $"{nextSeason.DisplayName()} begins{yearStr}";
        }

        return $"Day {nextDay}  -  {time.CurrentSeason.DisplayName()}  Year {time.CurrentYear}";
    }
}
