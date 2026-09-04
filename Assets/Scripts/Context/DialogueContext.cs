using Assets.Scripts.Characters;
using Assets.Scripts.Settings;

namespace Assets.Scripts.Context
{
    /// <summary>
    /// Read-only dialogue state shared between IDialogue and consumers.
    /// Mirrors the ActionContext pattern — a single reference object
    /// that consumers observe rather than pulling individual properties.
    /// </summary>
    public sealed class DialogueContext
    {
        public bool IsDialogueActive { get; set; }
        public Actor CurrentActor { get; set; }
        public DialogueEntry CurrentEntry { get; set; }
        public int CurrentLineIndex { get; set; }
    }
}