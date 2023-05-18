using System;
using System.Collections.Generic;
using UnityEngine;

namespace ButterBoard.UI.Tooltips
{
    public class ConstantTooltipHost : MonoBehaviour, ITooltipHost
    {
        [SerializeField]
        public string tooltip = String.Empty;

        public IEnumerable<Tooltip> GetTooltips()
        {
            // skip if tooltip is null or empty
            if (string.IsNullOrEmpty(tooltip))
                yield break;

            yield return new Tooltip(tooltip);
        }
    }
}