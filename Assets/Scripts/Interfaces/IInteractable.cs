using UnityEngine;

namespace Interfaces
{
    public interface IInteractable
    {
        void Interact();
        string GetPrompt();
        void SetHighlight(bool highlighted);
    }
}