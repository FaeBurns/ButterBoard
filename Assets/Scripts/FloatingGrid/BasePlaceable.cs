using BeanCore.Unity.ReferenceResolver;
using UnityEngine;
using UnityEngine.Events;

namespace ButterBoard.FloatingGrid
{
    public abstract class BasePlaceable : ReferenceResolvedBehaviour
    {
        [field: SerializeField]
        public Collider2D BoundsCollider { get; private set; } = null!;

        [field: SerializeField]
        public UnityEvent? Place { get; private set; }

        [field: SerializeField]
        public UnityEvent? Pickup { get; private set; }

        [field: SerializeField]
        public UnityEvent? Remove { get; private set; }

        public abstract void SetPlaceStatus(Color statusColor);
    }
}