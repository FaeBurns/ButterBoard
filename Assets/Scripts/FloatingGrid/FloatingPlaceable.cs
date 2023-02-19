using UnityEngine;

namespace ButterBoard.FloatingGrid
{
    public class FloatingPlaceable : BasePlaceable
    {
        public override void SetPlaceStatus(Color statusColor)
        {
            Debug.Log(statusColor);
        }
    }
}