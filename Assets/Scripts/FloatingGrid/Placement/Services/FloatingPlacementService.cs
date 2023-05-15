using System;
using System.Collections.Generic;
using ButterBoard.Building;
using ButterBoard.Building.BuildActions.Move;
using ButterBoard.Building.BuildActions.Place;
using ButterBoard.Building.BuildActions.Remove;
using ButterBoard.FloatingGrid.Placement.Placeables;
using ButterBoard.UI.Rack;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ButterBoard.FloatingGrid.Placement.Services
{
    public class FloatingPlacementService : PlacementService<FloatingPlaceable>
    {
        public FloatingPlacementService(LerpSettings lerpSettings, float displayZDistance) : base(lerpSettings, displayZDistance)
        {
        }

        public override void BeginPrefabPlacement(GameObject prefab, string assetSourceKey)
        {
            // create real and display objects
            GameObject placingObject = Object.Instantiate(prefab, BuildManager.GetFloatingPlaceableHost());

            // get placeable component on placing object
            FloatingPlaceable? placeable = placingObject.GetComponent<FloatingPlaceable>();

            // throw if not found
            if (placeable == null)
                throw new ArgumentException($"Cannot begin placement of prefab {placingObject.name} as argument it does not have a {nameof(FloatingPlaceable)} component");

            // set source key
            placeable.SourceAssetKey = assetSourceKey;

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

            moveInitialPosition = target.transform.position;
            moveInitialRotation = placeable.PlacedRotation;
            
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
            
            BuildAction action; 
            switch (Context.PlacementType)
            {
                case PlacementType.PLACE:
                    BuildManager.RegisterPlaceable(Context.Placeable, BuildManager.GetNextRegistryId());
                    action = FloatingPlacementAction.CreateInstance(Context.Placeable, Context.CheckingObject.transform.position, Context.CheckingObject.transform.rotation.eulerAngles.z);
                    break;
                case PlacementType.MOVE:
                    action = FloatingMoveAction.CreateInstance(Context.Placeable, moveInitialPosition, moveInitialRotation, Context.CheckingObject.transform.position, Context.CheckingObject.transform.rotation.eulerAngles.z);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            BuildActionManager.Instance.PushNoExecuteAction(action);

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
            FloatingRemoveSelfAndChildrenAction action = FloatingRemoveSelfAndChildrenAction.CreateInstance(target.Key);
            BuildActionManager.Instance.PushAndExecuteAction(action);
        }
    }

}