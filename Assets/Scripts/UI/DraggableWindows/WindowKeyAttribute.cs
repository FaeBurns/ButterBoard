using System;

namespace ButterBoard.UI.DraggableWindows
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class WindowKeyAttribute : Attribute
    {
        /// <summary>
        /// Gets the key of the window.
        /// </summary>
        public string Key { get; }

        public WindowKeyAttribute(string key)
        {
            Key = key;
        }
    }
}