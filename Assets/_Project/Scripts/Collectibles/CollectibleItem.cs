using UnityEngine;

/// <summary>
/// A world-space collectible that bobs and spins until the player walks into it.
/// Grants coins or a free seed (as coins) on collection.
/// Created and managed by CollectibleSpawner.
/// </summary>
public class CollectibleItem : MonoBehaviour
{
    public enum CollectibleType { Coins, Seed, LuckyFind }

    private CollectibleType type;
    private int coinAmount;
    private string label; // crop name for seed drops

    private Vector3 startPos;
    private float bobOffset;
    private Transform playerTransform;
    private const float PickupRadius = 1.8f;

    // Notifies the spawner slot so it can start its respawn timer
    public System.Action OnCollected;

    public void Initialise(CollectibleType collectibleType, int coins, string displayLabel)
    {
        type       = collectibleType;
        coinAmount = coins;
        label      = displayLabel;
        startPos   = transform.position;
        bobOffset  = Random.Range(0f, Mathf.PI * 2f);

        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null) playerTransform = playerGO.transform;
    }

    private void Update()
    {
        // Bob up and down
        float y = startPos.y + Mathf.Sin(Time.time * 2.2f + bobOffset) * 0.18f;
        transform.position = new Vector3(transform.position.x, y, transform.position.z);

        // Slow spin
        transform.Rotate(0f, 70f * Time.deltaTime, 0f, Space.World);

        // Proximity pickup — CharacterController doesn't fire OnTriggerEnter reliably
        if (playerTransform != null &&
            Vector3.Distance(transform.position, playerTransform.position) < PickupRadius)
        {
            Collect();
        }
    }

    private void Collect()
    {
        GameManager.Instance.Economy.AddCoins(coinAmount);

        string msg = type switch
        {
            CollectibleType.Seed      => $"Found a {label} seed! +{coinAmount} coins",
            CollectibleType.LuckyFind => $"Lucky find! +{coinAmount} coins",
            _                         => $"Found {coinAmount} coins!"
        };

        HUDManager.Instance?.ShowNotification(msg);
        AudioManager.Instance?.PlayCollect();

        OnCollected?.Invoke();
        Destroy(gameObject);
    }
}
