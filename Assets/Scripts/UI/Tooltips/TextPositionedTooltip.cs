using Toaster;

namespace ButterBoard.UI.Tooltips
{
    public class TextPositionedTooltip
    {
        public Tooltip Tooltip { get; }
        public int Line { get; }
        public int StartColumn { get; }
        public int EndColumn { get; }
        public int Length { get; }

        public TextPositionedTooltip(Tooltip tooltip, Error error)
        {
            Tooltip = tooltip;
            Line = error.Position.Line;
            StartColumn = error.Position.StartColumn;
            EndColumn = error.Position.EndColumn;
            Length = error.Position.Length;
        }
    }
}