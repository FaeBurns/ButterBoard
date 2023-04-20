using System;
using System.Collections;
using BeanCore.Unity.ReferenceResolver;
using BeanCore.Unity.ReferenceResolver.Attributes;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

namespace ButterBoard.UI.Drag
{
    /// <summary>
    /// A top-level component containing information about a draggable window.
    /// </summary>
    public sealed class DraggableWindow : MonoBehaviour
    {
        private bool _isSetup;
        private bool _isOpen;

        /// <summary>
        /// Gets a value indicating whether only one instance of this window is allowed at once.
        /// </summary>
        [field: SerializeField]
        public bool SingleInstance { get; private set; }

        /// <summary>
        /// Gets the unique handle for this window.
        /// </summary>
        public uint Handle { get; private set; }

        [field: BindComponent(Child = true)]
        public DraggableWindowControlBar ControlBar { get; private set; } = null!;

        /// <summary>
        /// Gets a value indicating whether this window is <see cref="Hide">Hidden</see> or <see cref="Show">Shown</see>.
        /// </summary>
        public bool Hidden { get; private set; }

        public event EventHandler? Closed;

        /// <summary>
        /// Sets up this window. Called on window creation. Should not be called <see cref="DraggableWindowManager"/>.
        /// </summary>
        /// <param name="handle"></param>
        /// <exception cref="InvalidOperationException">Setup has already been called.</exception>
        public void Setup(uint handle)
        {
            // throw if setup has already occured
            if (_isSetup)
                throw new InvalidOperationException("Cannot perform setup on window that has already been set up");

            // resolve all waiting components
            this.ResolveReferences();

            // set handle
            Handle = handle;

            // mark setup
            _isSetup = true;
        }

        /// <summary>
        /// Close this window.
        /// </summary>
        public void Close()
        {
            DraggableWindowManager.Instance.CloseWindow(Handle);
        }

        /// <summary>
        /// Notifies all listeners of closure. Do not call outside of <see cref="DraggableWindowManager"/>.
        /// </summary>
        public void NotifyOfClosure()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Hides and disables the window without closing it.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            Hidden = true;
        }

        /// <summary>
        /// Shows and enables a <see cref="Hidden"/> window. The window is then brought to the front.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            Hidden = false;

            SetTopmost();
        }

        /// <summary>
        /// Brings this window to the front.
        /// </summary>
        public void SetTopmost()
        {
            transform.SetAsLastSibling();
        }

        /// <summary>
        /// Sets the title of the window.
        /// </summary>
        /// <param name="title">The title of the window.</param>
        public void SetTitle(string title)
        {
            ControlBar.Title = title;
        }
    }
}