namespace ButterBoard.UI.Drag
{
    public class OpenWindowContainer
    {
        public string WindowKey { get; }
        public DraggableWindow Window { get; }

        public OpenWindowContainer(string windowKey, DraggableWindow window)
        {
            WindowKey = windowKey;
            Window = window;
        }
    }
}