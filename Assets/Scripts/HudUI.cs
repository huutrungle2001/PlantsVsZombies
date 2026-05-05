using TMPro;
using UnityEngine;

/// <summary>
/// Drives the in-game HUD.
///
/// Responsibilities:
///   - Keep the sun counter text in sync with GameManager.Sun.
///   - Cancel the active plant selection when the player presses Escape
///     or right-clicks anywhere on the screen.
///
/// Child object layout expected by Awake (built by UIFactory):
///   SunDisplay/SunText  – TextMeshProUGUI for the sun count
/// </summary>
public class HudUI : MonoBehaviour
{
    private TextMeshProUGUI sunText;

    private void Awake()
    {
        var sunDisplay = transform.Find("SunDisplay");
        if (sunDisplay != null)
        {
            var textTransform = sunDisplay.Find("SunText");
            if (textTransform != null)
                sunText = textTransform.GetComponent<TextMeshProUGUI>();
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        // Sync sun counter.
        if (sunText != null)
            sunText.text = GameManager.Instance.Sun.ToString();

        // Cancel selection with Escape or right-click.
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            GameManager.Instance.CancelSelection();
    }
}
