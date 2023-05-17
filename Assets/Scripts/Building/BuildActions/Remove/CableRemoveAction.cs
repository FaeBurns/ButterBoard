using System;
using System.ComponentModel;
using ButterBoard.Building.BuildHandlers;
using ButterBoard.FloatingGrid.Placement.Placeables;
using Newtonsoft.Json;

namespace ButterBoard.Building.BuildActions.Remove
{
    [DisplayName("Remove")]
    public class CableRemoveAction : BuildAction
    {
        [JsonProperty] private int _cableEndKey;
        [JsonProperty] private int _cableOtherEndKey;

        [JsonProperty] private string _sourceAssetKey = String.Empty;
        [JsonProperty] private int _pointIndexA;
        [JsonProperty] private int _pointIndexB;
        [JsonProperty] private int _cableEndAGridKey;
        [JsonProperty] private int _cableEndBGridKey;

        public static CableRemoveAction CreateInstance(int cableEndKey)
        {
            CablePlaceable placeable = BuildManager.GetPlaceable<CablePlaceable>(cableEndKey);
            return new CableRemoveAction()
            {
                _sourceAssetKey = placeable.SourceAssetKey,

                _pointIndexA = placeable.Pin.ConnectedPoint.PointIndex,
                _pointIndexB = placeable.OtherCable.Pin.ConnectedPoint.PointIndex,

                _cableEndKey = cableEndKey,
                _cableOtherEndKey = placeable.OtherCable.Key,

                _cableEndAGridKey = placeable.Pin.ConnectedPoint.HostingGrid.Key,
                _cableEndBGridKey = placeable.OtherCable.Pin.ConnectedPoint.HostingGrid.Key,
            };
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