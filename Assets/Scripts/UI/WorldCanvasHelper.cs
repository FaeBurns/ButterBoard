using BeanCore.Unity.ReferenceResolver;
using BeanCore.Unity.ReferenceResolver.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace ButterBoard.UI
{
    [RequireComponent(typeof(Canvas))]
    public class WorldCanvasHelper : RecordedBehaviour<WorldCanvasHelper>
    {
        [BindComponent]
        private GraphicRaycaster _raycaster = null!;

        [BindComponent]
        private Canvas _canvas = null!;

        [BindComponent]
        private RectTransform _rectTransform = null!;

        [SerializeField]
        private float rotationOffset = 0;

        [SerializeField]
        private bool allowInput = true;

        protected override void Awake()
        {
            base.Awake();

            ReferenceResolver.ResolveReferences(this);

            _canvas.worldCamera = Camera.main;

            DisableInteraction();
        }

        private void Update()
        {
            _rectTransform.rotation = Quaternion.identity * Quaternion.Euler(0, 0, rotationOffset);
        }

        public void EnableInteraction()
        {
            if (allowInput)
                _raycaster.enabled = true;
        }

        public void DisableInteraction()
        {
            _raycaster.enabled = false;
        }
    }
}