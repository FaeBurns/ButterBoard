using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Toaster;
using Toaster.Execution;
using Toaster.Parsing;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ButterBoard.UI.Processor
{
    public class ProcessorTextHighlighter
    {
        private readonly ExecutionConfig _validationConfig;
        private readonly List<TextTransform> _transforms = new List<TextTransform>();
        private readonly List<Tooltip> _tooltips = new List<Tooltip>();
        private readonly Parser _parser;
        private readonly TokenProgram _tokenProgram;
        private readonly TokenProgram _tokenProgramWithoutComments;

        public ProcessorTextHighlighter(string program, ExecutionConfig validationConfig)
        {
            _validationConfig = validationConfig;

            _parser = new Parser();
            _tokenProgram = _parser.Tokenize(program, true);

            // don't need to save this parser as it should not have any errors not present in the version with comments
            Parser noCommentParser = new Parser();
            _tokenProgramWithoutComments = noCommentParser.Tokenize(program, false);
        }

        public void Parse()
        {
            foreach (TokenLine tokenLine in _tokenProgram.Lines)
            {
                foreach (Token token in tokenLine.Tokens)
                {
                    ParseToken(token);
                }
            }

            AddErrorCollection(_parser.Errors);

            TokenProgramValidator programValidator = new TokenProgramValidator();
            programValidator.Validate(_tokenProgramWithoutComments, _validationConfig);

            AddErrorCollection(programValidator.ErrorCollection);
        }

        private void ParseToken(Token token)
        {
            Color color;
            bool bold = false;
            bool italics = false;
            switch (token.Id)
            {
                case TokenId.LABEL:
                    color = TextHighlightSettings.Instance.labelColor;
                    break;
                case TokenId.INSTRUCTION:
                    color = TextHighlightSettings.Instance.instructionColor;
                    break;
                case TokenId.REGISTER:
                    color = TextHighlightSettings.Instance.registerColor;
                    break;
                case TokenId.LABEL_ARG:
                    color = TextHighlightSettings.Instance.labelColor;
                    italics = true;
                    break;
                case TokenId.PIN_RANGE:
                case TokenId.PIN_RANGE_LENGTH:
                case TokenId.PIN:
                    color = TextHighlightSettings.Instance.pinColor;
                    break;
                case TokenId.BINARY:
                case TokenId.HEX:
                case TokenId.INTEGER:
                    color = TextHighlightSettings.Instance.constantColor;
                    break;
                case TokenId.WHITESPACE:
                    return;
                case TokenId.COMMENT:
                    color = TextHighlightSettings.Instance.commentColour;
                    italics = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            TextTransform transform = new TextTransform(
                token.Position.Line,
                token.Position.StartColumn,
                token.Position.Length,
                color,
                bold,
                italics,
                false);
            _transforms.Add(transform);
        }

        private void AddErrorCollection(ErrorCollection errorCollection)
        {
            foreach (Error error in errorCollection.Errors)
            {
                TextTransform transform = new TextTransform(
                    error.Position.Line,
                    error.Position.StartColumn,
                    error.Position.Length,
                    TextHighlightSettings.Instance.errorColor,
                    false,
                    false,
                    true);
                _transforms.Add(transform);

                _tooltips.Add(new Tooltip(error));
            }
        }

        public string CompileText()
        {
            Stopwatch compileStopwatch = new Stopwatch();
            compileStopwatch.Start();


            StringBuilder result = new StringBuilder();

            int currentLine = 0;

            // sort transforms so they all occur in order of line and then column
            // is a stable sort so transforms provided by error detection will override those caused by highlighting
            List<TextTransform> allTransforms = _transforms.OrderBy(t => t.Line).ThenBy(t => t.StartColumn).ToList();

            Queue<TextTransform> lineTransforms = new Queue<TextTransform>();

            foreach (TextTransform transform in allTransforms)
            {
                // if transform is on a different line
                if (transform.Line > currentLine)
                {
                    // compile what's been collected of the previous line
                    CompileLine(_tokenProgram.FullProgramLines[currentLine], result, lineTransforms);

                    // add any missing newlines after that line
                    int difference = transform.Line - currentLine;
                    if (difference > 1)
                    {
                        for (int i = 0; i < difference - 1; i++)
                        {
                            result.AppendLine();
                        }
                    }

                    // and continue on with the start of this new line
                }

                // add the transform to the collection of transforms
                // also update the current line
                lineTransforms.Enqueue(transform);
                currentLine = transform.Line;
            }

            // if there are any transforms left
            // compile the line
            if (lineTransforms.Count > 0)
                CompileLine(_tokenProgram.FullProgramLines[currentLine], result, lineTransforms, true);

            compileStopwatch.Stop();
            Debug.Log($"Text Highlight Compile took {compileStopwatch.Elapsed.TotalMilliseconds} milliseconds");

            return result.ToString();
        }

        private void CompileLine(string line, StringBuilder builder, Queue<TextTransform> transforms, bool finalLine = false)
        {
            if (transforms.Count == 0)
            {
                builder.AppendLine(line);
                return;
            }

            int previousTransformEndColumn = 0;

            FinalizingTransformCollection waitingFinalizers = new FinalizingTransformCollection();

            while (transforms.Count > 0)
            {
                TextTransform transform = transforms.Dequeue();
                int currentColumn = transform.StartColumn;

                // catch up text to beginning of current transform
                int catchupLength = transform.StartColumn - previousTransformEndColumn;
                if (catchupLength > 0)
                    builder.Append(line.Substring(previousTransformEndColumn, catchupLength));

                // loop through all transforms waiting to finish
                foreach (TextTransform finalizingTransform in waitingFinalizers.GetTransformsInColumnRange(previousTransformEndColumn, (currentColumn - previousTransformEndColumn) + 1))
                {
                    // add the tag postfix
                    finalizingTransform.BuildTagsPostfix(builder);
                }

                // build prefix to current transform's tag
                // prefix must come after to avoid cases that would close tags immediately
                transform.BuildTagsPrefix(builder);

                // add current transform to finalizer collection at its end position
                waitingFinalizers.Add(currentColumn + transform.Length, transform);

                previousTransformEndColumn = currentColumn;
            }

            // start at next column instead of continuing the same one
            for (int i = previousTransformEndColumn + 1; i < waitingFinalizers.LastFinalizerColumn + 1; i++)
            {
                // if there are no transforms waiting to be finished on this column, continue to the next iteration
                if (!waitingFinalizers.HasTransformsInColumn(i))
                    continue;

                // perform catchup to text
                // limit catchup length to inside the text
                int catchupLength = i - previousTransformEndColumn;
                catchupLength = Mathf.Min(previousTransformEndColumn + catchupLength, line.Length) - previousTransformEndColumn;
                if (catchupLength > 0)
                    builder.Append(line.Substring(previousTransformEndColumn, catchupLength));

                // apply postfix to all finalizers on this column
                IEnumerable<TextTransform> finalizingTransforms = waitingFinalizers.GetTransformsForColumn(i);
                foreach (TextTransform transform in finalizingTransforms)
                {
                    transform.BuildTagsPostfix(builder);
                }

                previousTransformEndColumn = i;
            }

            // if there is any remaining text
            // perform a final catchup
            if (line.Length - previousTransformEndColumn > 0)
                builder.Append(line.Substring(previousTransformEndColumn, line.Length - previousTransformEndColumn));


            // add a newline, but only if this is not the last line
            if (!finalLine)
                builder.AppendLine();
        }

        public IEnumerable<Tooltip> GetTooltips()
        {
            return _tooltips;
        }

        /// <summary>
        /// Compiles the text for the line number field.
        /// </summary>
        /// <param name="currentLine">The line to display the current line indicator on, -1 if the indicator should not be displayed.</param>
        /// <returns>The compiled text.</returns>
        public string CompileLineNumberText(int currentLine = -1)
        {
            string lineNumberColorTag = "<color=#" + ColorUtility.ToHtmlStringRGB(TextHighlightSettings.Instance.lineNumberColor) + ">";
            string currentLineNumberColorTag = "<color=#" + ColorUtility.ToHtmlStringRGB(TextHighlightSettings.Instance.lineNumberColor) + ">";

            StringBuilder lineNumberBuilder = new StringBuilder();
            for (int i = 0; i < _tokenProgram.FullProgramLines.Length; i++)
            {
                // select color tag to use from current line index and i
                lineNumberBuilder.Append(i == currentLine ? currentLineNumberColorTag : lineNumberColorTag);

                // add number
                lineNumberBuilder.Append(i + 1);

                // close off color tag and end line
                lineNumberBuilder.AppendLine("</color>");
            }

            return lineNumberBuilder.ToString();
        }
    }

    /// <summary>
    /// A container for information about a tooltip in a text editor.
    /// </summary>
    public class Tooltip
    {
        public Tooltip(Error error)
        {
            Line = error.Position.Line;
            StartColumn = error.Position.StartColumn;
            Length = error.Position.Length;

            // don't use the error's ToString as that causes extra information to end up in the message
            Message = error.Message;
        }

        public Tooltip(int line, int startColumn, int length, string message)
        {
            Line = line;
            StartColumn = startColumn;
            Length = length;
            Message = message;
        }

        public int Line { get; }
        public int StartColumn { get; }
        public int Length { get; }
        public string Message { get; }
    }

    public class FinalizingTransformCollection
    {
        private readonly Dictionary<int, Stack<TextTransform>> _transforms;

        public int LastFinalizerColumn { get; private set; } = 0;

        public FinalizingTransformCollection()
        {
            _transforms = new Dictionary<int, Stack<TextTransform>>();
        }

        public void Add(int column, TextTransform transform)
        {
            // if key is not found, add to
            if (!_transforms.ContainsKey(column))
                _transforms.Add(column, new Stack<TextTransform>(1));

            _transforms[column].Push(transform);

            int finalizerPosition = transform.StartColumn + transform.Length;
            if (finalizerPosition > LastFinalizerColumn)
                LastFinalizerColumn = finalizerPosition;
        }

        public bool HasTransformsInColumn(int column)
        {
            return _transforms.ContainsKey(column);
        }

        public IEnumerable<TextTransform> GetTransformsForColumn(int column)
        {
            if (!_transforms.ContainsKey(column))
                return Array.Empty<TextTransform>();

            return _transforms[column];
        }

        public IEnumerable<TextTransform> GetTransformsInColumnRange(int column, int length)
        {
            List<TextTransform> result = new List<TextTransform>();

            for (int i = 0; i < length; i++)
            {
                result.AddRange(GetTransformsForColumn(i + column));
            }

            return result;
        }
    }
}