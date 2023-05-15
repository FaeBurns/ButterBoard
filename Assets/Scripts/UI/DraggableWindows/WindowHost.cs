using System;
using BeanCore.Unity.ReferenceResolver;
using BeanCore.Unity.ReferenceResolver.Attributes;
using UnityEngine;

namespace ButterBoard.UI.DraggableWindows
{
    /// <summary>
    /// A singleton used to reference the transform that all windows should be children of.
    /// </summary>
    public class WindowHost : SingletonBehaviour<WindowHost>
    {
        [BindComponent(Parent = true)]
        public Canvas HostCanvas { get; private set; } = null!;

        [field: SerializeField]
        public RectTransform ViewportRectTransform { get; private set; } = null!;

        protected override void Awake()
        {
            base.Awake();
            ReferenceResolver.ResolveReferences(this);
        }
    }
}