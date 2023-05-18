using System;
using System.Collections.Generic;
using ButterBoard.Building;
using UnityEngine;

namespace ButterBoard.FloatingGrid.Placement.Placeables
{
    public class FloatingPlaceable : BasePlaceable
    {
        private List<GridHost> _gridHosts = new List<GridHost>();
        
        [field: SerializeField]
        public SpriteTintHelper MainSpriteTintHelper { get; private set; }= null!;

        public override void DisplayPlacementStatus(string statusMessage, bool isOk)
        {
            Color targetColor = isOk ? Color.green : Color.red;
            MainSpriteTintHelper.SetTint(targetColor);
        }

        public override void ClearPlacementStatus()
        {
            MainSpriteTintHelper.RestoreColor();
        }

        private void Awake()
        {
            _gridHosts.AddRange(GetComponentsInChildrenGuaranteeOrder<GridHost>(transform));
            
            Place.AddListener(PlaceListener);
            Remove.AddListener(RemoveListener);
        }

        private T[] GetComponentsInChildrenGuaranteeOrder<T>(Transform baseTransform)
        {
            List<T> result = new List<T>();
            int childCount = baseTransform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                result.AddRange(GetComponentsInChildrenGuaranteeOrder<T>(baseTransform.GetChild(i)));
            }
            
            result.AddRange(baseTransform.GetComponents<T>());
            
            return result.ToArray();
        }

        private void PlaceListener()
        {
            foreach (GridHost gridHost in _gridHosts)
            {
                BuildManager.RegisterGridHost(gridHost);
            }
        }

        private void RemoveListener()
        {
            // loop through in reverse
            for (int i = _gridHosts.Count - 1; i >= 0; i--)
            {
                BuildManager.RemoveRegisteredGridHost(_gridHosts[i].Key);
            }
        }
    }
}