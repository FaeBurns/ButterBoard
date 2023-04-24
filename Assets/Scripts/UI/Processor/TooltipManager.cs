using System;
using TMPro;
using UnityEngine;

namespace ButterBoard.UI.Processor
{
    /// <summary>
    /// Manages displaying a tooltip to the user.
    /// </summary>
    public class TooltipManager : SingletonBehaviour<TooltipManager>
    {
        private Vector2 _initialMousePosition = Vector2.zero;

        [SerializeField]
        private GameObject tooltipObject = null!;

        [SerializeField]
        private TextMeshProUGUI tooltipTextField = null!;

        [SerializeField]
        private float mouseDistanceThreshold = 5;

        /// <summary>
        /// Gets a value indicating whether or not there is a tooltip currently displaying.
        /// </summary>
        public bool IsDisplaying => ActiveTooltipText != String.Empty;

        /// <summary>
        /// Gets the text of the currently displaying tooltip.
        /// </summary>
        public string ActiveTooltipText => tooltipTextField.text;

        /// <summary>
        /// Sets the displaying tooltip.
        /// </summary>
        /// <param name="tooltip">The new tooltip to display</param>
        public void SetActiveTooltip(string tooltip)
        {
            if(IsDisplaying)
                ClearActiveTooltip();

            // set tooltip text and show
            tooltipTextField.SetText(tooltip);
            tooltipObject.SetActive(true);

            // record mouse position
            _initialMousePosition = Input.mousePosition;
        }

        /// <summary>
        /// Clears the currently displaying tooltip if there is one.
        /// </summary>
        public void ClearActiveTooltip()
        {
            // exit if already clear
            if (!IsDisplaying)
                return;

            tooltipObject.SetActive(false);
            tooltipTextField.SetText(String.Empty);
        }

        private void Update()
        {
            // check if tooltip is in allowed distance from initial mouse position
            // if not disable and exit
            float distance = Vector2.Distance(_initialMousePosition, Input.mousePosition);
            if (distance > mouseDistanceThreshold)
            {
                tooltipTextField.SetText(String.Empty);
                tooltipObject.SetActive(false);
                return;
            }

            // update tooltip position
            tooltipObject.transform.position = Input.mousePosition;
        }
    }
}