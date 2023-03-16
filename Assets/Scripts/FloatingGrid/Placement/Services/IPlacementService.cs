using UnityEngine;

namespace ButterBoard.FloatingGrid.Placement.Services
{
    public interface IPlacementService
    {
        public void BeginPrefabPlacement(GameObject prefab);

        public void BeginMovePlacement(GameObject target);

        public void TryCommitPlacement(Vector3 targetPosition, Quaternion targetRotation);

        public bool Update(Vector3 targetPosition, Quaternion targetRotation);

        public void CancelPlacement();

        public void CompletePlacement();

        public bool CanCancel();
    }
}