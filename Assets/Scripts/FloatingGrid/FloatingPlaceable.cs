using UnityEngine;

namespace ButterBoard.FloatingGrid
{
    public class FloatingPlaceable : BasePlaceable
    {
        [field: SerializeField]
        public SpriteTintHelper MainSpriteTintHelper { get; private set; }= null!;

        public override void DisplayPlacementStatus(string statusMessage, bool isOk)
        {
            Color targetColor = isOk ? Color.green : Color.red;
            MainSpriteTintHelper.SetTint(targetColor);
        }

        public override void ClearPlacementStatus()
        {
            MainSpriteTintHelper.RestoreColor();
        }
    }
}