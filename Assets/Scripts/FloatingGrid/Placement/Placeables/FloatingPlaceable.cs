using System;
using System.Collections.Generic;
using ButterBoard.Building;
using UnityEngine;

namespace ButterBoard.FloatingGrid.Placement.Placeables
{
    public class FloatingPlaceable : BasePlaceable
    {
        [field: SerializeField]
        public List<GridHost> GridHosts { get; private set; } = new List<GridHost>();

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