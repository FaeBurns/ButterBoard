using Toaster;

namespace ButterBoard.UI.Tooltips
{
    /// <summary>
    /// A container for information about a tooltip in a text editor.
    /// </summary>
    public struct Tooltip
    {
        public Tooltip(Error error)
        {
            line = error.Position.Line;
            startColumn = error.Position.StartColumn;
            length = error.Position.Length;

            // don't use the error's ToString as that causes extra information to end up in the message
            message = error.Message;
        }

        public Tooltip(int line, int startColumn, int length, string message)
        {
            this.line = line;
            this.startColumn = startColumn;
            this.length = length;
            this.message = message;
        }

        public readonly int line;
        public readonly int startColumn;
        public readonly int length;
        public readonly string message;
    }
}