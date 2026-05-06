using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Editor utility that builds the in-game HUD hierarchy inside the Main scene.
///
/// Interactive use  – Unity menu: PvZ > UI > Build HUD
/// CLI / batch use  – called from ProjectSetup.FullSetup
///
/// Built hierarchy (parented to the existing "UI" root object):
///
///   UI  [Canvas, CanvasScaler, GraphicRaycaster, HudUI]
///     SunDisplay
///       SunIcon   [Image – Sun.png]
///       SunText   [TextMeshProUGUI – live sun count]
///     CardArea
///       PeashooterCard  [Button, PlantCardUI]
///         Background        [Image]
///         CardImage         [Image – PeaShooter.png]
///         CostText          [TextMeshProUGUI – "100"]
///         SelectionHighlight [Image – yellow, hidden]
///         DimOverlay         [Image – dark, shown when unaffordable]
///       SunflowerCard   [Button, PlantCardUI]
///         ... (same structure)
///   EventSystem  [EventSystem, StandaloneInputModule]
/// </summary>
public static class UIFactory
{
    private const string ScenePath   = "Assets/Scenes/Main.unity";
    private const string SunIconPath = "Assets/Art/items/Sun.png";
    private const string PeaCardPath = "Assets/Art/Cards/PeaShooter.png";
    private const string SunCardPath = "Assets/Art/Cards/SunFlower.png";

    // Reference resolution the CanvasScaler targets.
    private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

    // Card dimensions in reference pixels.
    private const float CardW = 80f;
    private const float CardH = 110f;
    private const float CardGap = 8f;

    // Top-left padding.
    private const float PadX = 12f;
    private const float PadY = 12f;

    // -------------------------------------------------------------------------
    // Entry points
    // -------------------------------------------------------------------------

    [MenuItem("PvZ/UI/Build HUD")]
    public static void BuildHud()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        EnsureEventSystem();

        var uiRoot = SetupCanvas();

        BuildSunDisplay(uiRoot);
        BuildCardArea(uiRoot);
        BuildWinLoseOverlay(uiRoot);

        // WinLoseUI drives the panels at runtime.
        if (uiRoot.GetComponent<WinLoseUI>() == null)
            uiRoot.AddComponent<WinLoseUI>();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("[UIFactory] HUD built and scene saved.");
    }

    // -------------------------------------------------------------------------
    // Canvas setup
    // -------------------------------------------------------------------------

    private static GameObject SetupCanvas()
    {
        var uiRoot = GameObject.Find("UI");
        if (uiRoot == null)
        {
            uiRoot = new GameObject("UI");
            Debug.LogWarning("[UIFactory] 'UI' root not found – created a new one.");
        }

        // Canvas
        var canvas = uiRoot.GetComponent<Canvas>();
        if (canvas == null) canvas = uiRoot.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        // CanvasScaler – scale with screen size so layout works at any resolution.
        var scaler = uiRoot.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = uiRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = ReferenceResolution;
        scaler.matchWidthOrHeight  = 0.5f;

        // GraphicRaycaster – needed for button clicks.
        if (uiRoot.GetComponent<GraphicRaycaster>() == null)
            uiRoot.AddComponent<GraphicRaycaster>();

        // HudUI runtime component.
        if (uiRoot.GetComponent<HudUI>() == null)
            uiRoot.AddComponent<HudUI>();

        return uiRoot;
    }

    // -------------------------------------------------------------------------
    // Sun display
    // -------------------------------------------------------------------------

    private static void BuildSunDisplay(GameObject parent)
    {
        var display = FindOrCreate(parent.transform, "SunDisplay");
        var rt = SetRect(display, PadX + 60f, -(PadY + 20f), 120f, 40f);
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        // Background tint
        var bg = display.GetComponent<Image>();
        if (bg == null) bg = display.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.45f);

        // Sun icon
        var iconGo = FindOrCreate(display.transform, "SunIcon");
        SetRect(iconGo, -40f, 0f, 32f, 32f);
        var iconImg = iconGo.GetComponent<Image>();
        if (iconImg == null) iconImg = iconGo.AddComponent<Image>();
        iconImg.sprite = LoadSprite(SunIconPath);
        iconImg.preserveAspect = true;

        // Sun count text
        var textGo = FindOrCreate(display.transform, "SunText");
        SetRect(textGo, 16f, 0f, 64f, 36f);
        var tmp = textGo.GetComponent<TextMeshProUGUI>();
        if (tmp == null) tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text      = "50";
        tmp.fontSize  = 22;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = Color.yellow;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
    }

    // -------------------------------------------------------------------------
    // Card area
    // -------------------------------------------------------------------------

    private static void BuildCardArea(GameObject parent)
    {
        var area = FindOrCreate(parent.transform, "CardArea");
        // Position the card area below the sun display.
        float areaTop = PadY + 40f + CardGap + 10f;
        SetRect(area, PadX + CardW * 0.5f, -(areaTop), CardW,
                CardH * 2 + CardGap);
        var rt = area.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);

        BuildCard(area.transform, "PeashooterCard", PlantType.Peashooter,
                  100, PeaCardPath, 0f);
        BuildCard(area.transform, "SunflowerCard",  PlantType.Sunflower,
                  50,  SunCardPath,  -(CardH + CardGap));
    }

    private static void BuildCard(
        Transform parent,
        string    goName,
        PlantType plantType,
        int       sunCost,
        string    artPath,
        float     localY)
    {
        var card = FindOrCreate(parent, goName);

        // Position within the card area.
        var rt = card.GetComponent<RectTransform>();
        if (rt == null) rt = card.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, localY);
        rt.sizeDelta = new Vector2(CardW, CardH);

        // Button component for click handling.
        if (card.GetComponent<Button>() == null)
            card.AddComponent<Button>();

        // PlantCardUI component – set fields via SerializedObject so they persist.
        var cardUi = card.GetComponent<PlantCardUI>();
        if (cardUi == null) cardUi = card.AddComponent<PlantCardUI>();
        var so = new SerializedObject(cardUi);
        so.FindProperty("plantType").enumValueIndex = (int)plantType;
        so.FindProperty("sunCost").intValue          = sunCost;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Background panel.
        var bgGo  = FindOrCreate(card.transform, "Background");
        FillParent(bgGo);
        var bgImg = bgGo.GetComponent<Image>();
        if (bgImg == null) bgImg = bgGo.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.75f);

        // Card art image.
        var artGo  = FindOrCreate(card.transform, "CardImage");
        SetRect(artGo, 0f, -5f, CardW - 8f, CardH - 28f);
        var artImg = artGo.GetComponent<Image>();
        if (artImg == null) artImg = artGo.AddComponent<Image>();
        artImg.sprite         = LoadSprite(artPath);
        artImg.preserveAspect = true;

        // Sun cost text at the bottom of the card.
        var costGo  = FindOrCreate(card.transform, "CostText");
        SetRect(costGo, 0f, -(CardH - 16f), CardW, 22f);
        var costTmp = costGo.GetComponent<TextMeshProUGUI>();
        if (costTmp == null) costTmp = costGo.AddComponent<TextMeshProUGUI>();
        costTmp.text      = sunCost.ToString();
        costTmp.fontSize  = 16;
        costTmp.fontStyle = FontStyles.Bold;
        costTmp.color     = Color.yellow;
        costTmp.alignment = TextAlignmentOptions.Center;

        // Yellow selection highlight (hidden by default).
        var selGo  = FindOrCreate(card.transform, "SelectionHighlight");
        FillParent(selGo);
        var selImg = selGo.GetComponent<Image>();
        if (selImg == null) selImg = selGo.AddComponent<Image>();
        selImg.color = new Color(1f, 0.92f, 0f, 0.35f);
        selGo.SetActive(false);

        // Dark dim overlay shown when the player cannot afford this plant.
        var dimGo  = FindOrCreate(card.transform, "DimOverlay");
        FillParent(dimGo);
        var dimImg = dimGo.GetComponent<Image>();
        if (dimImg == null) dimImg = dimGo.AddComponent<Image>();
        dimImg.color = new Color(0f, 0f, 0f, 0.55f);
        dimGo.SetActive(false);

        // Cooldown overlay – fills from top, shrinks as cooldown drains.
        var cdOverlayGo  = FindOrCreate(card.transform, "CooldownOverlay");
        FillParent(cdOverlayGo);
        var cdImg = cdOverlayGo.GetComponent<Image>();
        if (cdImg == null) cdImg = cdOverlayGo.AddComponent<Image>();
        cdImg.color      = new Color(0f, 0f, 0f, 0.70f);
        cdImg.type       = Image.Type.Filled;
        cdImg.fillMethod = Image.FillMethod.Vertical;
        cdImg.fillOrigin = (int)Image.OriginVertical.Top;
        cdImg.fillAmount = 0f;
        cdOverlayGo.SetActive(false);

        // Cooldown countdown text shown on top of the overlay.
        var cdTextGo = FindOrCreate(card.transform, "CooldownText");
        SetRect(cdTextGo, 0f, -(CardH * 0.25f), CardW, 28f);
        var cdTmp = cdTextGo.GetComponent<TextMeshProUGUI>();
        if (cdTmp == null) cdTmp = cdTextGo.AddComponent<TextMeshProUGUI>();
        cdTmp.text      = "";
        cdTmp.fontSize  = 20;
        cdTmp.fontStyle = FontStyles.Bold;
        cdTmp.color     = Color.white;
        cdTmp.alignment = TextAlignmentOptions.Center;
        cdTextGo.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // Win / Lose overlay
    // -------------------------------------------------------------------------

    private static void BuildWinLoseOverlay(GameObject parent)
    {
        BuildResultPanel(parent, "WinPanel",
            "YOU WIN!",
            new Color(0f, 0.1f, 0f, 0.82f),
            new Color(0.3f, 1f, 0.3f, 1f));

        BuildResultPanel(parent, "LosePanel",
            "YOU LOSE!",
            new Color(0.15f, 0f, 0f, 0.82f),
            new Color(1f, 0.3f, 0.3f, 1f));
    }

    private static void BuildResultPanel(
        GameObject parent,
        string     panelName,
        string     titleText,
        Color      bgColor,
        Color      titleColor)
    {
        var panel = FindOrCreate(parent.transform, panelName);
        FillParent(panel);
        var bg = panel.GetComponent<Image>();
        if (bg == null) bg = panel.AddComponent<Image>();
        bg.color = bgColor;

        // Title
        var titleGo = FindOrCreate(panel.transform, "TitleText");
        var trt     = titleGo.GetComponent<RectTransform>();
        if (trt == null) trt = titleGo.AddComponent<RectTransform>();
        trt.anchorMin        = new Vector2(0.5f, 0.5f);
        trt.anchorMax        = new Vector2(0.5f, 0.5f);
        trt.pivot            = new Vector2(0.5f, 0.5f);
        trt.anchoredPosition = new Vector2(0f, 100f);
        trt.sizeDelta        = new Vector2(700f, 120f);
        var title = titleGo.GetComponent<TextMeshProUGUI>();
        if (title == null) title = titleGo.AddComponent<TextMeshProUGUI>();
        title.text      = titleText;
        title.fontSize  = 80;
        title.fontStyle = FontStyles.Bold;
        title.color     = titleColor;
        title.alignment = TextAlignmentOptions.Center;

        // Restart button
        var btnGo = FindOrCreate(panel.transform, "RestartButton");
        var brt   = btnGo.GetComponent<RectTransform>();
        if (brt == null) brt = btnGo.AddComponent<RectTransform>();
        brt.anchorMin        = new Vector2(0.5f, 0.5f);
        brt.anchorMax        = new Vector2(0.5f, 0.5f);
        brt.pivot            = new Vector2(0.5f, 0.5f);
        brt.anchoredPosition = new Vector2(0f, -60f);
        brt.sizeDelta        = new Vector2(220f, 65f);
        if (btnGo.GetComponent<Button>() == null) btnGo.AddComponent<Button>();
        var btnBg = btnGo.GetComponent<Image>();
        if (btnBg == null) btnBg = btnGo.AddComponent<Image>();
        btnBg.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

        var btnLblGo = FindOrCreate(btnGo.transform, "Label");
        FillParent(btnLblGo);
        var btnLbl = btnLblGo.GetComponent<TextMeshProUGUI>();
        if (btnLbl == null) btnLbl = btnLblGo.AddComponent<TextMeshProUGUI>();
        btnLbl.text      = "Restart";
        btnLbl.fontSize  = 34;
        btnLbl.fontStyle = FontStyles.Bold;
        btnLbl.color     = Color.white;
        btnLbl.alignment = TextAlignmentOptions.Center;

        panel.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // EventSystem
    // -------------------------------------------------------------------------

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;

        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
        Debug.Log("[UIFactory] Created EventSystem.");
    }

    // -------------------------------------------------------------------------
    // RectTransform helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Finds or creates a child, ensures it has a RectTransform, and returns it.
    /// </summary>
    private static GameObject FindOrCreate(Transform parent, string name)
    {
        var existing = parent.Find(name);
        if (existing != null) return existing.gameObject;

        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    /// <summary>
    /// Sets anchor to top-left (0,1), pivot to centre, and positions the element
    /// at <paramref name="cx"/>, <paramref name="cy"/> (cy negative = below anchor).
    /// </summary>
    private static RectTransform SetRect(
        GameObject go, float cx, float cy, float w, float h)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0f, 1f);
        rt.anchorMax        = new Vector2(0f, 1f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(cx, cy);
        rt.sizeDelta        = new Vector2(w, h);
        return rt;
    }

    /// <summary>Stretches a child to fill its parent completely.</summary>
    private static void FillParent(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.one;
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.offsetMin        = Vector2.zero;
        rt.offsetMax        = Vector2.zero;
    }

    // -------------------------------------------------------------------------
    // Asset helpers
    // -------------------------------------------------------------------------

    private static Sprite LoadSprite(string path)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
            Debug.LogWarning($"[UIFactory] Sprite not found at {path}");
        return sprite;
    }
}
