using ButterBoard.Building.BuildHandlers;
using ButterBoard.FloatingGrid;
using ButterBoard.FloatingGrid.Placement.Placeables;
using Newtonsoft.Json;

namespace ButterBoard.Building.BuildActions.Move
{
    public class CableMoveAction : BuildAction
    {
        [JsonProperty]
        private int _cableKey;
        
        [JsonProperty]
        private int _sourcePointIndex;
        
        [JsonProperty]
        private int _targetPointIndex;

        [JsonProperty]
        private int _sourceGridHostId;

        [JsonProperty]
        private int _targetGridHostId;

        public CableMoveAction(CablePlaceable placeable, int sourcePointIndex, int targetPointIndex, GridHost sourceGrid, GridHost targetGrid)
        {
            _cableKey = placeable.Key;
            _sourcePointIndex = sourcePointIndex;
            _targetPointIndex = targetPointIndex;
            
            _sourceGridHostId = sourceGrid.GetComponentInParent<BasePlaceable>().Key;
            _targetGridHostId = targetGrid.GetComponentInParent<BasePlaceable>().Key;
        }
        
        public override void Execute()
        {
            CableBuildHandler.Move(BuildManager.GetPlaceable<CablePlaceable>(_cableKey), _targetPointIndex, _targetGridHostId);
        }

        public override void UndoExecute()
        {
            CableBuildHandler.Move(BuildManager.GetPlaceable<CablePlaceable>(_cableKey), _sourcePointIndex, _sourceGridHostId);
        }
    }
}