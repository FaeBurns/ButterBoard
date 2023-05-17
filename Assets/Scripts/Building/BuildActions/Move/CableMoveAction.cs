using System.ComponentModel;
using ButterBoard.Building.BuildHandlers;
using ButterBoard.FloatingGrid;
using ButterBoard.FloatingGrid.Placement.Placeables;
using Newtonsoft.Json;

namespace ButterBoard.Building.BuildActions.Move
{
    [DisplayName("Move")]
    public class CableMoveAction : BuildAction
    {
        [JsonProperty] private int _cableKey;
        [JsonProperty] private int _sourcePointIndex;
        [JsonProperty] private int _targetPointIndex;
        [JsonProperty] private int _sourceGridHostId;
        [JsonProperty] private int _targetGridHostId;

        public static CableMoveAction CreateInstance(CablePlaceable placeable, int sourcePointIndex, int targetPointIndex, GridHost sourceGrid, GridHost targetGrid)
        {
            return new CableMoveAction()
            {
                _cableKey = placeable.Key,
                _sourcePointIndex = sourcePointIndex,
                _targetPointIndex = targetPointIndex,
                _sourceGridHostId = sourceGrid.Key,
                _targetGridHostId = targetGrid.Key,
            };
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