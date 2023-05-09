using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeanCore.Unity.ReferenceResolver;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ButterBoard.UI.Tooltips
{
    /// <summary>
    /// Manages displaying a tooltip to the user.
    /// </summary>
    public class TooltipManager : SingletonBehaviour<TooltipManager>
    {
        private Vector2 _initialMousePosition = Vector2.zero;
        private List<Tooltip> _previousTooltips = new List<Tooltip>();

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

        private void Awake()
        {
            ReferenceResolver.ResolveReferences(this);
        }

        private void Update()
        {
            // get tooltip host under cursor
            TextTooltipHost? tooltipHost = CheckForTooltips();

            // if nothing was found
            if (tooltipHost == null)
            {
                // clear all displaying tooltips
                _previousTooltips = new List<Tooltip>();
                ClearActiveTooltip();
                return;
            }

            // get all tooltips on host
            List<Tooltip> discoveredTooltips = GetTooltips(tooltipHost).ToList();

            // if previous and this frame's tooltip collections are of the same length, a change might have occured
            if (discoveredTooltips.Count == _previousTooltips.Count)
            {
                // if the match is while they're both empty just exit early
                if (discoveredTooltips.Count == 0)
                    return;

                UpdateTooltipDisplay(discoveredTooltips);
            }
            // if they are of different lengths, a change is guaranteed
            else
            {
                UpdateTooltipDisplay(discoveredTooltips);
            }

            _previousTooltips = discoveredTooltips;

            UpdateTooltipPosition();
        }

        private TextTooltipHost? CheckForTooltips()
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition,
                pointerId = -1,
            };

            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);

            foreach (RaycastResult raycastResult in raycastResults)
            {
                TextTooltipHost tooltipHost = raycastResult.gameObject.GetComponent<TextTooltipHost>();
                if (tooltipHost != null)
                    return tooltipHost;
            }

            return null;
        }

        private IEnumerable<Tooltip> GetTooltips(TextTooltipHost tooltipHost)
        {
            Vector3 mousePosition = Input.mousePosition;

            // should probably check if mouse is inside window rect first
            // should probably also not do this every frame

            // check against programInputField.textComponent rather than programDisplayField to make sure it does not include rtf tags

            // if camera is provided it will search for world text
            int intersectingLine = TMP_TextUtilities.FindIntersectingLine(tooltipHost.TextComponent, mousePosition, null!);
            if (intersectingLine == -1)
                return new List<Tooltip>();

            int intersectingCharacter = TMP_TextUtilities.FindIntersectingCharacter(tooltipHost.TextComponent, mousePosition, null!, true);
            if (intersectingCharacter == -1)
                return new List<Tooltip>();

            return tooltipHost.TooltipCollection.FindTooltipsAtPosition(intersectingLine, intersectingCharacter);
        }

        private void UpdateTooltipDisplay(List<Tooltip> tooltips)
        {
            // if no tooltips were found, remove any that may be currently shown and exit
            if (tooltips.Count == 0)
            {
                ClearActiveTooltip();
                return;
            }

            StringBuilder tooltipTextBuilder = new StringBuilder();
            int index = 0;
            foreach (Tooltip tooltip in tooltips)
            {
                if (index > 0)
                    tooltipTextBuilder.AppendLine();
                tooltipTextBuilder.Append(tooltip.message);
                index++;
            }

            // compile tooltip
            string tooltipMessage = tooltipTextBuilder.ToString();

            // skip setting tooltip if it has not changed
            if (tooltipMessage == ActiveTooltipText)
                return;

            SetActiveTooltip(tooltipMessage);
        }

        private void UpdateTooltipPosition()
        {
            // exit if no tooltip displaying
            if (!IsDisplaying)
                return;

            // check if tooltip is in allowed distance from initial mouse position
            // if not disable and exit
            float distance = Vector2.Distance(_initialMousePosition, Input.mousePosition);
            if (distance > mouseDistanceThreshold)
            {
                ClearActiveTooltip();
                return;
            }

            // update tooltip position
            tooltipObject.transform.position = Input.mousePosition;
        }
    }
}