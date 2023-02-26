using System;
using System.Collections.Generic;
using UnityEngine;

namespace ButterBoard.FloatingGrid.Placement.Services
{
    public class FloatingPlacementService : PlacementService<FloatingPlaceable>
    {
        public FloatingPlacementService(LerpSettings lerpSettings, float displayZDistance) : base(lerpSettings, displayZDistance)
        {
        }

        protected override bool CommitPlacement()
        {
            // check if placement is valid
            List<FloatingPlaceable> allOverlapPlaceables = GetOverlaps<FloatingPlaceable>(Context.CheckingPlaceable);
            bool canPlace = allOverlapPlaceables.Count == 0;

            // if cannot place
            // exit early to allow for changes
            if (!canPlace)
                return false;

            // clear display status
            Context.Placeable.ClearPlacementStatus();

            return true;
        }

        protected override void UpdatePosition(Vector3 targetPosition, Quaternion targetRotation)
        {
            SetPositionAndRotation(targetPosition, targetRotation);

            List<FloatingPlaceable> allOverlapPlaceables = GetOverlaps<FloatingPlaceable>(Context.CheckingPlaceable);

            bool canPlace = allOverlapPlaceables.Count == 0;
            string statusMessage = canPlace ? String.Empty : "Placement Invalid";

            Context.Placeable.DisplayPlacementStatus(statusMessage, canPlace);
        }
    }

}