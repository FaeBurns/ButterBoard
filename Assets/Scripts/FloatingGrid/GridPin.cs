using System;
using BeanCore.Unity.ReferenceResolver;
using BeanCore.Unity.ReferenceResolver.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace ButterBoard.FloatingGrid
{
    public class GridPin : ReferenceResolvedBehaviour
    {
        public GridPoint? ConnectedPoint { get; private set; }

        [BindComponent(Parent = true)]
        public GridPlaceable HostPlaceable { get; private set; } = null!;

        public void Connect(GridPoint target)
        {
            ConnectedPoint = target;
        }

        public void Free()
        {
            ConnectedPoint = null;
        }
    }
}