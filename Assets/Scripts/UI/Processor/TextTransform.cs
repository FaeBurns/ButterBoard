using System.Text;
using UnityEngine;

namespace ButterBoard.UI.Processor
{
    public class TextTransform
    {
        public int Line { get; }
        public int StartColumn { get; }
        public int Length { get; }

        public bool IsUnderline { get; }

        private readonly Color _color;
        private readonly bool _bold;
        private readonly bool _italics;

        public TextTransform(int line, int startColumn, int length, Color color, bool bold, bool italics, bool isUnderline)
        {
            Line = line;
            StartColumn = startColumn;
            Length = length;
            IsUnderline = isUnderline;
            _color = color;
            _bold = bold;
            _italics = italics;
        }

        public void BuildTagsPrefix(StringBuilder builder)
        {
            if (_bold)
                builder.Append("<b>");
            if (_italics)
                builder.Append("<i>");

            BuildColour(builder, _color);
        }

        public void BuildTagsPostfix(StringBuilder builder)
        {
            builder.Append(IsUnderline ? "</u>" : "</color>");

            if (_italics)
                builder.Append("</i>");
            if (_bold)
                builder.Append("</b>");
        }

        private void BuildColour(StringBuilder builder, Color color)
        {
            builder.Append("<");

            if (IsUnderline)
                builder.Append("u ");

            builder.Append("color=");
            builder.Append("#");
            builder.Append(ColorUtility.ToHtmlStringRGB(color));
            builder.Append(">");
        }
    }
}