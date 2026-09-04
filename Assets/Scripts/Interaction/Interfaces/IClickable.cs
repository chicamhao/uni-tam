namespace Assets.Scripts.Interaction.Interfaces
{
    /// <summary>Defines an object that can be clicked (selection-based interaction).</summary>
    public interface IClickable
    {
        void OnClick();
        void SetSelected(bool selected);
    }
}