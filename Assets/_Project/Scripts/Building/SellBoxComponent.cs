using UnityEngine;

/// <summary>
/// Attach to a Sell Box building.
/// Player walks into the trigger zone and presses E to sell all inventory.
/// Shows a context hint while in range.
/// </summary>
public class SellBoxComponent : MonoBehaviour
{
    [SerializeField] private float triggerRadius = 3f;

    private bool _playerInRange;
    private Transform _player;

    private void Start()
    {
        // Create a sphere trigger collider at runtime
        var col = gameObject.GetComponent<SphereCollider>() ?? gameObject.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius    = triggerRadius;

        var go = GameObject.FindWithTag("Player");
        if (go != null) _player = go.transform;
    }

    private void Update()
    {
        if (!_playerInRange) return;

        if (Input.GetKeyDown(KeyCode.E))
            Sell();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = true;
        HUDManager.Instance?.SetContextHint("Press E to Sell All");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = false;
        HUDManager.Instance?.SetContextHint("");
    }

    private void Sell()
    {
        if (GameManager.Instance?.Inventory == null) return;

        int earned = GameManager.Instance.Inventory.SellAll(); // AddCoins called internally
        if (earned > 0)
        {
            GameManager.Instance.Economy.AddLifetimeEarnings(earned);
            GameManager.Instance.RealTime?.ResetAutosaveTimer();
            AudioManager.Instance?.PlaySell();
            HUDManager.Instance?.ShowNotification($"Sold everything for {earned} coins!");
            Debug.Log($"[SellBox] Sold for {earned} coins. Total: {GameManager.Instance.Economy.Coins} | Lifetime: {GameManager.Instance.Economy.LifetimeEarnings}");
        }
        else
        {
            HUDManager.Instance?.ShowNotification("Nothing to sell!");
            Debug.Log("[SellBox] Nothing to sell.");
        }
    }
}
