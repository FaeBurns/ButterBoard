using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace ButterBoard.FloatingGrid
{
    public static class PlacementHelpers
    {
        public static Vector3 SnapPositionToGrid(GridHost targetGrid, Vector3 position, Vector3 localOffset)
        {
            Transform gridTransform = targetGrid.transform;
            Vector3 gridPosition = gridTransform.position;
            float gridRotation = gridTransform.rotation.eulerAngles.z;
            float gridScale = targetGrid.Spacing;
            Vector3 offset = targetGrid.TopLeftOffsetFromCenter.Mod(gridScale);

            // rotate grid and position back so that they're in default space
            // apply grid snap
            // apply any offsets
            // rotate position back

            // get position relative to grid origin
            Vector3 relativePosition = (position + offset) - gridPosition;

            // initial position rotation
            Vector3 rotatedPosition = relativePosition.Rotate(-gridRotation);

            // snap position to grid
            Vector3 snappedPosition = SnapPositionToLocalGridOfSize(rotatedPosition, gridScale);

            // offset snapped position
            Vector3 offsetSnappedPosition = (snappedPosition + localOffset) - offset;

            // rotate position back to where it was before (now with snap applied)
            Vector3 gridRotatedPosition = offsetSnappedPosition.Rotate(gridRotation);

            // re-add offset remove in step one
            Vector3 gridRelativePosition = gridRotatedPosition + gridPosition;

            // return result
            return gridRelativePosition;
        }

        public static Vector3 SnapPositionToLocalGridOfSize(Vector3 position, float size)
        {
            return new Vector3(Mathf.Round(position.x) * size, Mathf.Round(position.y) * size, Mathf.Round(position.z) * size);
        }

        public static Vector3 GetMouseWorldPosition()
        {
            // get position of mouse on screen
            Vector3 screenPosition = Input.mousePosition;

            // translate to world position
            Vector3 result = Camera.main!.ScreenToWorldPoint(screenPosition);

            // set z of world position to 0
            result.z = 0;

            return result;
        }

        public static Vector3 SnapPositionToGridSnapPoints(GridHost targetGrid, Vector3 position)
        {
            float closestDistance = float.MaxValue;
            Vector3 closestPosition = Vector3.zero;
            foreach (GameObject snapPoint in targetGrid.SnapPoints)
            {
                Vector3 snapPointPosition = snapPoint.transform.position;

                float distance = Vector3.Distance(snapPointPosition, position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPosition = snapPointPosition;
                }
            }

            return closestPosition;
        }
    }
}