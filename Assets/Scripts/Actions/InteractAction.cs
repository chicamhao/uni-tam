namespace Actions
{
    /// <summary>
    /// Dispatches IInteractable.Interact() when the interact button is pressed.
    /// Follows the tamagooo convention of stateless action classes.
    /// </summary>
    public sealed class InteractAction
    {
        readonly ActionContext _context;

        public InteractAction(ActionContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns true when an interaction was performed this frame.
        /// </summary>
        public bool Interact()
        {
            if (!_context.Input.GetInteractInputDown())
                return false;

            if (_context.InteractObject == null)
                return false;

            _context.InteractObject.Interact();
            return true;
        }
    }
}