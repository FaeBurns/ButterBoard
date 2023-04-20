using System;
using ButterBoard.Simulation.Elements;
using ButterBoard.UI.Drag;
using UnityEngine;

namespace ButterBoard.UI.Processor
{
    public class ProcessorElementInterface : MonoBehaviour
    {
        private uint editorWindowHandle;
        private uint statusWindowHandle;

        [SerializeField]
        public ProcessorElement processor = null!;

        public void OnOpenEditorClick()
        {
            Console.WriteLine("opening processor editor");

            // if window is already open
            if (editorWindowHandle != 0)
            {
                DraggableWindow openWindow = DraggableWindowManager.Instance.TryGetWindowByHandle(editorWindowHandle)!;
                openWindow.SetTopmost();
                return;
            }

            DraggableWindow window = DraggableWindowManager.Instance.CreateWindow("ProcessorEditor");
            window.Closed += OnEditorWindowClosed;
            editorWindowHandle = window.Handle;

            ProcessorEditorWindow editorWindow = window.GetComponent<ProcessorEditorWindow>();
            editorWindow.SetProcessor(processor);
        }

        public void OpenErrorDisplay()
        {
            Console.WriteLine("opening processor error display");
        }

        private void OnEditorWindowClosed(object sender, EventArgs e)
        {
            editorWindowHandle = 0;
        }
    }
}