﻿using UnityEngine;

namespace ButterBoard.FloatingGrid.Placement.Services
{
    public class FloatingPlacementService : PlacementService<FloatingPlaceable>
    {

        public override void BeginPrefabPlacement(GameObject prefab)
        {
            throw new System.NotImplementedException();
        }

        public override void BeginMovePlacement(GameObject target)
        {
            throw new System.NotImplementedException();
        }

        protected override bool CommitPlacement()
        {
            throw new System.NotImplementedException();
        }

        protected override void UpdatePosition(Vector3 targetPosition, Quaternion targetRotation)
        {
            throw new System.NotImplementedException();
        }

        protected override bool UpdateFinalize()
        {
            throw new System.NotImplementedException();
        }

        public FloatingPlacementService(LerpSettings lerpSettings) : base(lerpSettings)
        {
        }
    }

}