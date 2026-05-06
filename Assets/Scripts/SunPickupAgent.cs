using UnityEngine;

/// <summary>
/// A sun orb the player clicks to collect sun.
/// Spawned by SunflowerAgent. Disappears after its lifetime expires.
/// </summary>
public class SunPickupAgent : MonoBehaviour
{
    [Header("Value")]
    [SerializeField] private int sunValue = 25;

    [Header("Timing")]
    [Tooltip("Seconds before the orb fades away uncollected.")]
    [SerializeField] private float lifetime = 8f;

    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;

    private float age;
    private bool  collected;

    private void Update()
    {
        if (collected) return;

        age += Time.deltaTime;
        if (age >= lifetime)
            Destroy(gameObject);
    }

    private void OnMouseDown()
    {
        Collect();
    }

    /// <summary>Adds sun to GameManager and destroys this pickup.</summary>
    public void Collect()
    {
        if (collected) return;
        collected = true;

        if (GameManager.Instance != null)
            GameManager.Instance.AddSun(sunValue);

        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

        Destroy(gameObject);
    }
}
