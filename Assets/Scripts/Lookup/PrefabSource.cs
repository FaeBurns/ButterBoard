using System;
using System.Collections.Generic;
using UnityEngine;

namespace ButterBoard.Lookup
{
    [ExecuteInEditMode]
    [DefaultExecutionOrder(-100)]
    public class PrefabSource : MonoBehaviour
    {
        [SerializeField]
        private List<PrefabEntry> prefabs = new List<PrefabEntry>();

        private static Dictionary<string, GameObject> _mappedPrefabs = new Dictionary<string, GameObject>();

        private void OnValidate()
        {
            _mappedPrefabs.Clear();

            foreach (PrefabEntry entry in prefabs)
            {
                _mappedPrefabs.Add(entry.key, entry.prefab);
            }
        }

        public static GameObject Fetch(string key)
        {
            return _mappedPrefabs[key];
        }
    }
}