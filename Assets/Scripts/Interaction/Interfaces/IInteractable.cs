namespace Assets.Scripts.Interaction.Interfaces
{
    /// <summary>Defines an object the player can interact with (talk, use, etc.).</summary>
    public interface IInteractable
    {
        void Interact();
        void SetHighlight(bool highlighted);
        string GetPrompt();
    }
}