using System.Collections.Generic;
using UnityEngine;

namespace ButterBoard.FloatingGrid
{
    public class GridPlaceable : MonoBehaviour
    {
        public IReadOnlyList<GridPin> Pins => pins;

        [SerializeField]
        private List<GridPin> pins = new List<GridPin>();

        [field: SerializeField]
        public Collider2D BoundsCollider { get; private set; } = null!;

        [field: SerializeField]
        public Vector3 GridOffset { get; private set; } = Vector3.zero;
    }
}