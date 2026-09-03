using Assets.Scripts.Puzzle;
using Manager.Scene;
using Utility;

namespace Core
{
    /// <summary>
    /// Eagerly registers all plain-C# singletons before any scene objects Awake,
    /// preventing null-reference access when MonoBehaviour.Start runs before a
    /// scene-based driver gets a chance to register dependencies.
    ///
    /// Singletons that need scene references (UI panels, cameras, spawn points)
    /// receive those later via their <c>Init(...)</c> method, called by GameDriver.
    /// </summary>
    public static class Bootstrapper
    {
        [UnityEngine.RuntimeInitializeOnLoadMethod(
            UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // Reset tracking in case of a domain-reload / play-mode restart.
            DIContainer.ResetTracking();

            // ── Non-MonoBehaviour singletons ──────────────────────────────────

            DIContainer.Inject(new Dialogue());

            // ── Former-MonoBehaviour singletons (now plain C#) ────────────────
            // Their Init(...) will be called by GameDriver once the scene is loaded.

            DIContainer.Inject(new ProgressionManager());
            DIContainer.Inject(new PlayerState());
            DIContainer.Inject(new UIManager());
            DIContainer.Inject(new GameplayScene());
            DIContainer.Inject(new Puzzle());
        }
    }
}