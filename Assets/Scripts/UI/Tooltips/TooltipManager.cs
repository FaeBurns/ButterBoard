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
        private TooltipMode _mode = TooltipMode.HOVER;
        private bool _isOverriden;
        
        private Vector2 _initialMousePosition = Vector2.zero;
        private Tooltip[] _previousTooltips = Array.Empty<Tooltip>();

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
        /// Disables normal tooltip generation and sets the current tooltip.
        /// </summary>
        /// <param name="tooltip">The tooltip to display.</param>
        /// <param name="mode">The mode used to describe how the tooltip is handled.</param>
        public void SetTooltipOverride(string tooltip, TooltipMode mode = TooltipMode.HOVER)
        {
            SetActiveTooltip(tooltip);
            _isOverriden = true;
            _mode = mode;
        }

        /// <summary>
        /// Clears the currently displaying tooltip if there is one.
        /// </summary>
        public void ClearActiveTooltip()
        {
            // exit if already clear
            if (!IsDisplaying)
                return;

            _isOverriden = false;
            _mode = TooltipMode.HOVER;

            tooltipObject.SetActive(false);
            tooltipTextField.SetText(String.Empty);
        }

        protected override void Awake()
        {
            base.Awake();
            ReferenceResolver.ResolveReferences(this);
        }

        private void Update()
        {
            // is tooltip set manually by something else
            if (_isOverriden)
            {
                // if tooltip mode requires mouse to be pressed, check if mouse is up and clear and exit if true
                if (_mode == TooltipMode.HELD && !Input.GetMouseButton(0))
                {
                    ClearActiveTooltip();
                    return;
                }
                
                UpdateTooltipPosition();
                return;
            }

            // if tooltip mode requires mouse to be pressed, check if mouse is up and clear and exit if true
            if (_mode == TooltipMode.HELD && !Input.GetMouseButton(0))
            {
                ClearActiveTooltip();
                
                if (_previousTooltips.Length > 0)
                    _previousTooltips = Array.Empty<Tooltip>();
                
                return;
            }
            
            // get tooltip host under cursor
            ITooltipHost? tooltipHost = CheckForTooltips();

            // if nothing was found
            if (tooltipHost == null)
            {
                // if currently displaying tooltips, update display
                // allows for tooltips to continue displaying for a short distance after leaving the discovery range
                if (_previousTooltips.Length > 0)
                    _previousTooltips = Array.Empty<Tooltip>();
                
                UpdateTooltipPosition();
                return;
            }

            // get all tooltips on host
            Tooltip[] discoveredTooltips = tooltipHost.GetTooltips().ToArray();

            // update display as a change might have occured
            // don't check first and filter, check happens inside UpdateTooltipDisplay - 
            UpdateTooltipDisplay(discoveredTooltips);

            _previousTooltips = discoveredTooltips;

            UpdateTooltipPosition();
        }

        private ITooltipHost? CheckForTooltips()
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
                ITooltipHost tooltipHost = raycastResult.gameObject.GetComponent<ITooltipHost>();
                if (tooltipHost != null)
                    return tooltipHost;
            }

            return null;
        }

        private void UpdateTooltipDisplay(Tooltip[] tooltips)
        {
            // if no tooltips were found, remove any that may be currently shown and exit
            if (tooltips.Length == 0)
            {
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

            // skip setting tooltip only if it has changed
            if (tooltipMessage != ActiveTooltipText)
                SetActiveTooltip(tooltipMessage);
            
            // update initial mouse position
            // otherwise tooltip flickers while moving
            _initialMousePosition = Input.mousePosition;
        }

        private void UpdateTooltipPosition()
        {
            // exit if no tooltip displaying
            if (!IsDisplaying)
                return;

            // update tooltip position
            tooltipObject.transform.position = Input.mousePosition;
            
            // if in held mode, don't need to check to see if movement has occured
            if (_mode == TooltipMode.HELD)
                return;
            
            // check if tooltip is in allowed distance from initial mouse position
            // if not disable and exit
            float distance = Vector2.Distance(_initialMousePosition, Input.mousePosition);
            if (distance > mouseDistanceThreshold)
            {
                ClearActiveTooltip();
            }
        }
    }
        
    /// <summary>
    /// An enum that describes how the <see cref="TooltipManager"/> handles tooltips set through <see cref="TooltipManager.SetTooltipOverride"/>.
    /// </summary>
    public enum TooltipMode
    {
        HOVER,
        HELD,
    }
}