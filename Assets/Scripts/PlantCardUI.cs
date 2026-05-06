using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives a single plant card in the HUD.
///
/// Responsibilities:
///   - Toggle plant selection through GameManager when clicked.
///   - Show a yellow highlight when this card is the active selection.
///   - Show a dark dim overlay when the player cannot afford this plant.
///   - Display the sun cost.
///
/// Child object layout expected by Awake (built by UIFactory):
///   CardImage         – Image displaying the plant card art
///   CostText          – TextMeshProUGUI displaying the sun cost
///   SelectionHighlight – Image overlay shown when selected (yellow tint)
///   DimOverlay         – Image overlay shown when unaffordable (dark tint)
/// </summary>
public class PlantCardUI : MonoBehaviour
{
    // Set by UIFactory after component is added to the scene object.
    [HideInInspector] public PlantType plantType;
    [HideInInspector] public int sunCost;

    private Image           selectionHighlight;
    private Image           dimOverlay;
    private TextMeshProUGUI costText;
    private Image           cooldownOverlay;
    private TextMeshProUGUI cooldownText;

    private void Awake()
    {
        selectionHighlight = transform.Find("SelectionHighlight")?.GetComponent<Image>();
        dimOverlay         = transform.Find("DimOverlay")?.GetComponent<Image>();
        costText           = transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();
        cooldownOverlay    = transform.Find("CooldownOverlay")?.GetComponent<Image>();
        cooldownText       = transform.Find("CooldownText")?.GetComponent<TextMeshProUGUI>();

        if (costText != null)
            costText.text = sunCost.ToString();

        var button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(OnClicked);
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        bool isSelected   = GameManager.Instance.SelectedPlant == plantType;
        bool canAfford    = GameManager.Instance.Sun >= sunCost;
        bool isOnCooldown = GameManager.Instance.IsOnCooldown(plantType);
        float cdFraction  = GameManager.Instance.GetCooldownFraction(plantType);

        // Selection highlight.
        if (selectionHighlight != null)
            selectionHighlight.gameObject.SetActive(isSelected);

        // Dim when unaffordable OR on cooldown.
        if (dimOverlay != null)
            dimOverlay.gameObject.SetActive(!canAfford && !isOnCooldown);

        // Cooldown overlay: fill amount shrinks as cooldown drains.
        if (cooldownOverlay != null)
        {
            cooldownOverlay.gameObject.SetActive(isOnCooldown);
            cooldownOverlay.fillAmount = cdFraction;
        }

        // Cooldown text: shows remaining seconds.
        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(isOnCooldown);
            if (isOnCooldown)
            {
                float remaining = cdFraction * GameManager.Instance.GetCooldownDuration(plantType);
                cooldownText.text = Mathf.CeilToInt(remaining).ToString();
            }
        }

        // Button interactable only when affordable and not on cooldown.
        var btn = GetComponent<UnityEngine.UI.Button>();
        if (btn != null)
            btn.interactable = canAfford && !isOnCooldown;
    }

    private void OnClicked()
    {
        if (GameManager.Instance == null) return;

        // Clicking the already-selected card deselects it.
        if (GameManager.Instance.SelectedPlant == plantType)
            GameManager.Instance.CancelSelection();
        else
            GameManager.Instance.SelectPlant(plantType);
    }
}
