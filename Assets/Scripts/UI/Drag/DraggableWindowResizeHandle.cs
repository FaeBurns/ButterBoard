using GluonGui.WorkspaceWindow.Views.WorkspaceExplorer.Configuration;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ButterBoard.UI.Drag
{
    /// <summary>
    /// A component that handles the resizing of a <see cref="DraggableWindow"/>. Must have a transparent image component to receive drag events. Assumed to be placed in the bottom right corner
    /// </summary>
    public class DraggableWindowResizeHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Vector2 _staticAnchorPosition;
        private Vector2 _startMovingAnchorPosition;
        private Vector2 _startMouseScreenPosition;

        [SerializeField]
        private Vector2 minimumSize = new Vector2(200, 100);

        [SerializeField]
        private RectTransform windowTransform = null!;

        public void OnBeginDrag(PointerEventData eventData)
        {
            Rect rect = windowTransform.rect;
            _staticAnchorPosition = new Vector2(rect.xMin, rect.yMax);
            _startMouseScreenPosition = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdatePosition(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            UpdatePosition(eventData);
        }

        public void UpdatePosition(PointerEventData eventData)
        {
            (Vector2 position, Vector2 size) = GetDesiredResizeRect(eventData.position);
            windowTransform.anchoredPosition = position;
            windowTransform.sizeDelta = size;
        }

        public (Vector2 position, Vector2 size) GetDesiredResizeRect(Vector2 currentMouseScreenPosition)
        {
            Vector2 mouseDelta = currentMouseScreenPosition - _startMouseScreenPosition;

            Vector2 size = (_startMovingAnchorPosition + mouseDelta) - _staticAnchorPosition;
            Vector2 limitedSize = new Vector2(Mathf.Max(size.x, minimumSize.x), -Mathf.Min(-size.y, -minimumSize.y));

            Vector2 position = (_staticAnchorPosition + limitedSize) / 2;

            return (position, limitedSize);
        }
    }
}