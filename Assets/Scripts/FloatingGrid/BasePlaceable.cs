using System;
using BeanCore.Unity.ReferenceResolver;
using UnityEngine;
using UnityEngine.Events;

namespace ButterBoard.FloatingGrid
{
    public abstract class BasePlaceable : ReferenceResolvedBehaviour
    {
        /// <summary>
        /// Gets the bounds collider of the placeable.
        /// </summary>
        [field: SerializeField]
        public Collider2D BoundsCollider { get; private set; } = null!;

        /// <summary>
        /// Gets an event fired when this placeable is placed.
        /// </summary>
        [field: SerializeField]
        public UnityEvent? Place { get; private set; }

        /// <summary>
        /// Gets an event fired when this placeable is picked up.
        /// </summary>
        [field: SerializeField]
        public UnityEvent? Pickup { get; private set; }

        /// <summary>
        /// Gets an event fired when this placeable is about to be removed.
        /// </summary>
        [field: SerializeField]
        public UnityEvent? Remove { get; private set; }

        /// <summary>
        /// Allows this placeable to display information about why it can or cannot be placed.
        /// </summary>
        public abstract void DisplayPlacementStatus(string statusMessage, bool isOk);

        /// <summary>
        /// Clear any values set by <see cref="DisplayPlacementStatus"/>.
        /// </summary>
        public abstract void ClearPlacementStatus();

        /// <summary>
        /// Marks this object as the display instance.
        /// </summary>
        public virtual void SetDisplay()
        {
            BoundsCollider.enabled = false;
        }
    }
}