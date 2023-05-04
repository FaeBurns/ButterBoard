using System;
using BeanCore.Unity.ReferenceResolver;
using BeanCore.Unity.ReferenceResolver.Attributes;
using TMPro;
using UnityEngine;

namespace ButterBoard.UI.DraggableWindows
{
    public class WindowControlBar : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI titleLabel = null!;

        [SerializeField]
        private WindowDragHandle movementHandle = null!;

        /// <summary>
        /// Gets the window that hosts this control bar.
        /// </summary>
        [BindComponent(Parent = true)]
        public Window HostWindow { get; private set; } = null!;

        /// <summary>
        /// Gets or Sets the title text of the control bar.
        /// </summary>
        public string Title
        {
            get => titleLabel.text;
            set => titleLabel.text = value;
        }

        private void Awake()
        {
            ReferenceResolver.ResolveReferences(this);

            movementHandle.WindowTransform = HostWindow.GetComponent<RectTransform>();
        }

        public void OnCloseButtonClick()
        {
            HostWindow.Close();
        }

        public void OnControlBarClicked()
        {
            HostWindow.BringToFront();
        }
    }
}