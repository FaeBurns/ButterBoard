using TMPro;
using UnityEngine;

namespace ButterBoard.UI.Tooltips
{
    public class TextTooltipHost : MonoBehaviour
    {
        public TooltipCollection TooltipCollection { get; } = new TooltipCollection();

        [field: SerializeField]
        public TextMeshProUGUI TextComponent { get; private set; } = null!;
    }
}