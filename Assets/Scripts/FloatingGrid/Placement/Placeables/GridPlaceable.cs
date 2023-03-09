using System;
using System.Collections.Generic;
using UnityEngine;

namespace ButterBoard.FloatingGrid.Placement.Placeables
{
    [Serializable]
    public class GridPlaceable : BasePlaceable
    {
        [SerializeField]
        private List<GridPin> pins = new List<GridPin>();

        [field: SerializeField]
        public Vector3 GridOffset { get; private set; } = Vector3.zero;

        [field: SerializeField]
        public SpriteTintHelper MainSpriteTintHelper { get; private set; } = null!;

        [field: SerializeField]
        public GridPoint[] OverlappingPoints { get; set; } = Array.Empty<GridPoint>();

        [field: SerializeField]
        public GridHost? HostingGrid { get; set; }

        [field: SerializeField]
        public bool SnapsToGridSnapPoints { get; set; }

        public IReadOnlyList<GridPin> Pins => pins;


        public override void DisplayPlacementStatus(string statusMessage, bool isOk)
        {
            Color targetColor = isOk ? Color.green : Color.red;
            MainSpriteTintHelper.SetTint(targetColor);
        }

        public override void ClearPlacementStatus()
        {
            MainSpriteTintHelper.RestoreColor();
        }

        public void DisplayPinIssue(PinPlacementIssue issue)
        {
            issue.PinWithIssue.DisplayIssue(issue.IssueType);
        }

        public void ClearPinIssues()
        {
            foreach (GridPin gridPin in Pins)
            {
                gridPin.ClearIssue();
            }
        }
    }
}