using System;
using System.Collections.Generic;
using BeanCore.Unity.ReferenceResolver;
using BeanCore.Unity.ReferenceResolver.Attributes;
using UnityEngine;
using UnityEngine.Events;

namespace ButterBoard.FloatingGrid
{
    [Serializable]
    public class GridPlaceable : BasePlaceable
    {
        [SerializeField]
        private List<GridPin> pins = new List<GridPin>();

        [field: SerializeField]
        public bool AllowFloating { get; private set; }

        [field: SerializeField]
        public Vector3 GridOffset { get; private set; } = Vector3.zero;

        [field: SerializeField]
        public SpriteRenderer[] MainSprites { get; private set; }= null!;

        [field: SerializeField]
        public GridPoint[] OverlappingPoints { get; set; } = Array.Empty<GridPoint>();

        [field: SerializeField]
        public GridHost? HostingGrid { get; set; }

        public IReadOnlyList<GridPin> Pins => pins;

        public override void DisplayPlacementStatus(string statusMessage, bool isOk)
        {
            Color displayColor = isOk ? Color.green : Color.red;

            foreach (SpriteRenderer sprite in MainSprites)
            {
                sprite.color = displayColor;
            }
        }

        public override void ClearPlacementStatus()
        {
            foreach (SpriteRenderer sprite in MainSprites)
            {
                sprite.color = Color.white;
            }
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