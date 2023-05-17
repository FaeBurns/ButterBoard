using Toaster;

namespace ButterBoard.UI.Tooltips
{
    /// <summary>
    /// A container for information about a tooltip.
    /// </summary>
    public struct Tooltip
    {
        public Tooltip(string message)
        {
            this.message = message;
        }
        
        public readonly string message;
    }
}