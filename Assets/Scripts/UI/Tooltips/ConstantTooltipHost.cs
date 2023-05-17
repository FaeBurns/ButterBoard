using System;
using System.Collections.Generic;
using UnityEngine;

namespace ButterBoard.UI.Tooltips
{
    public class ConstantTooltipHost : MonoBehaviour, ITooltipHost
    {
        [SerializeField]
        private string tooltip = String.Empty;
        
        public IEnumerable<Tooltip> GetTooltips()
        {
            yield return new Tooltip(tooltip);
        }
    }
}