using System;
using System.Collections.Generic;
using ButterBoard.FloatingGrid.Placement.Placeables;

namespace ButterBoard.UI.Rack
{
    [Obsolete]
    public static class PlacementLimitManager
    {
        private static readonly Dictionary<string, int> _recordedPlacementCounts = new Dictionary<string, int>();

        public static void MarkPlacement(BasePlaceable placedObject)
        {
            placedObject.Remove.AddListener(() => MarkRemoval(placedObject));

            if (!_recordedPlacementCounts.ContainsKey(placedObject.SourceAssetKey))
                _recordedPlacementCounts.Add(placedObject.SourceAssetKey, 0);

            _recordedPlacementCounts[placedObject.SourceAssetKey]++;
        }

        public static bool CanPlace(RackEntryAsset entryAsset)
        {
            // allow if no limit is set
            if (entryAsset.MaxBuildCount <= 0)
                return true;

            // if a recorded entry exists
            // check if another one can be fit in the range
            if (_recordedPlacementCounts.ContainsKey(entryAsset.SpawnTargetSourceKey))
                return _recordedPlacementCounts[entryAsset.SpawnTargetSourceKey] < entryAsset.MaxBuildCount;

            // no recorded entry, allow placement
            return true;
        }

        public static void MarkRemoval(BasePlaceable removingObject)
        {
            if (_recordedPlacementCounts.ContainsKey(removingObject.SourceAssetKey))
                _recordedPlacementCounts[removingObject.SourceAssetKey]--;
        }
    }
}