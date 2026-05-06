using UnityEngine;

/// <summary>
/// Runtime debug overlay. Press backtick (`) to toggle.
///
/// All sizes are derived from Screen.height so the overlay is readable at any
/// window size. Styles are rebuilt automatically when the window is resized.
///
/// When visible:
///   ● Red box over every ZombieAgent   – name, row, HP
///   ● Green box over every Plant       – name, grid position, HP
///   ● Top-left HUD                     – GameManager state + control buttons
///   ● Top-left HUD (below)             – LaneRegistry per-row zombie counts
///   ● Bottom-right hint                – toggle key reminder
///
/// Remove the "Debug" scene root before shipping.
/// </summary>
[DefaultExecutionOrder(1000)]
public class DebugOverlay : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector
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

    // All styles are rebuilt whenever the screen height changes.
    private GUIStyle zombieBoxStyle;
    private GUIStyle plantBoxStyle;
    private GUIStyle hudBoxStyle;
    private GUIStyle labelStyle;
    private GUIStyle boldLabelStyle;
    private GUIStyle buttonStyle;
    private GUIStyle dangerButtonStyle;
    private int      cachedScreenHeight;

    // -------------------------------------------------------------------------
    // Scale helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Multiplier relative to a 720-pixel-tall reference window.
    /// At 720 p  → 1.0   (base sizes)
    /// At 1080 p → 1.5
    /// At 1440 p → 2.0
    /// </summary>
    private float Scale => Mathf.Max(0.5f, Screen.height / 720f);

    private float Px(float basePixels) => basePixels * Scale;
    private int   Fs(int  basePt)      => Mathf.Max(1, Mathf.RoundToInt(basePt * Scale));

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
        float bw = Px(140f); // box width
        float bh = Px(44f);  // box height

        // Red boxes – zombies
        foreach (var zombie in FindObjectsByType<ZombieAgent>(FindObjectsInactive.Exclude))
        {
            if (zombie == null) continue;
            string label = CleanName(zombie.name) + "\n" +
                           $"row:{zombie.Row}  HP:{zombie.CurrentHp}/{zombie.MaxHp}";
            DrawWorldBox(zombie.transform.position + Vector3.up * 0.7f,
                         label, zombieBoxStyle, bw, bh);
        }

        // Green boxes – plants
        foreach (var plant in FindObjectsByType<Plant>(FindObjectsInactive.Exclude))
        {
            if (plant == null) continue;
            string label = CleanName(plant.name) + "\n" +
                           $"({plant.row},{plant.column})  HP:{plant.CurrentHp}/{plant.MaxHp}";
            DrawWorldBox(plant.transform.position + Vector3.up * 0.7f,
                         label, plantBoxStyle, bw, bh);
        }
    }

    private void DrawWorldBox(Vector3 worldPos, string label, GUIStyle style, float w, float h)
    {
        Vector3 sp = cam.WorldToScreenPoint(worldPos);
        if (sp.z < 0f) return;

        float gx = sp.x               - w * 0.5f;
        float gy = Screen.height - sp.y - h * 0.5f;
        GUI.Box(new Rect(gx, gy, w, h), label, style);
    }

    // -------------------------------------------------------------------------
    // Corner HUD
    // -------------------------------------------------------------------------

    private void DrawHud()
    {
        float x  = Px(10f);
        float w  = Px(250f);
        float lh = Px(22f);   // line height
        float p  = Px(8f);    // inner padding
        float bh = Px(34f);   // button height
        float y  = Px(10f);

        // ── GameManager block ──────────────────────────────────────────────
        if (GameManager.Instance != null)
        {
            var gm = GameManager.Instance;

            float blockH = p + lh * 3f + p * 0.5f + bh + p;
            GUI.Box(new Rect(x, y, w, blockH), GUIContent.none, hudBoxStyle);

            GUI.Label(new Rect(x + p, y + p,              w - p * 2f, lh), $"State:    {gm.State}",         labelStyle);
            GUI.Label(new Rect(x + p, y + p + lh,         w - p * 2f, lh), $"Sun:      {gm.Sun}",            labelStyle);
            GUI.Label(new Rect(x + p, y + p + lh * 2f,    w - p * 2f, lh), $"Selected: {gm.SelectedPlant}", labelStyle);

            // ── Control buttons ────────────────────────────────────────────
            float btnY  = y + p + lh * 3f + p * 0.5f;
            float gap   = Px(6f);
            float btnW  = (w - p * 2f - gap) * 0.5f;

            // Left button: Pause / Resume / Play depending on current state
            string leftLabel;
            switch (gm.State)
            {
                case GameState.Playing:     leftLabel = "⏸  Pause";  break;
                case GameState.Paused:      leftLabel = "▶  Resume"; break;
                default:                    leftLabel = "▶  Play";   break;
            }

            if (GUI.Button(new Rect(x + p, btnY, btnW, bh), leftLabel, buttonStyle))
            {
                if      (gm.State == GameState.Playing) gm.Pause();
                else if (gm.State == GameState.Paused)  gm.Resume();
                else                                     gm.StartGame();
            }

            // Right button: Restart (always available)
            if (GUI.Button(new Rect(x + p + btnW + gap, btnY, btnW, bh), "↺  Restart", dangerButtonStyle))
                gm.RestartGame();

            y += blockH + Px(6f);
        }

        // ── LaneRegistry block ────────────────────────────────────────────
        if (LaneRegistry.Instance != null)
        {
            const int rows   = 5;
            int       total  = LaneRegistry.Instance.GetActiveZombieCount();
            float     blockH = p + lh * (rows + 1) + p;

            GUI.Box(new Rect(x, y, w, blockH), GUIContent.none, hudBoxStyle);
            GUI.Label(new Rect(x + p, y + p, w - p * 2f, lh),
                      $"LaneRegistry  (total: {total})", boldLabelStyle);

            for (int row = 0; row < rows; row++)
            {
                bool   hasZ    = LaneRegistry.Instance.HasZombiesInRow(row);
                string rowText = hasZ ? $"  row {row}:  ● zombie(s)"
                                      : $"  row {row}:  ○ clear";
                GUI.Label(new Rect(x + p, y + p + lh * (row + 1), w - p * 2f, lh),
                          rowText, labelStyle);
            }
        }
    }

    // -------------------------------------------------------------------------
    // Toggle hint
    // -------------------------------------------------------------------------

    private void DrawToggleHint()
    {
        float w  = Px(220f);
        float h  = Px(22f);
        float margin = Px(8f);
        GUI.Label(
            new Rect(Screen.width - w - margin, Screen.height - h - margin, w, h),
            "[ ` ]  toggle debug overlay",
            labelStyle);
    }

    // -------------------------------------------------------------------------
    // Styles – rebuilt whenever the window is resized
    // -------------------------------------------------------------------------

    private void EnsureStyles()
    {
        if (zombieBoxStyle != null && cachedScreenHeight == Screen.height) return;
        cachedScreenHeight = Screen.height;

        zombieBoxStyle    = BuildBoxStyle(new Color(0.75f, 0.10f, 0.10f, 0.85f), Color.white);
        plantBoxStyle     = BuildBoxStyle(new Color(0.10f, 0.60f, 0.10f, 0.85f), Color.white);

        hudBoxStyle = new GUIStyle
        {
            normal = { background = MakeTex(new Color(0f, 0f, 0f, 0.68f)) }
        };

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = Fs(14),
            normal   = { textColor = Color.white }
        };

        boldLabelStyle = new GUIStyle(labelStyle)
        {
            fontStyle = FontStyle.Bold
        };

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize  = Fs(14),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        buttonStyle.normal.textColor  = Color.white;
        buttonStyle.hover.textColor   = Color.white;
        buttonStyle.active.textColor  = Color.white;

        dangerButtonStyle = new GUIStyle(buttonStyle);
        dangerButtonStyle.normal.background = MakeTex(new Color(0.65f, 0.15f, 0.15f, 0.95f));
        dangerButtonStyle.hover.background  = MakeTex(new Color(0.80f, 0.20f, 0.20f, 0.95f));
    }

    private GUIStyle BuildBoxStyle(Color background, Color textColor)
    {
        var style = new GUIStyle(GUI.skin.box)
        {
            fontSize  = Fs(12),
            alignment = TextAnchor.MiddleCenter
        };
        style.normal.background = MakeTex(background);
        style.normal.textColor  = textColor;
        return style;
    }

    private static Texture2D MakeTex(Color color)
    {
        var tex = new Texture2D(2, 2);
        tex.SetPixels(new[] { color, color, color, color });
        tex.Apply();
        return tex;
    }

    private static string CleanName(string n) =>
        n.Replace("(Clone)", "").Trim();
}
