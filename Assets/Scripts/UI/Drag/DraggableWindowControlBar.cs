using System;
using BeanCore.Unity.ReferenceResolver.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace ButterBoard.UI.Drag
{
    public class DraggableWindowControlBar : MonoBehaviour
    {
        private string _title = String.Empty;

        [SerializeField]
        private TextMeshProUGUI titleLabel = null!;

        [SerializeField]
        private DraggableWindowDragHandle movementHandle = null!;

        /// <summary>
        /// Gets or Sets the window that hosts this control bar.
        /// </summary>
        [field: SerializeField]
        public DraggableWindow HostWindow { get; set; } = null!;

        /// <summary>
        /// Gets or Sets the title text of the control bar.
        /// </summary>
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                titleLabel.text = _title;
            }
        }

        private void Awake()
        {
            if (HostWindow == null)
                throw new InvalidOperationException($"{nameof(DraggableWindowControlBar)} must have the property {nameof(HostWindow)} set in the inspector before it can function");

            movementHandle.TargetTransform = HostWindow.GetComponent<RectTransform>();
        }

        public void OnCloseButtonClick()
        {
            HostWindow.Close();
        }
    }
}