using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

// just tired of it at this point
#nullable disable
namespace ButterBoard.UI.Rack
{
    public class RackPopulator : MonoBehaviour
    {
        private RackEntryDisplay[] _allEntryDisplays;

        [SerializeField]
        private Transform buttonHost;

        [SerializeField]
        private Transform rackEntryHost;

        [SerializeField]
        private RackOrderAsset rackOrder;

        [SerializeField]
        private GameObject rackEntryDisplayPrefab;

        [SerializeField]
        private GameObject tabButtonPrefab;

        private void Awake()
        {
            int highestEntryCount = 0;

            for (int i = 0; i < rackOrder.Categories.Length; i++)
            {
                // create new button object and set category text
                GameObject newButton = Instantiate(tabButtonPrefab, buttonHost);
                newButton.GetComponentInChildren<TextMeshProUGUI>().SetText(rackOrder.Categories[i].DisplayName);

                // copy to local before adding listener
                int buttonIndex = i;
                newButton.GetComponent<Button>().onClick.AddListener(() => SetCategory(buttonIndex));

                // record highest entry count
                if (rackOrder.Categories[i].CategoryEntries.Length > highestEntryCount)
                    highestEntryCount = rackOrder.Categories[i].CategoryEntries.Length;
            }

            CreateToSize(highestEntryCount);

            // set to first category
            SetCategory(0);
        }

        public void SetCategory(int index)
        {
            SetCategory(rackOrder.Categories[index]);
        }

        public void SetCategory(RackCategoryAsset categoryAsset)
        {
            // also counts the enabled entries
            int index = 0;

            foreach (RackEntryAsset entryAsset in categoryAsset.CategoryEntries)
            {
                // skip if not enabled
                if (!entryAsset.Enabled)
                    continue;

                // set display entry
                _allEntryDisplays[index].SetDisplayEntry(entryAsset);

                // increment index
                index++;
            }

            // disable all unneeded displays
            EnableDisplayRange(index);
        }

        private void CreateToSize(int size)
        {
            // set target to size
            _allEntryDisplays = new RackEntryDisplay[size];

            // create size numbers of entry display objects
            for (int i = 0; i < size; i++)
            {
                GameObject newObject = Instantiate(rackEntryDisplayPrefab, rackEntryHost);
                _allEntryDisplays[i] = newObject.GetComponent<RackEntryDisplay>();
            }
        }

        /// <summary>
        /// Enables the required amount of displays and disables any excess.
        /// </summary>
        /// <param name="desiredCount">The amount needed to be enabled.</param>
        private void EnableDisplayRange(int desiredCount)
        {
            Debug.Assert(desiredCount <= _allEntryDisplays.Length);

            int i = 0;
            foreach (RackEntryDisplay entryDisplay in _allEntryDisplays)
            {
                // set enabled if i is less than the desired count
                // i = 0, desired = 0 - false
                // i = 0, desired = 1 - true
                entryDisplay.gameObject.SetActive(i < desiredCount);

                // inc index
                i++;
            }
        }
    }
}