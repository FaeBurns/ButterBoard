using System;
using UnityEngine;

namespace ButterBoard.UI.Rack
{
    [CreateAssetMenu(fileName = "Rack Order", menuName = "Assets/Rack/Rack Order", order = 2)]
    public class RackOrderAsset : ScriptableObject
    {
        [field: SerializeField]
        public RackCategoryAsset[] Categories { get; private set; } = Array.Empty<RackCategoryAsset>();
    }
}