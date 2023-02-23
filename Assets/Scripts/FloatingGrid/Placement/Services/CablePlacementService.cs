using System;
using ButterBoard.Cables;
using UnityEngine;

namespace ButterBoard.FloatingGrid.Placement.Services
{
    public class CablePlacementService : PlacementService<CablePlaceable>
    {
        public CablePlacementService(LerpSettings lerpSettings, float displayZDistance) : base(lerpSettings, displayZDistance)
        {
        }

        public override void BeginPrefabPlacement(GameObject prefab)
        {

        }

        public override void BeginMovePlacement(GameObject target)
        {

        }

        protected override bool CommitPlacement()
        {
            throw new NotImplementedException();
        }

        protected override void UpdatePosition(Vector3 targetPosition, Quaternion targetRotation)
        {

        }

        protected override bool UpdateFinalize()
        {
            throw new NotImplementedException();
        }
    }
}