using System;
using System.Collections.Generic;
using ButterBoard.FloatingGrid.Placement.Placeables;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ButterBoard.FloatingGrid.Placement.Services
{
    public class FloatingPlacementService : PlacementService<FloatingPlaceable>
    {
        public FloatingPlacementService(LerpSettings lerpSettings, float displayZDistance) : base(lerpSettings, displayZDistance)
        {
        }

        public override void BeginPrefabPlacement(GameObject prefab)
        {
            // create real and display objects
            GameObject placingObject = Object.Instantiate(prefab);

            // get placeable component on placing object
            FloatingPlaceable? placeable = placingObject.GetComponent<FloatingPlaceable>();

            // throw if not found
            if (placeable == null)
                throw new ArgumentException($"Cannot begin placement of prefab {placingObject.name} as argument it does not have a {nameof(FloatingPlaceable)} component");

            // use blank instead of duplicate to avoid slowdowns during duplication
            GameObject checkingObject = new GameObject();

            // set context
            Context = new PlacementContext<FloatingPlaceable>(placingObject, checkingObject, placeable, placeable, PlacementType.PLACE);

            // notify the placeable that it is the checking version
            Context.Placeable.SetDisplayStatus(true);
        }

        public override void BeginMovePlacement(GameObject target)
        {
            // get placeable component on target object
            FloatingPlaceable? placeable = target.GetComponent<FloatingPlaceable>();

            // throw if not found
            if (placeable == null)
                throw new ArgumentException($"Cannot begin movement of object {target.name} as argument {nameof(target)} does not have a {nameof(FloatingPlaceable)} component");

            // invoke pickup - should disable/disconnect ect. all components that are used during tick.
            placeable.Pickup.Invoke();

            // use blank instead of duplicate to avoid slowdowns during duplication
            GameObject checkingObject = new GameObject();

            // set context
            Context = new PlacementContext<FloatingPlaceable>(target, checkingObject, placeable, placeable, PlacementType.MOVE);

            // clear parent
            Context.PlacingObject.transform.SetParent(null);

            // notify the placeable that it is the checking version
            Context.Placeable.SetDisplayStatus(true);
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

        public override void Remove(BasePlaceable target)
        {
            // get all child placeables (includes self)
            BasePlaceable[] childPlaceables = target.GetComponentsInChildren<BasePlaceable>();

            foreach (BasePlaceable child in childPlaceables)
            {
                // check if child is self
                if (child != target)
                    // remove child if not
                    PlacementManager.Instance.Remove(child);
            }

            base.Remove(target);
        }
    }

}