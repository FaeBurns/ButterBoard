using BeanCore.Unity.ReferenceResolver;
using BeanCore.Unity.ReferenceResolver.Attributes;
using Coil;
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
        public GridPin? ConnectedPin { get; set; }

        [field: SerializeField]
        public GridHost HostingGrid { get; private set; } = null!;

        [field: SerializeField]
        public float Radius { get; set; }

        [field: SerializeField]
        public bool Blocked { get; set; } = false;

        [field: SerializeField]
        public Wire Wire { get; set; } = null!;

        [field: SerializeField]
        public int PointIndex { get; set; } = 0;

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
    }
}