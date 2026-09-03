using Assets.Scripts.Interaction.Interfaces;
using Assets.Scripts.Settings;

namespace Assets.Scripts.Interfaces
{
    public interface IProgression
    {
        void Init(ProgressionSettings settings);
        void RegisterPositionable(IPositionable obj);
        void ApplyChapter(int chapter);
    }
}