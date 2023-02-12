using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using BeanCore.Unity.ReferenceResolver;
using BeanCore.Unity.ReferenceResolver.Attributes;
using UnityEngine;

namespace ButterBoard.FloatingGrid
{
    [RequireComponent(typeof(GridBuilder))]
    [ExecuteInEditMode]
    public class EditorGridBuilder : ReferenceResolvedBehaviour
    {
        [BindComponent]
        public GridBuilder GridBuilder { get; private set; } = null!;

#if DEBUG
        #pragma warning disable CS0414
        [SerializeField]
        private bool revalidate = false;
        #pragma warning restore CS0414
#endif

        [SerializeField]
        public Transform targetTransform = null!;

        [SerializeField]
        public GridBuildOffsetType gridOffsetType = GridBuildOffsetType.TOP_LEFT;

        [SerializeField]
        public int width = 10;

        [SerializeField]
        public int height = 4;

        [SerializeField]
        public float spacing = 0.1f;

        private void Awake()
        {
            RunResolve();

            if (targetTransform == null)
                targetTransform = transform;
        }
    }
}