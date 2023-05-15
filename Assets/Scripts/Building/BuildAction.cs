#nullable disable

namespace ButterBoard.Building
{
    /// <summary>
    /// An action that can be done and undone.
    /// </summary>
    public abstract class BuildAction
    {
        /// <summary>
        /// Performs this action.
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// Undoes this action.
        /// </summary>
        public abstract void UndoExecute();
    }
}