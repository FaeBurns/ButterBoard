using System.Collections.Generic;
using BeanCore.Unity.ReferenceResolver;
using BeanCore.Unity.ReferenceResolver.Attributes;
using UnityEngine;

namespace ButterBoard.FloatingGrid
{
    public class GridPlaceable : ReferenceResolvedBehaviour
    {
        public IReadOnlyList<GridPin> Pins => pins;

        [SerializeField]
        private List<GridPin> pins = new List<GridPin>();

        [field: SerializeField]
        public Collider2D BoundsCollider { get; private set; } = null!;

        [field: SerializeField]
        public Vector3 GridOffset { get; private set; } = Vector3.zero;

        [BindMultiComponent(Child = true)]
        public SpriteRenderer[] AllSprites { get; private set; }= null!;

        public void SetSpriteColor(Color statusColor)
        {
            foreach (SpriteRenderer spriteRenderer in AllSprites)
            {
                spriteRenderer.color = statusColor;
            }
        }
    }
}