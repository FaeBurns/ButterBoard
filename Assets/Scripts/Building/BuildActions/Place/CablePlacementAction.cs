using System;
using ButterBoard.Building.BuildHandlers;
using ButterBoard.FloatingGrid.Placement.Placeables;
using Newtonsoft.Json;
using UnityEngine;

namespace ButterBoard.Building.BuildActions.Place
{
    public class CablePlacementAction : BuildAction
    {
        [JsonProperty] private string _prefabKey = String.Empty;
        [JsonProperty] private int _cableEndAGridKey;
        [JsonProperty] private int _cableEndBGridKey;
        [JsonProperty] private int _pointAIndex;
        [JsonProperty] private int _pointBIndex;
        
        [JsonProperty] private int _cableEndKey;
        [JsonProperty] private int _cableOtherEndKey;

        public static CablePlacementAction CreateInstance(CablePlaceable cable)
        {
            return new CablePlacementAction()
            {
                _prefabKey = cable.SourceAssetKey,
                _pointAIndex = cable.Pin.ConnectedPoint.PointIndex,
                _pointBIndex = cable.OtherCable.Pin.ConnectedPoint.PointIndex,

                _cableEndKey = cable.Key,
                _cableOtherEndKey = cable.OtherCable.Key,

                _cableEndAGridKey = cable.Pin.ConnectedPoint.HostingGrid.Key,
                _cableEndBGridKey = cable.OtherCable.Pin.ConnectedPoint.HostingGrid.Key,
            };
        }

        public override void Execute()
        {
            CablePlaceable cableEnd = CableBuildHandler.Place(_prefabKey, _pointAIndex, _pointBIndex, _cableEndAGridKey, _cableEndBGridKey);
            _cableEndKey = BuildManager.RegisterPlaceable(cableEnd, BuildManager.GetNextOrExistingId(_cableEndKey));
            _cableOtherEndKey = BuildManager.RegisterPlaceable(cableEnd.OtherCable, BuildManager.GetNextOrExistingId(_cableOtherEndKey));
        }

        public override void UndoExecute()
        {
            CableBuildHandler.Remove(BuildManager.GetPlaceable<CablePlaceable>(_cableEndKey));
            BuildManager.RemoveRegisteredPlaceable(_cableEndKey);
            BuildManager.RemoveRegisteredPlaceable(_cableOtherEndKey);
        }
    }
}