using UnityEngine;

namespace ButterBoard.UI.Rack
{
    [CreateAssetMenu(fileName = "Rack Entry", menuName = "Assets/Rack/New Rack Entry", order = 0)]
    public class RackEntryAsset : ScriptableObject
    {
        [field: SerializeField]
        public string DisplayName { get; private set; } = "";

        [field: SerializeField]
        public string SpawnTargetSourceKey { get; private set; } = "";

        [field: SerializeField]
        public Sprite Sprite { get; private set; } = null!;

        [field: SerializeField]
        public bool Enabled { get; private set; } = true;
    }

}