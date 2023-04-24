using System;
using BeanCore.Unity.ReferenceResolver.Attributes;
using UnityEngine;

namespace ButterBoard.UI.DraggableWindows
{
    /// <summary>
    /// Base class for all window types.
    /// </summary>
    public abstract class Window : MonoBehaviour
    {
        /// <summary>
        /// Gets a value indicating whether the window has been opened.
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the window is visible.
        /// </summary>
        public bool IsVisible { get; private set; }

        /// <summary>
        /// Gets the <see cref="WindowControlBar"/> for this window.
        /// </summary>
        [BindComponent(Child = true)]
        public WindowControlBar ControlBar { get; private set; } = null!;

        /// <summary>
        /// Opens the <see cref="Window"/>.
        /// </summary>
        protected void Open()
        {
            // if window is already open, bring it to the front
            if (IsOpen)
            {
                BringToFront();
                return;
            }

            IsOpen = true;
            BringToFront();

            ShowWithoutNotify();

            // notify of open
            OnOpen();
        }

        /// <summary>
        /// Closes the <see cref="Window"/>.
        /// </summary>
        public void Close()
        {
            // exit if window is already closed
            if (!IsOpen)
                return;

            // notify of close
            OnClose();

            Destroy(gameObject);
            IsOpen = false;
        }

        /// <summary>
        /// Show this <see cref="Window"/>.
        /// </summary>
        public void Show()
        {
            ShowWithoutNotify();

            OnShow();
        }

        private void ShowWithoutNotify()
        {
            IsVisible = true;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Hide this <see cref="Window"/>.
        /// </summary>
        public void Hide()
        {
            IsVisible = false;
            gameObject.SetActive(false);

            OnHide();
        }

        /// <summary>
        /// Brings this <see cref="Window"/> to the front of the display.
        /// </summary>
        public void BringToFront()
        {
            if (!IsOpen)
                throw new InvalidOperationException("Cannot bring a window to the front if it is not open");

            transform.SetAsLastSibling();
        }

        /// <summary>
        /// Set the title of the window.
        /// </summary>
        /// <param name="title"></param>
        public void SetTitle(string title)
        {
            ControlBar.Title = title;
        }

        protected virtual void OnOpen() { }
        protected virtual void OnClose() { }
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
    }
}