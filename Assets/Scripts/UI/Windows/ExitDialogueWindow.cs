using ButterBoard.UI.DraggableWindows;
using UnityEditor;
using UnityEngine;

namespace ButterBoard.UI.Windows
{
    public class ExitDialogueWindow : CreatableWindow<ExitDialogueWindow>
    {
        public void ButtonClose()
        {
            // stop playing
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void ButtonCancel()
        {
            // close window
            Close();
        }
    }
}