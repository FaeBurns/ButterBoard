using System;
using UnityEngine;

namespace ButterBoard.UI.Rack
{
    [CreateAssetMenu(fileName = "Rack Category", menuName = "Assets/Rack/New Rack Category", order = 1)]
    public class RackCategoryAsset : ScriptableObject
    {
        [field: SerializeField]
        public string DisplayName { get; private set; } = "";

        [field: SerializeField]
        public RackEntryAsset[] CategoryEntries { get; private set; } = Array.Empty<RackEntryAsset>();
    }
}