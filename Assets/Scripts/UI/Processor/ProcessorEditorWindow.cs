using System;
using System.Linq;
using System.Text;
using ButterBoard.Simulation.Elements;
using ButterBoard.UI.DraggableWindows;
using ButterBoard.UI.Tooltips;
using TMPro;
using Toaster;
using UnityEngine;
using UnityEngine.Serialization;

namespace ButterBoard.UI.Processor
{
    [WindowKey("UI/Windows/ProcessorEditor")]
    public class ProcessorEditorWindow : CreatableWindow<ProcessorEditorWindow>
    {
        private ProcessorElement _processorElement = null!;

        [SerializeField]
        private RectTransform hiddenTextTransform = null!;

        [SerializeField]
        private RectTransform programDisplayFieldTransform = null!;

        [SerializeField]
        private RectTransform lineDisplayFieldTransform = null!;

        [SerializeField]
        private TMP_InputField programInputField = null!;

        [SerializeField]
        private TextMeshProUGUI programDisplayField = null!;

        [FormerlySerializedAs("lineDisplayField")]
        [SerializeField]
        private TextMeshProUGUI lineNumberDisplayField = null!;

        [SerializeField]
        private TextMeshProUGUI statusDisplayField = null!;

        [SerializeField]
        private TextTooltipHost textTooltipHost = null!;

        private void Awake()
        {
            programInputField.onValueChanged.AddListener(OnInputChanged);
        }

        private void Update()
        {
            lineDisplayFieldTransform.offsetMin = hiddenTextTransform.offsetMin;
            lineDisplayFieldTransform.offsetMax = hiddenTextTransform.offsetMax;

            programDisplayFieldTransform.offsetMin = hiddenTextTransform.offsetMin;
            programDisplayFieldTransform.offsetMax = hiddenTextTransform.offsetMax;
            
            CreateLineNumberText();
        }

        public void SetProcessor(ProcessorElement processorElement)
        {
            _processorElement = processorElement;
            programInputField.text = _processorElement.UnvalidatedProgramText;

            SetStatusDisplay(processorElement.IsActive ? "Processor Running" : "Processor Stopped");
            UpdateWriteState();
        }

        public void OnInputChanged(string text)
        {
            HighlightText(text);
            _processorElement.UnvalidatedProgramText = text;
        }

        private void HighlightText(string inputText)
        {
            ProcessorTextHighlighter highlighter = new ProcessorTextHighlighter(inputText, _processorElement.ExecutionConfig);
            highlighter.Parse();

            string modifiedText = highlighter.CompileText();
            programDisplayField.SetText(modifiedText);

            // reset and record tooltips
            textTooltipHost.TooltipCollection.Clear();
            textTooltipHost.TooltipCollection.AddRange(highlighter.GetTooltips());

            CreateLineNumberText();
        }

        public void TryPushProgramToProcessor()
        {
            // do nothing if input is empty text
            if (programInputField.text == "")
                return;

            ErrorCollection compilationErrors = _processorElement.TrySetProgram(programInputField.text);

            if (compilationErrors.HasErrors)
            {
                // would be ideal if showing this stopped editor window from being interactable
                // disabling for now to test using status text instead
                // TextDisplayWindow.CreateWindow(compilationErrors.ToString(), "Error compiling program!");

                UpdateWriteState();
                SetStatusDisplay($"{compilationErrors.Errors.Count} {(compilationErrors.Errors.Count > 1 ? "Errors" : "Error")} found during compilation.");
                return;
            }

            UpdateWriteState();
            SetStatusDisplay("Program compiled successfully.");
        }

        public void StopExecution()
        {
            _processorElement.Stop();
            UpdateWriteState();
            SetStatusDisplay("Processor Stopped.");
        }

        private void CreateLineNumberText()
        {
            // get amount of lines in program
            int lineCount = programInputField.text.Split(new string[2]{"\n", "\r\n"}, StringSplitOptions.None).Length;

            // get colour tags for highlighted and non-highlighted lines
            string lineNumberColorTag = "<color=#" + ColorUtility.ToHtmlStringRGB(TextHighlightSettings.Instance.lineNumberColor) + ">";
            string currentLineNumberColorTag = "<color=#" + ColorUtility.ToHtmlStringRGB(TextHighlightSettings.Instance.currentLineNumberColor) + ">";

            // get index of currently executing line
            int highlightedLine = 0;
            if (_processorElement.IsActive)
                highlightedLine = _processorElement.ActiveInterpreter!.CurrentLineIndex;

            StringBuilder lineNumberBuilder = new StringBuilder();
            for (int i = 0; i < lineCount; i++)
            {
                // select color tag to use from current line index and i
                lineNumberBuilder.Append(i == highlightedLine ? currentLineNumberColorTag : lineNumberColorTag);

                // add spacing or current line indicator
                lineNumberBuilder.Append(i == highlightedLine ? ">" : " ");

                // add number
                lineNumberBuilder.Append(i + 1);

                // close off color tag and end line
                lineNumberBuilder.AppendLine("</color>");
            }

            // set line display text
            lineNumberDisplayField.SetText(lineNumberBuilder.ToString());
        }

        /// <summary>
        /// Updates the input text field to allow or disallow edits based on whether the program is running or not.
        /// </summary>
        private void UpdateWriteState()
        {
            programInputField.readOnly = _processorElement.IsActive;
        }

        /// <summary>
        /// Updates the status display with the provided message.
        /// </summary>
        /// <param name="message"></param>
        private void SetStatusDisplay(string message)
        {
            statusDisplayField.SetText(message);
        }
    }
}