using UnityEngine;
using UnityEngine.Serialization;

namespace ButterBoard.UI.Processor
{
    /// <summary>
    /// A component containing all colours to be used by the <see cref="ProcessorTextHighlighter"/>. Stored in a component to allow for editing in the inspector.
    /// </summary>
    public class TextHighlightSettings : SingletonBehaviour<TextHighlightSettings>
    {
        public Color commentColour = Color.white;
        public Color errorColor = Color.red;
        public Color instructionColor = Color.cyan;
        public Color registerColor = Color.magenta;
        public Color labelColor = Color.yellow;
        public Color pinColor = Color.blue;
        public Color constantColor = Color.green;
        public float indentUnits = 15;
    }
}