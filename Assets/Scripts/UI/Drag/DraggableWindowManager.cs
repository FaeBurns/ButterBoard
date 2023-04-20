using System;
using System.Collections.Generic;
using BeanCore.Unity.ReferenceResolver;
using BeanCore.Unity.ReferenceResolver.Attributes;
using ButterBoard.Lookup;
using UnityEngine;

namespace ButterBoard.UI.Drag
{
    /// <summary>
    /// Manages opening
    /// </summary>
    public class DraggableWindowManager : SingletonBehaviour<DraggableWindowManager>
    {
        [BindComponent(Parent = true)]
        private Canvas hostCanvas = null!;

        [BindComponent]
        public RectTransform RectTransform { get; private set; }= null!;

        [SerializeField]
        private string windowKeyPrefix = String.Empty;

        // start at one as 0 is the id for a not yet set handle reference
        private uint _nextWindowHandle = 1;
        private readonly Dictionary<uint, OpenWindowContainer> _openWindows = new Dictionary<uint, OpenWindowContainer>();

        private void Awake()
        {
            this.ResolveReferences();
        }

        /// <summary>
        /// Creates a new window from the specified key.
        /// </summary>
        /// <param name="windowKey">The key to create the window from.</param>
        /// <param name="screenPlacementPosition">A scale from 0 to 1 on both axis that specifies the position for the window to be created at as a percentage of screen size. Places the center of the window at this position. This argument is optional, if left undefined the window will be placed at the center of the screen.</param>
        /// <returns>A new window or an existing window if the desired template has to <see cref="DraggableWindow.SingleInstance"/> set to true.</returns>
        /// <exception cref="ArgumentException">No window template was found with the specified key.</exception>
        public DraggableWindow CreateWindow(string windowKey, Vector2? screenPlacementPosition = null)
        {
            // set screen placement position to center of screen if not set by optional argument
            // cannot be defined in method signature as it is not a constant value
            Vector2 placementPosition = screenPlacementPosition ?? new Vector2(0.5f, 0.5f);

            // the center of the screen is actually 0, 0 rather than 0.5*width, 0.5*height
            // apply offset here to move placement percentage into the correct position
            placementPosition -= new Vector2(0.5f, 0.5f);

            // try and fetch prefab from provided key
            GameObject? windowPrefab = AssetSource.Fetch<GameObject>(windowKeyPrefix + windowKey);
            if (windowPrefab == null)
                throw new ArgumentException($"Could not create window with key {windowKey}, window does not exist");

            // check if the target window is limited to only one at a time
            if (windowPrefab.GetComponent<DraggableWindow>().SingleInstance)
            {
                // if a single-instance window already exists with the specified key, return that instead.
                DraggableWindow? existingWindow = TryGetWindowByKey(windowKey);
                if (existingWindow != null)
                {
                    // move window to front
                    existingWindow.transform.SetAsLastSibling();
                    return existingWindow;
                }
            }

            // create and set up new window
            GameObject windowObject = Instantiate(windowPrefab, transform);
            DraggableWindow window = windowObject.GetComponent<DraggableWindow>();
            window.Setup(_nextWindowHandle++);

            // set the position of the window and ensure it's the topmost window
            Vector2 canvasPlacementPosition = hostCanvas.renderingDisplaySize * placementPosition;
            RectTransform windowTransform = window.GetComponent<RectTransform>();
            windowTransform.anchoredPosition = canvasPlacementPosition;
            windowTransform.SetAsLastSibling();

            // store in open windows collection
            _openWindows.Add(window.Handle, new OpenWindowContainer(windowKey, window));

            return window;
        }

        /// <summary>
        /// Searches for an open <see cref="DraggableWindow"/> with the specified key.
        /// </summary>
        /// <param name="windowKey">The key of the window being searched for</param>
        /// <returns>The <see cref="DraggableWindow"/> if found, null if not.</returns>
        public DraggableWindow? TryGetWindowByKey(string windowKey)
        {
            foreach (OpenWindowContainer container in _openWindows.Values)
            {
                if (container.WindowKey == windowKey)
                    return container.Window;
            }

            return null;
        }

        /// <summary>
        /// Tries to find an open <see cref="DraggableWindow"/> by its handle.
        /// </summary>
        /// <param name="handle">The handle of the target window.</param>
        /// <returns>The window if it exists, null if not.</returns>
        public DraggableWindow? TryGetWindowByHandle(uint handle)
        {
            if (_openWindows.TryGetValue(handle, out OpenWindowContainer container))
                return container.Window;

            return null;
        }

        /// <summary>
        /// Close a window.
        /// </summary>
        /// <param name="handle">The handle of the window to close.</param>
        public void CloseWindow(uint handle)
        {
            // get target window
            DraggableWindow? window = TryGetWindowByHandle(handle);

            // window does not exist
            if (window == null)
                return;

            // remove window from record
            _openWindows.Remove(handle);

            // uh oh stinky
            window.NotifyOfClosure();

            // destroy window object
            Destroy(window.gameObject);
        }
    }
}