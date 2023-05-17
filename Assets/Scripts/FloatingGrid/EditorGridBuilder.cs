using BeanCore.Unity.ReferenceResolver;
using BeanCore.Unity.ReferenceResolver.Attributes;
using ButterBoard.FloatingGrid;
using UnityEngine;

namespace ButterBoard.Editor.FloatingGrid
{
    [RequireComponent(typeof(GridBuilder))]
    [ExecuteInEditMode]
    public class EditorGridBuilder : ReferenceResolvedBehaviour
    {
        [BindComponent]
        public GridBuilder GridBuilder { get; private set; } = null!;

        [SerializeField]
        public GridHost? activeHost;

        [SerializeField]
        public Transform targetTransform = null!;

        [SerializeField]
        public GridBuildOffsetType gridOffsetType = GridBuildOffsetType.TOP_LEFT;

        [SerializeField]
        public int width = 10;

        [SerializeField]
        public int height = 4;

        [SerializeField]
        public float spacing = 1;
    }
}