using BeanCore.Unity.ReferenceResolver;
using BeanCore.Unity.ReferenceResolver.Attributes;
using UnityEngine;

namespace ButterBoard.FloatingGrid.Placement
{
    public class EditorPlacementManager : ReferenceResolvedBehaviour
    {
        [BindComponent]
        public PlacementManager PlacementManager { get; private set; } = null!;
    }
}