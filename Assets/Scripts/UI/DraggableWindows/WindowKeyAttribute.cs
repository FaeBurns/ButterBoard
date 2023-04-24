using System;

namespace ButterBoard.UI.DraggableWindows
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class WindowKeyAttribute : Attribute
    {
        public string Key { get; }

        // See the attribute guidelines at
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        public WindowKeyAttribute(string key)
        {
            Key = key;
        }
    }
}