namespace Actions
{
    public sealed class SkipLineAction
    {
        readonly ActionContext _context;

        public SkipLineAction(ActionContext context)
        {
            _context = context;
        }

        public void Skip()
        { 
            if (_context.Input.GetSkipInputDown())
            {
                Dialogue.Instance.RequestSkip();
            }
        }
    }
}