using ButterBoard.Building.BuildHandlers;
using ButterBoard.FloatingGrid.Placement.Placeables;
using Newtonsoft.Json;

namespace ButterBoard.Building.BuildActions.Remove
{
    public class CableRemoveAction : BuildAction
    {
        [JsonProperty]
        private int _cableEndKey;

        [JsonProperty]
        private int _cableOtherEndKey;

        [JsonIgnore]
        private readonly string _sourceAssetKey;
        [JsonIgnore]
        private readonly int _pointIndexA;
        [JsonIgnore]
        private readonly int _pointIndexB;
        [JsonIgnore]
        private readonly int _cableEndAGridKey;
        [JsonIgnore]
        private readonly int _cableEndBGridKey;
        

        public CableRemoveAction(int cableEndKey)
        {
            CablePlaceable placeable = BuildManager.GetPlaceable<CablePlaceable>(cableEndKey);
            _sourceAssetKey = placeable.SourceAssetKey;

            _pointIndexA = placeable.Pin.ConnectedPoint.PointIndex;
            _pointIndexB = placeable.OtherCable.Pin.ConnectedPoint.PointIndex;
            
            _cableEndKey = cableEndKey;
            _cableOtherEndKey = placeable.OtherCable.Key;

            _cableEndAGridKey = placeable.Pin.ConnectedPoint.HostingGrid.GetComponentInParent<BasePlaceable>().Key;
            _cableEndBGridKey = placeable.OtherCable.Pin.ConnectedPoint.HostingGrid.GetComponentInParent<BasePlaceable>().Key;
        }
        
        public override void Execute()
        {
            CableBuildHandler.Remove(BuildManager.GetPlaceable<CablePlaceable>(_cableEndKey));
            BuildManager.RemoveRegisteredPlaceable(_cableEndKey);
            BuildManager.RemoveRegisteredPlaceable(_cableOtherEndKey);
        }

        public override void UndoExecute()
        {
            CablePlaceable cableEnd = CableBuildHandler.Place(_sourceAssetKey, _pointIndexA, _pointIndexB, _cableEndAGridKey, _cableEndBGridKey);
            BuildManager.RegisterPlaceable(cableEnd, _cableEndKey);
            BuildManager.RegisterPlaceable(cableEnd.OtherCable, _cableOtherEndKey);
        }
    }
}