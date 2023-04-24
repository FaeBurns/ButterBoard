using System;
using ButterBoard.Simulation.Elements;
using ButterBoard.UI.DraggableWindows;
using UnityEngine;

namespace ButterBoard.UI.Processor
{
    public class ProcessorElementInterface : MonoBehaviour
    {
        private ProcessorEditorWindow? _editorWindow;

        [SerializeField]
        public ProcessorElement processor = null!;

        public void OnOpenEditorClick()
        {
            Console.WriteLine("opening processor editor");

            // check to see if window is already open
            // if so then bring to front and return
            if (_editorWindow != null)
            {
                _editorWindow.BringToFront();
                return;
            }

            // otherwise create a new window
            _editorWindow = ProcessorEditorWindow.CreateWindow();
            _editorWindow.SetProcessor(processor);
        }

        public void OpenErrorDisplay()
        {
            Console.WriteLine("opening processor error display");
        }
    }
}