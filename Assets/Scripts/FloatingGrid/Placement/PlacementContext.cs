﻿using System.Collections.Generic;
using UnityEngine;

namespace ButterBoard.FloatingGrid.Placement
{
    public class PlacementContext<T> where T : BasePlaceable
    {
        public PlacementContext(GameObject placingObject, GameObject displayObject, T placeable, T displayPlaceable, PlacementType placementType)
        {
            PlacingObject = placingObject;
            DisplayObject = displayObject;
            Placeable = placeable;
            DisplayPlaceable = displayPlaceable;
            PlacementType = placementType;
            Size = placeable.BoundsCollider.bounds.size;
        }

        /// <summary>
        /// Gets the object being placed.
        /// </summary>
        public GameObject PlacingObject { get; }

        /// <summary>
        /// Gets the preview object being displayed.
        /// </summary>
        public GameObject DisplayObject { get; }

        /// <summary>
        /// Gets the <see cref="T"/> placeable found on <see cref="PlacingObject"/>.
        /// </summary>
        public T Placeable { get; }

        /// <summary>
        /// Gets the <see cref="T"/> placeable found on <see cref="DisplayObject"/>.
        /// </summary>
        public T DisplayPlaceable { get; }

        /// <summary>
        /// Gets the type of placement occuring.
        /// </summary>
        public PlacementType PlacementType { get; }

        /// <summary>
        /// Gets the bounds of the placeable.
        /// </summary>
        public Vector2 Size { get; }

        /// <summary>
        /// Gets the current state of the placement.
        /// </summary>
        public PlacementState State { get; set; } = PlacementState.POSITION;
    }

    public enum PlacementState
    {
        POSITION,
        FINALIZE,
    }

    public enum PlacementType
    {
        PLACE,
        MOVE,
    }
}