/// <summary>
/// All possible match states owned by GameManager.
///
/// Transition rules:
///   NotStarted → Playing          (GameManager.StartGame)
///   Playing    → Paused           (GameManager.Pause)
///   Paused     → Playing          (GameManager.Resume)
///   Playing    → Won              (GameManager.NotifyWin)
///   Playing    → Lost             (GameManager.NotifyLose)
///   Any        → NotStarted       (GameManager.RestartGame reloads the scene)
/// </summary>
public enum GameState
{
    NotStarted,
    Playing,
    Paused,
    Won,
    Lost
}
