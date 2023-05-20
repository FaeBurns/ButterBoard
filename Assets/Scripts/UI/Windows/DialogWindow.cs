using System;
using ButterBoard.UI.DraggableWindows;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace ButterBoard.UI.Windows
{
    [WindowKey("UI/Windows/DialogWindow")]
    public class DialogWindow : CreatableWindow<DialogWindow>
    {
        [SerializeField]
        private TextMeshProUGUI dialogText = null!;

        [SerializeField]
        private TextMeshProUGUI trueButtonText = null!;

        [SerializeField]
        private TextMeshProUGUI falseButtonText = null!;

        private Action<bool> resultAction = null!;
        
        public void SetDialog(string text, string title, Action<bool> returnAction, string trueButtonTitle = "Yes", string falseButtonTitle = "No")
        {
            dialogText.SetText(text);
            SetTitle(title);
            
            resultAction = returnAction;
            
            trueButtonText.SetText(trueButtonTitle);
            falseButtonText.SetText(falseButtonTitle);
        }
        
        public void ButtonOkay()
        {
            // notify of okay result
            resultAction?.Invoke(true);
        }

        public void ButtonCancel()
        {
            // close window
            Close();
            
            // notify of cancel result
            resultAction?.Invoke(false);
        }
    }
}