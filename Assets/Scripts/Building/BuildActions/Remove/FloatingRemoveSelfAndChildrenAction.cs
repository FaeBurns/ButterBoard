using System;
using System.ComponentModel;
using ButterBoard.Building.BuildHandlers;
using ButterBoard.FloatingGrid.Placement.Placeables;
using Newtonsoft.Json;
using UnityEngine;

namespace ButterBoard.Building.BuildActions.Remove
{
    [DisplayName("Remove")]
    public class FloatingRemoveSelfAndChildrenAction : BuildAction
    {
        [JsonProperty] private FloatingRemoveSelfOnlyAction _removeSelfAction = null!;
        [JsonProperty] private BuildAction[] _childActions = Array.Empty<BuildAction>();

        public static FloatingRemoveSelfAndChildrenAction CreateInstance(int placeableKey)
        {
            FloatingPlaceable placeable = BuildManager.GetPlaceable<FloatingPlaceable>(placeableKey);

            // get all child placeables
            BasePlaceable[] childPlaceables = placeable.GetComponentsInChildren<BasePlaceable>();
            BuildAction[] childActions = new BuildAction[childPlaceables.Length - 1];

            // start at 1 - first will always be the floating host
            for (int i = 1; i < childPlaceables.Length; i++)
            {
                switch (childPlaceables[i])
                {
                    case CablePlaceable cablePlaceable:
                        childActions[i - 1] = CableRemoveAction.CreateInstance(cablePlaceable.Key);
                        break;
                    case FloatingPlaceable floatingPlaceable:
                        childActions[i - 1] = FloatingRemoveSelfOnlyAction.CreateInstance(floatingPlaceable.Key);
                        break;
                    case GridPlaceable gridPlaceable:
                        childActions[i - 1] = GridRemoveAction.CreateInstance(gridPlaceable.Key, placeableKey);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(childPlaceables));

                }
            }
            
            return new FloatingRemoveSelfAndChildrenAction()
            {
                _removeSelfAction = FloatingRemoveSelfOnlyAction.CreateInstance(placeableKey),
                _childActions = childActions,
            };
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