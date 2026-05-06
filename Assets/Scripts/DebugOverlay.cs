using UnityEngine;

/// <summary>
/// Runtime debug overlay. Press backtick (`) to toggle.
///
/// When visible:
///   - Red labeled box over every ZombieAgent showing name, row, and HP.
///   - Green labeled box over every Plant showing name, grid position, and HP.
///   - Corner HUD showing GameManager state (sun, game state, selection)
///     and LaneRegistry per-row zombie counts.
///   - Bottom-right hint reminding the player how to toggle.
///
/// This component is created by SceneArtSetup on a "Debug" scene root.
/// Remove the "Debug" game object (or disable this component) before shipping.
/// </summary>
[DefaultExecutionOrder(1000)] // Run after all agent Update calls
public class DebugOverlay : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector fields
    // -------------------------------------------------------------------------

    [Tooltip("Key that toggles the overlay while the game is running.")]
    [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote;

    [Tooltip("Show the overlay immediately when Play mode starts.")]
    [SerializeField] private bool showOnStart = true;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private bool   visible;
    private Camera cam;

    // GUI style objects – built once on first OnGUI call.
    private GUIStyle zombieBoxStyle;
    private GUIStyle plantBoxStyle;
    private GUIStyle hudBoxStyle;
    private GUIStyle labelStyle;
    private GUIStyle boldLabelStyle;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void Start()
    {
        visible = showOnStart;
        cam     = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            visible = !visible;
    }

    private void OnGUI()
    {
        if (!visible || cam == null) return;

        EnsureStyles();

        DrawWorldLabels();
        DrawHud();
        DrawToggleHint();
    }

    // -------------------------------------------------------------------------
    // World-space labeled boxes
    // -------------------------------------------------------------------------

    private void DrawWorldLabels()
    {
        // --- Zombies (red boxes) ---
        foreach (var zombie in FindObjectsByType<ZombieAgent>(FindObjectsInactive.Exclude))
        {
            if (zombie == null) continue;

            string label = CleanName(zombie.name) + "\n" +
                           $"row:{zombie.Row}  HP:{zombie.CurrentHp}/{zombie.MaxHp}";

            DrawWorldBox(zombie.transform.position + Vector3.up * 0.6f,
                         label, zombieBoxStyle, 110f, 36f);
        }

        // --- Plants (green boxes) ---
        foreach (var plant in FindObjectsByType<Plant>(FindObjectsInactive.Exclude))
        {
            if (plant == null) continue;

            string label = CleanName(plant.name) + "\n" +
                           $"({plant.row},{plant.column})  HP:{plant.CurrentHp}/{plant.MaxHp}";

            DrawWorldBox(plant.transform.position + Vector3.up * 0.6f,
                         label, plantBoxStyle, 110f, 36f);
        }
    }

    /// <summary>
    /// Converts a world position to GUI space and draws a labelled box centred on it.
    /// </summary>
    private void DrawWorldBox(Vector3 worldPos, string label, GUIStyle style, float w, float h)
    {
        Vector3 sp = cam.WorldToScreenPoint(worldPos);
        if (sp.z < 0f) return; // Behind camera – skip

        float gx = sp.x      - w * 0.5f;
        float gy = Screen.height - sp.y - h * 0.5f;

        GUI.Box(new Rect(gx, gy, w, h), label, style);
    }

    // -------------------------------------------------------------------------
    // Corner HUD
    // -------------------------------------------------------------------------

    private void DrawHud()
    {
        const float x  = 10f;
        const float w  = 215f;
        const float lh = 18f; // line height
        float       y  = 10f;

        // --- GameManager block ---
        if (GameManager.Instance != null)
        {
            var gm      = GameManager.Instance;
            float blockH = lh * 3f + 16f;

            GUI.Box(new Rect(x, y, w, blockH), GUIContent.none, hudBoxStyle);
            GUI.Label(new Rect(x + 7, y + 6,             w - 14f, lh), $"State:    {gm.State}",         labelStyle);
            GUI.Label(new Rect(x + 7, y + 6 + lh,        w - 14f, lh), $"Sun:      {gm.Sun}",            labelStyle);
            GUI.Label(new Rect(x + 7, y + 6 + lh * 2f,   w - 14f, lh), $"Selected: {gm.SelectedPlant}", labelStyle);

            y += blockH + 6f;
        }

        // --- LaneRegistry block ---
        if (LaneRegistry.Instance != null)
        {
            const int rows    = 5;
            float     blockH  = lh * (rows + 1) + 16f;
            int       total   = LaneRegistry.Instance.GetActiveZombieCount();

            GUI.Box(new Rect(x, y, w, blockH), GUIContent.none, hudBoxStyle);
            GUI.Label(new Rect(x + 7, y + 6, w - 14f, lh),
                $"LaneRegistry  (total: {total})", boldLabelStyle);

            for (int row = 0; row < rows; row++)
            {
                bool hasZ = LaneRegistry.Instance.HasZombiesInRow(row);
                string rowText = hasZ
                    ? $"  row {row}:  ● zombie(s)"
                    : $"  row {row}:  ○ clear";

                GUI.Label(new Rect(x + 7, y + 6 + lh * (row + 1), w - 14f, lh),
                          rowText, labelStyle);
            }
        }
    }

    // -------------------------------------------------------------------------
    // Toggle hint (bottom-right)
    // -------------------------------------------------------------------------

    private void DrawToggleHint()
    {
        const float w = 185f;
        const float h = 20f;
        GUI.Label(
            new Rect(Screen.width - w - 8f, Screen.height - h - 6f, w, h),
            "[ ` ] toggle debug overlay",
            labelStyle);
    }

    // -------------------------------------------------------------------------
    // Style helpers – built once, cached thereafter
    // -------------------------------------------------------------------------

    private void EnsureStyles()
    {
        if (zombieBoxStyle != null) return;

        zombieBoxStyle = BuildBoxStyle(new Color(0.75f, 0.1f, 0.1f, 0.80f), Color.white);
        plantBoxStyle  = BuildBoxStyle(new Color(0.10f, 0.60f, 0.1f, 0.80f), Color.white);

        hudBoxStyle = new GUIStyle();
        hudBoxStyle.normal.background = MakeTex(new Color(0f, 0f, 0f, 0.65f));

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            normal   = { textColor = Color.white }
        };

        boldLabelStyle = new GUIStyle(labelStyle)
        {
            fontStyle = FontStyle.Bold
        };
    }

    private static GUIStyle BuildBoxStyle(Color background, Color textColor)
    {
        var style = new GUIStyle(GUI.skin.box)
        {
            fontSize  = 10,
            alignment = TextAnchor.MiddleCenter
        };
        style.normal.background  = MakeTex(background);
        style.normal.textColor   = textColor;
        return style;
    }

    private static Texture2D MakeTex(Color color)
    {
        var tex = new Texture2D(2, 2);
        tex.SetPixels(new[] { color, color, color, color });
        tex.Apply();
        return tex;
    }

    // -------------------------------------------------------------------------
    // Utilities
    // -------------------------------------------------------------------------

    private static string CleanName(string n) =>
        n.Replace("(Clone)", "").Trim();
}
