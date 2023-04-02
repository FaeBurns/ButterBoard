using UnityEngine;

namespace ButterBoard
{
    [RequireComponent(typeof(Canvas))]
    public class WorldCanvasHelper : MonoBehaviour
    {
        private Canvas _canvas = null!;
        private RectTransform _rectTransform = null!;

        [SerializeField]
        private float rotationOffset = 0;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _rectTransform = GetComponent<RectTransform>();

            _canvas.worldCamera = Camera.main;
        }

        private void Update()
        {
            _rectTransform.rotation = Quaternion.identity * Quaternion.Euler(0, 0, rotationOffset);
        }
    }
}