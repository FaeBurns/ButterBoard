using System;
using BeanCore.Unity.ReferenceResolver;
using BeanCore.Unity.ReferenceResolver.Attributes;
using UnityEngine;

namespace ButterBoard.FloatingGrid
{
    public class GridPoint : MonoBehaviour
    {
        [BindComponent(Child = true)]
        private SpriteRenderer _scalableSprite = null!;

        [BindComponent]
        private CircleCollider2D _pointCollider = null!;

        [field: SerializeField]
        public GridPin? ConnectedPin { get; private set; }

        [field: SerializeField]
        public GridHost HostingGrid { get; private set; } = null!;

        [field: SerializeField]
        public float Radius { get; private set; }

        [field: SerializeField]
        public bool Blocked { get; set; } = false;

        public bool Open => ConnectedPin == null && !Blocked;

        private void Awake()
        {
            this.ResolveReferences();
        }

        public void Initialize(GridHost gridHost)
        {
            // resolve references now
            this.ResolveReferences();

            HostingGrid = gridHost;

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