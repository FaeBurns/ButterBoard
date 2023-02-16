using System;
using BeanCore.Unity.ReferenceResolver;
using BeanCore.Unity.ReferenceResolver.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace ButterBoard.FloatingGrid
{
    public class GridPoint : MonoBehaviour
    {
        [BindComponent(Child = true)]
        private SpriteRenderer _scalableSprite = null!;

        [BindComponent]
        private CircleCollider2D _pointCollider = null!;

        public GridPin? ConnectedPin { get; private set; }

        public GridHost HostGridHost { get; private set; } = null!;

        public float Radius { get; private set; }

        [field: SerializeField]
        public bool Blocked { get; set; } = false;

        public bool Open => ConnectedPin == null && !Blocked;

        public void Initialize(GridHost gridHost)
        {
            // resolve references now
            this.ResolveReferences();

            HostGridHost = gridHost;

            Vector3 scale = new Vector3(gridHost.Spacing / 2f, gridHost.Spacing / 2f, 1f);
            _scalableSprite.transform.localScale = scale;
            Radius = gridHost.Spacing / 4f;
            _pointCollider.radius = Radius;
        }

        public void Connect(GridPin target)
        {
            ConnectedPin = target;
        }

        public void Free()
        {
            ConnectedPin = null;
        }
    }
}