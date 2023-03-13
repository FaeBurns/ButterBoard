using UnityEngine;

namespace ButterBoard.Lookup
{
    [CreateAssetMenu(fileName = "New Asset Folder", menuName = "Assets/New Asset Folder", order = 1)]
    public class SearchableAssetFolder : ScriptableObject
    {
        [SerializeField]
        public string folderKey = "";

        [SerializeField]
        public string path = "";
    }
}