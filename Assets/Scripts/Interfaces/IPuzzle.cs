using System;

namespace Assets.Scripts.Interfaces
{
    public interface IPuzzle
    {
        event Action OnPuzzleStarted;
        event Action OnPuzzleExited;
        string ClickActionName { get; set; }
        string ExitActionName { get; set; }
        void Init(UnityEngine.Camera puzzleCamera);
        void EnableInputActions();
        void DisableInputActions();
        void Tick();
        void SetActive(bool active);
    }
}