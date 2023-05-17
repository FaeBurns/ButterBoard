using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
            List<BuildAction> childActions = new List<BuildAction>(childPlaceables.Length - 1);

            HashSet<int> childCableKeys = new HashSet<int>();

            // skip the first - first will always be the floating host
            foreach (BasePlaceable childPlaceable in childPlaceables.Skip(1))
            {
                switch (childPlaceable)
                {
                    case CablePlaceable cablePlaceable:
                        // skip adding this cable end removal if the other end of the cable has already been added
                        if (childCableKeys.Contains(cablePlaceable.Key))
                            break;

                        childActions.Add(CableRemoveAction.CreateInstance(cablePlaceable.Key));
                        childCableKeys.Add(cablePlaceable.Key);
                        childCableKeys.Add(cablePlaceable.OtherCable.Key);
                        break;
                    case FloatingPlaceable floatingPlaceable:
                        childActions.Add(FloatingRemoveSelfOnlyAction.CreateInstance(floatingPlaceable.Key));
                        break;
                    case GridPlaceable gridPlaceable:
                        childActions.Add(GridRemoveAction.CreateInstance(gridPlaceable.Key, placeableKey));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(childPlaceable));

                }
            }

            return new FloatingRemoveSelfAndChildrenAction()
            {
                _removeSelfAction = FloatingRemoveSelfOnlyAction.CreateInstance(placeableKey),
                _childActions = childActions.ToArray(),
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