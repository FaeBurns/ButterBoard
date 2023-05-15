using ButterBoard.Building.BuildHandlers;
using ButterBoard.FloatingGrid.Placement.Placeables;
using Newtonsoft.Json;
using UnityEngine;

namespace ButterBoard.Building.BuildActions.Place
{
    public class CablePlacementAction : BuildAction
    {
        [JsonProperty]
        private string _prefabKey;
        
        [JsonProperty]
        private int _cableEndAGridKey;
        
        [JsonProperty]
        private int _cableEndBGridKey;
        
        [JsonProperty]
        private int _pointAIndex;
        
        [JsonProperty]
        private int _pointBIndex;

        [JsonIgnore]
        private int _cableEndKey;

        [JsonIgnore]
        private int _cableOtherEndKey;

        public CablePlacementAction(CablePlaceable cable)
        {
            _prefabKey = cable.SourceAssetKey;
            _pointAIndex = cable.Pin.ConnectedPoint.PointIndex;
            _pointBIndex = cable.OtherCable.Pin.ConnectedPoint.PointIndex;
            
            _cableEndKey = cable.Key;
            _cableOtherEndKey = cable.OtherCable.Key;
            
            _cableEndAGridKey = cable.Pin.ConnectedPoint.HostingGrid.GetComponentInParent<BasePlaceable>().Key;
            _cableEndBGridKey = cable.OtherCable.Pin.ConnectedPoint.HostingGrid.GetComponentInParent<BasePlaceable>().Key;
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