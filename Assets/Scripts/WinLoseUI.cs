using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows the WinPanel or LosePanel (built by UIFactory) when the game ends.
/// Wires Restart buttons at runtime so onClick listeners survive scene saves.
/// Attached to the UI canvas root by UIFactory.
/// </summary>
public class WinLoseUI : MonoBehaviour
{
    private GameObject winPanel;
    private GameObject losePanel;

    private void Awake()
    {
        winPanel  = transform.Find("WinPanel")?.gameObject;
        losePanel = transform.Find("LosePanel")?.gameObject;

        SetActive(winPanel,  false);
        SetActive(losePanel, false);

        WireRestartButton(winPanel);
        WireRestartButton(losePanel);
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        SetActive(winPanel,  GameManager.Instance.State == GameState.Won);
        SetActive(losePanel, GameManager.Instance.State == GameState.Lost);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static void SetActive(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }

    private static void WireRestartButton(GameObject panel)
    {
        if (panel == null) return;
        var btn = panel.transform.Find("RestartButton")?.GetComponent<Button>();
        if (btn == null) return;

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => GameManager.Instance?.RestartGame());
    }
}
