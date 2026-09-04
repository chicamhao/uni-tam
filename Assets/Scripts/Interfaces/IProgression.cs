using Assets.Scripts.Interaction.Interfaces;
using Assets.Scripts.Settings;

namespace Assets.Scripts.Interfaces
{
    /// <summary>Defines chapter progression — initialization, positionable registration, and chapter application.</summary>
    public interface IProgression
    {
        void Init(ProgressionSettings settings);
        void RegisterPositionable(IPositionable obj);
        void ApplyChapter(int chapter);
    }
}