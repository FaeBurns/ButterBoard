using UnityEngine;
using UnityEngine.EventSystems;

namespace ButterBoard.UI.DraggableWindows
{
    /// <summary>
    /// A component that handles the drag movement of a <see cref="Window"/>. Must have a transparent image component to receive drag events.
    /// </summary>
    public class WindowDragHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Vector2 _startMouseScreenPosition;
        private Vector2 _startWindowScreenPosition;

        [field: SerializeField]
        public RectTransform TargetTransform { get; set; } = null!;

        /// <summary>
        /// Gets a value indicating whether this window is in the process of being dragged.
        /// </summary>
        public bool IsDragging { get; private set; }

        public void OnBeginDrag(PointerEventData eventData)
        {
            IsDragging = true;
            _startMouseScreenPosition = Input.mousePosition;
            _startWindowScreenPosition = TargetTransform.anchoredPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // exit if not dragging
            if (!eventData.dragging)
                return;

            // eventData.position seems to lag behind a little
            TargetTransform.anchoredPosition = GetDesiredWindowPosition(Input.mousePosition);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            IsDragging = false;
            _startMouseScreenPosition = Vector2.zero;
            _startWindowScreenPosition = Vector2.zero;
        }

        /// <summary>
        /// Gets the desired position for the window from the current mouse screen position.
        /// </summary>
        /// <param name="currentMouseScreenPosition">The current screen position of the mouse.</param>
        /// <returns>The new screen position of the window.</returns>
        private Vector2 GetDesiredWindowPosition(Vector2 currentMouseScreenPosition)
        {
            // get how much the mouse has moved since starting the drag
            // move the window by the same amount

            Vector2 mouseDelta = currentMouseScreenPosition - _startMouseScreenPosition;
            return _startWindowScreenPosition + mouseDelta;
        }
    }
}