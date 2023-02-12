using System;
using UnityEngine;

namespace ButterBoard.Lookup
{
    [Serializable]
    public struct PrefabEntry
    {
        public string key;

        public GameObject prefab;
    }
}