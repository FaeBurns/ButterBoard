using UnityEngine;

namespace ButterBoard.FloatingGrid.Placement
{
    public class GridPlacementContext
    {
        public GridPlacementContext(GameObject placingObject, GridPlaceable placeable)
        {
            PlacingObject = placingObject;
            Placeable = placeable;
            Size = placeable.BoundsCollider.bounds.size;
        }

        public GameObject PlacingObject { get; }

        public BasePlaceable Placeable { get; }

        public GridPlaceable GridPlaceable => (GridPlaceable)Placeable;

        public FloatingPlaceable FloatingPlaceable => (FloatingPlaceable)Placeable;

        public Vector2 Size { get; }

        public GridPlacementState State { get; set; } = GridPlacementState.POSITION;
    }

    public enum GridPlacementState
    {
        POSITION,
        FINALIZE,
    }
}