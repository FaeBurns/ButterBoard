using System;
using System.Collections.Generic;
using System.Reflection;
using BeanCore.Unity.ReferenceResolver;
using ButterBoard.Lookup;
using UnityEngine;
using UnityEngine.UI;

namespace ButterBoard.UI.DraggableWindows
{
    /// <summary>
    /// A base class for all windows that can be opened via a static method. Child classes require the <see cref="WindowKeyAttribute"/> in order to define the prefab to spawn.
    /// </summary>
    /// <typeparam name="T">The type of the window to open.</typeparam>
    public class CreatableWindow<T> : Window
        where T : CreatableWindow<T>
    {
        public static T CreateWindow()
        {
            // try and get key for creating window object
            WindowKeyAttribute? keyAttribute = typeof(T).GetCustomAttribute<WindowKeyAttribute>();
            if (keyAttribute == null)
                throw new ArgumentException($"Child class of {nameof(CreatableWindow<T>)} must have a {nameof(WindowKeyAttribute)} attribute");

            // try and get prefab for window
            GameObject? windowPrefab = AssetSource.Fetch<GameObject>(keyAttribute.Key);
            if (windowPrefab == null)
                throw new KeyNotFoundException($"Could not find window to create with key {keyAttribute.Key}");

            Transform windowHostTransform = WindowHost.Instance.transform;

            // create new window
            GameObject windowObject = Instantiate(windowPrefab, windowHostTransform);
            RectTransform windowTransform = windowObject.GetComponent<RectTransform>();

            // set the position of the window and ensure it's the topmost window
            Vector2 canvasScale = WindowHost.Instance.HostCanvas.transform.localScale;
            Vector2 canvasCenter = (WindowHost.Instance.HostCanvas.renderingDisplaySize / 2) / canvasScale;
            Vector2 windowOffset = windowTransform.sizeDelta / 2;
            Vector2 canvasPlacementPosition = canvasCenter - windowOffset;
            canvasPlacementPosition = new Vector2(canvasPlacementPosition.x, -canvasPlacementPosition.y); // negate y - otherwise shows above canvas
            windowTransform.anchoredPosition = canvasPlacementPosition;

            // get the window, resolve references, and call open
            T result = windowObject.GetComponent<T>();
            ReferenceResolver.ResolveReferences(result);
            result.Open();

            // return the result window class
            return result;
        }
    }
}