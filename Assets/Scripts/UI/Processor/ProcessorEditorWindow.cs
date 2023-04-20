using System;
using ButterBoard.Simulation.Elements;
using ButterBoard.UI.Drag;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace ButterBoard.UI.Processor
{
    public class ProcessorEditorWindow : MonoBehaviour
    {
        private ProcessorElement _processorElement = null!;

        [SerializeField]
        private TMP_InputField programInputField = null!;

        [SerializeField]
        private TextMeshProUGUI programDisplayField = null!;

        private void Awake()
        {
            programInputField.onValueChanged.AddListener(OnInputChanged);
        }

        public void SetProcessor(ProcessorElement processorElement)
        {
            _processorElement = processorElement;
            HighlightText(processorElement.UnvalidatedProgramText);
        }

        public void OnInputChanged(string text)
        {
            HighlightText(text);
        }

        private void HighlightText(string inputText)
        {
            ProcessorTextHighlighter highlighter = new ProcessorTextHighlighter(inputText, _processorElement.ExecutionConfig);
            highlighter.Parse();

            string modifiedText = highlighter.CompileText();
            programDisplayField.SetText(modifiedText);
        }
    }
}