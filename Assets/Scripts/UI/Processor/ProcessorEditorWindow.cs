using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ButterBoard.Simulation.Elements;
using ButterBoard.UI.DraggableWindows;
using ButterBoard.UI.Windows;
using TMPro;
using Toaster;
using Toaster.Parsing;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

namespace ButterBoard.UI.Processor
{
    [WindowKey("UI/Windows/ProcessorEditor")]
    public class ProcessorEditorWindow : CreatableWindow<ProcessorEditorWindow>
    {
        private readonly SortedTooltipCollection _tooltipCollection = new SortedTooltipCollection();
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

        [SerializeField]
        private TextMeshProUGUI lineDisplayField = null!;

        private void Awake()
        {
            programInputField.onValueChanged.AddListener(OnInputChanged);
        }

        private void Update()
        {
            CreateLineText();

            Vector3 mousePosition = Input.mousePosition;

            // should probably check if mouse is inside window rect first
            // should probably also not do this every frame

            // check against programInputField.textComponent rather than programDisplayField to make sure it does not include rtf tags

            // if camera is provided it will search for world text
            int intersectingLine = TMP_TextUtilities.FindIntersectingLine(programInputField.textComponent, mousePosition, null!);
            if (intersectingLine == -1)
                return;

            int intersectingCharacter = TMP_TextUtilities.FindIntersectingCharacter(programInputField.textComponent, mousePosition, null!, true);
            if (intersectingCharacter == -1)
                return;

            Tooltip[] tooltips = _tooltipCollection.FindTooltipsUnderCursor(intersectingLine, intersectingCharacter).ToArray();

            // if no tooltips were found, remove any that may be currently shown and exit
            if (tooltips.Length == 0)
            {
                TooltipManager.Instance.ClearActiveTooltip();
                return;
            }

            StringBuilder tooltipTextBuilder = new StringBuilder();
            int index = 0;
            foreach (Tooltip tooltip in tooltips)
            {
                if (index > 0)
                    tooltipTextBuilder.AppendLine();
                tooltipTextBuilder.Append(tooltip.Message);
                index++;
            }

            // compile tooltip
            string tooltipMessage = tooltipTextBuilder.ToString();

            // skip setting tooltip if it has not changed
            if (tooltipMessage == TooltipManager.Instance.ActiveTooltipText)
                return;

            TooltipManager.Instance.SetActiveTooltip(tooltipMessage);
        }

        public void SetProcessor(ProcessorElement processorElement)
        {
            _processorElement = processorElement;
            programInputField.text = _processorElement.UnvalidatedProgramText;
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
            _tooltipCollection.ClearTooltips();
            _tooltipCollection.AddTooltips(highlighter.GetTooltips());

            CreateLineText();
        }

        public void TryPushProgramToProcessor()
        {
            ErrorCollection compilationErrors = _processorElement.TrySetProgram(programInputField.text);

            if (compilationErrors.HasErrors)
            {
                // would be ideal if showing this stopped editor window from being interactable
                TextDisplayWindow.CreateWindow(compilationErrors.ToString(), "Error compiling program!");
            }

            UpdateWriteState();
        }

        public void StopExecution()
        {
            _processorElement.Stop();
            UpdateWriteState();
        }

        public void OnScroll(float value)
        {
            lineDisplayFieldTransform.offsetMin = hiddenTextTransform.offsetMin;
            lineDisplayFieldTransform.offsetMax = hiddenTextTransform.offsetMax;

            programDisplayFieldTransform.offsetMin = hiddenTextTransform.offsetMin;
            programDisplayFieldTransform.offsetMax = hiddenTextTransform.offsetMax;
        }

        private void CreateLineText()
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
            lineDisplayField.SetText(lineNumberBuilder.ToString());
        }

        /// <summary>
        /// Updates the input text field to allow or disallow edits based on whether the program is running or not.
        /// </summary>
        private void UpdateWriteState()
        {
            programInputField.readOnly = _processorElement.IsActive;
        }
    }
}