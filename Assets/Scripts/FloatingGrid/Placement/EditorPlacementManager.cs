using System;
using BeanCore.Unity.ReferenceResolver;
using BeanCore.Unity.ReferenceResolver.Attributes;
using UnityEngine;

namespace ButterBoard.FloatingGrid.Placement
{
    public class EditorPlacementManager : ReferenceResolvedBehaviour
    {
        [BindComponent]
        public PlacementManager PlacementManager { get; private set; } = null!;

        [field: SerializeField]
        public string TestPlacementAsset { get; private set; } = String.Empty;

        [field: SerializeField]
        public CableColour CableColour { get; private set; } = CableColour.RED;
    }

    public enum CableColour
    {
        RED,
        GREEN,
        BLUE,
    }
}