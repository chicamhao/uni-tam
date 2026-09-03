namespace Assets.Scripts.Interaction.Interfaces
{
    public interface IInteractable
    {
        void Interact();
        void SetHighlight(bool highlighted);
        string GetPrompt();
    }
}