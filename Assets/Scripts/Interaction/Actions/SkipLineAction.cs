using Assets.Scripts.Interfaces;
using Assets.Scripts.Context;

namespace Assets.Scripts.Interaction.Actions
{
    /// <summary>
    /// Skips the current dialogue line when the skip input is pressed.
    /// Receives IDialogue via constructor injection (no static access).
    /// </summary>
    public sealed class SkipLineAction
    {
        private readonly ActionContext _context;
        private readonly IDialogue _dialogue;

        public SkipLineAction(ActionContext context, IDialogue dialogue)
        {
            _context = context;
            _dialogue = dialogue;
        }

        public void Skip()
        {
            if (_context.Input.GetSkipInputDown())
            {
                _dialogue.RequestSkip();
            }
        }
    }
}