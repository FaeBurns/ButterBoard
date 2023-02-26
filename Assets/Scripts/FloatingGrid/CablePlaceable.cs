using ButterBoard.Cables;
using ButterBoard.FloatingGrid.Placement;
using ButterBoard.FloatingGrid.Placement.Services;
using UnityEngine;

#nullable disable

namespace ButterBoard.FloatingGrid
{
    public class CablePlaceable : BasePlaceable
    {
        [field: SerializeField]
        public CablePlaceable Other { get; set; }

        [field: SerializeField]
        public GridPin Pin { get; private set; }

        [field: SerializeField]
        public CablePlacementType PlacementType { get; set; }

        [field: SerializeField]
        public CableDisplay LineDisplay { get; set; }

        public override void DisplayPlacementStatus(string statusMessage, bool isOk)
        {
            Color tintColor = isOk ? Color.green : Color.red;
            Pin.PinSpriteTintHelper.SetTint(tintColor);
        }

        public override void ClearPlacementStatus()
        {
            Pin.PinSpriteTintHelper.RestoreColor();
        }
    }
}