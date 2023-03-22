using ButterBoard.FloatingGrid.Placement;
using ButterBoard.Lookup;
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

        public void SetDisplayEntry(RackEntryAsset entryAsset)
        {
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
        }

        public void TryBeginPlacement()
        {
            // cancel any placement in progress
            if (PlacementManager.Instance.Placing)
                PlacementManager.Instance.Cancel();

            if (PlacementLimitManager.CanPlace(_currentEntryAsset))
                PlacementManager.Instance.BeginPlace(_currentEntryAsset.SpawnTargetSourceKey);
        }
    }
}