using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ButterBoard.UI.Tooltips
{
    public class TextTooltipHost : MonoBehaviour, ITooltipHost
    {
        public TooltipCollection TooltipCollection { get; } = new TooltipCollection();

        [field: SerializeField]
        public TextMeshProUGUI TextComponent { get; private set; } = null!;

        public IEnumerable<Tooltip> GetTooltips()
        {
            Vector3 mousePosition = Input.mousePosition;

            // if camera is provided it will search for world text
            int intersectingLine = TMP_TextUtilities.FindIntersectingLine(TextComponent, mousePosition, null!);
            if (intersectingLine == -1)
                return new List<Tooltip>();

            // if camera is provided it will search for world text
            int intersectingCharacter = TMP_TextUtilities.FindIntersectingCharacter(TextComponent, mousePosition, null!, true);
            if (intersectingCharacter == -1)
                return new List<Tooltip>();

            int lineRelativeCharacter = GetLineRelativeCharacterIndex(TextComponent.text, intersectingLine, intersectingCharacter);

            return TooltipCollection.FindTooltipsAtPosition(intersectingLine, lineRelativeCharacter);
        }

        private int GetLineRelativeCharacterIndex(string text, int line, int globalCharacterIndex)
        {
            // TMP input field always uses \n
            string[] lines = text.Split('\n');

            if (line >= lines.Length)
                throw new ArgumentException("Line index is out of bounds of the text lines");

            int accumulatedCharacters = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string lineText = lines[i];
                if (i == line)
                {
                    return globalCharacterIndex - accumulatedCharacters;
                }

                // add one as \n is two characters
                accumulatedCharacters += lineText.Length + 1;
            }

            throw new Exception("Unreachable");
        }
    }
}