using BeanCore.Unity.ReferenceResolver;
using BeanCore.Unity.ReferenceResolver.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ButterBoard.UI.DraggableWindows
{
    /// <summary>
    /// A component that handles the drag movement of a <see cref="Window"/>. Must have a transparent image component to receive drag events.
    /// </summary>
    public class WindowDragHandle : ReferenceResolvedBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
    {
        private Vector2 _startMouseScreenPosition;
        private Vector2 _startWindowScreenPosition;

        [SerializeField]
        private WindowControlBar controlBar = null!;

        [field: SerializeField]
        public RectTransform WindowTransform { get; set; } = null!;

        [AutoReference]
        private CanvasScaler _canvasScaler = null!;

        /// <summary>
        /// Gets a value indicating whether this window is in the process of being dragged.
        /// </summary>
        public bool IsDragging { get; private set; }

        public void OnBeginDrag(PointerEventData eventData)
        {
            IsDragging = true;
            _startMouseScreenPosition = ScaleScreenPosition(eventData.pressPosition);
            _startWindowScreenPosition = WindowTransform.anchoredPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // exit if not dragging
            if (!eventData.dragging)
                return;

            UpdatePosition(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            IsDragging = false;

            UpdatePosition(eventData);

            _startMouseScreenPosition = Vector2.zero;
            _startWindowScreenPosition = Vector2.zero;
        }

        private void UpdatePosition(PointerEventData eventData)
        {
            // ScaleScreenPosition used to stop issues when CanvasScaler is in use
            Vector2 desiredWindowPosition = GetDesiredWindowPosition(ScaleScreenPosition(eventData.position));
            WindowTransform.anchoredPosition = LimitWindowPositionToViewportRange(desiredWindowPosition);
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

        private Vector2 LimitWindowPositionToViewportRange(Vector2 currentCalculatedPosition)
        {
            Vector2 windowSize = WindowTransform.sizeDelta;
            Vector2 viewportSize = WindowHost.Instance.ViewportRectTransform.rect.size;

            // limit to fit inside top and left borders
            Vector2 windowTopLeft = new Vector2(Mathf.Max(currentCalculatedPosition.x, 0), Mathf.Min(currentCalculatedPosition.y, 0));

            // limit to fit inside bottom and right borders
            windowTopLeft = new Vector2(Mathf.Min(windowTopLeft.x, viewportSize.x - windowSize.x), Mathf.Max(windowTopLeft.y, -viewportSize.y + windowSize.y));

            Debug.Log($"CCP: {currentCalculatedPosition} | TL: {windowTopLeft} | VS: {viewportSize}");

            return windowTopLeft;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            controlBar.OnControlBarClicked();
        }

        /// <summary>
        /// Scales the given screen position by the <see cref="CanvasScaler">CanvasScaler's</see> scale amount.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private Vector2 ScaleScreenPosition(Vector2 position)
        {
            RectTransform scalerTransform = (RectTransform)_canvasScaler.transform;
            Vector3 scalerScale = scalerTransform.localScale;
            return new Vector2(position.x / scalerScale.x, position.y / scalerScale.y);
        }
    }
}