using BeanCore.Unity.ReferenceResolver;
using BeanCore.Unity.ReferenceResolver.Attributes;
using ButterBoard.FloatingGrid.Placement;
using ButterBoard.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ButterBoard.UI.Rack
{
    public class RackEntryDisplay : MonoBehaviour
    {
        private RackEntryAsset _currentEntryAsset = null!;

        [SerializeField]
        private Image thumbnailImage = null!;

        [SerializeField]
        private TMP_Text textHost = null!;

        [SerializeField]
        private Sprite fallbackSprite = null!;

        [BindComponent(Child = true)]
        private ConstantTooltipHost tooltipHost = null!;

        public void SetDisplayEntry(RackEntryAsset entryAsset)
        {
            ReferenceResolver.ResolveReferences(this);

            _currentEntryAsset = entryAsset;
            textHost.SetText(entryAsset.DisplayName);

            if (entryAsset.Sprite == null)
            {
                Debug.Log("Sprite on rack entry is not set");
                thumbnailImage.sprite = fallbackSprite;
            }
            else
            {
                thumbnailImage.sprite = entryAsset.Sprite;
            }

            if (!string.IsNullOrEmpty(entryAsset.Description))
                tooltipHost.tooltip = entryAsset.Description;
        }

        public void TryBeginPlacement()
        {
            // cancel any placement in progress
            if (PlacementManager.Instance.Placing)
                PlacementManager.Instance.Cancel();

            PlacementManager.Instance.BeginPlace(_currentEntryAsset.SpawnTargetSourceKey);
        }
    }
}