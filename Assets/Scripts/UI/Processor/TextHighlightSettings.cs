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
        public Color errorColor = Color.white;
        public Color instructionColor = Color.white;
        public Color registerColor = Color.white;
        public Color labelColor = Color.white;
        public Color pinColor = Color.white;
        public Color constantColor = Color.white;
        public float indentUnits = 15;
    }
}