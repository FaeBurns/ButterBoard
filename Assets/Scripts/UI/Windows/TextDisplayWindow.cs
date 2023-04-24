using ButterBoard.UI.DraggableWindows;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace ButterBoard.UI.Windows
{
    [WindowKey("UI/Windows/TextDisplay")]
    public class TextDisplayWindow : CreatableWindow<TextDisplayWindow>
    {
        [SerializeField]
        private TextMeshProUGUI textDisplay = null!;

        public static TextDisplayWindow CreateWindow(string message, string title)
        {
            TextDisplayWindow window = CreateWindow();
            window.SetText(message);
            window.SetTitle(title);
            return window;
        }

        public void SetText(string text)
        {
            textDisplay.SetText(text);
        }
    }
}