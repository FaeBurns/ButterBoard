using System;
using System.Collections.Generic;
using BeanCore.Unity.ReferenceResolver;
using BeanCore.Unity.ReferenceResolver.Attributes;
using UnityEngine;
using UnityEngine.Events;

namespace ButterBoard.FloatingGrid
{
    public class GridPlaceable : BasePlaceable
    {
        public IReadOnlyList<GridPin> Pins => pins;

        [SerializeField]
        private List<GridPin> pins = new List<GridPin>();

        [field: SerializeField]
        public bool AllowFloating { get; private set; }

        [field: SerializeField]
        public Vector3 GridOffset { get; private set; } = Vector3.zero;

        [BindMultiComponent(Child = true)]
        public SpriteRenderer[] AllSprites { get; private set; }= null!;

        public GridPoint[] OverlappingPoints { get; set; } = Array.Empty<GridPoint>();

        public GridHost? PlacedGrid { get; set; }

        public override void SetPlaceStatus(Color statusColor)
        {
            foreach (SpriteRenderer spriteRenderer in AllSprites)
            {
                spriteRenderer.color = statusColor;
            }
        }
    }
}