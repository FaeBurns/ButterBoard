using System;
using ButterBoard.Building.BuildHandlers;
using ButterBoard.FloatingGrid.Placement.Placeables;
using Newtonsoft.Json;
using UnityEngine;

namespace ButterBoard.Building.BuildActions.Remove
{
    public class FloatingRemoveSelfAndChildrenAction : BuildAction
    {
        [JsonProperty]
        private FloatingRemoveSelfOnlyAction _removeSelfAction;
        
        [JsonProperty]
        private BuildAction[] _childActions;

        public FloatingRemoveSelfAndChildrenAction(int placeableKey)
        {
            _removeSelfAction = new FloatingRemoveSelfOnlyAction(placeableKey);

            FloatingPlaceable placeable = BuildManager.GetPlaceable<FloatingPlaceable>(placeableKey);

            // get all child placeables
            BasePlaceable[] childPlaceables = placeable.GetComponentsInChildren<BasePlaceable>();
            _childActions = new BuildAction[childPlaceables.Length - 1];

            // start at 1 - first will always be the floating host
            for (int i = 1; i < childPlaceables.Length; i++)
            {
                switch (childPlaceables[i])
                {
                    case CablePlaceable cablePlaceable:
                        _childActions[i - 1] = new CableRemoveAction(cablePlaceable.Key);
                        break;
                    case FloatingPlaceable floatingPlaceable:
                        _childActions[i - 1] = new FloatingRemoveSelfOnlyAction(floatingPlaceable.Key);
                        break;
                    case GridPlaceable gridPlaceable:
                        _childActions[i - 1] = new GridRemoveAction(gridPlaceable.Key, placeableKey);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(childPlaceables));

                }
            }
        }

        public override void Execute()
        {
            foreach (BuildAction buildAction in _childActions)
            {
                buildAction.Execute();
            }
            
            _removeSelfAction.Execute();
        }

        public override void UndoExecute()
        {
            _removeSelfAction.UndoExecute();

            foreach (BuildAction buildAction in _childActions)
            {
                buildAction.UndoExecute();
            }
        }
    }
}