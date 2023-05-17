using System.Collections;
using System.Collections.Generic;

namespace ButterBoard.UI.Tooltips
{
    public interface ITooltipHost
    {
        public IEnumerable<Tooltip> GetTooltips();
    }
}