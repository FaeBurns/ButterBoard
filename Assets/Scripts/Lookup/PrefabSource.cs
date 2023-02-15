using System;
using System.Collections.Generic;
using BeanCore.Unity.ReferenceResolver;
using UnityEditor;
using UnityEngine;

namespace ButterBoard.Lookup
{
    [ExecuteInEditMode]
    [DefaultExecutionOrder(-100)]
    public class PrefabSource : MonoBehaviour
    {
        private void Awake()
        {
            ReferenceStore.RegisterReference(this);
        }

        [SerializeField]
        private List<PrefabEntry> prefabs = new List<PrefabEntry>();

        private readonly Dictionary<string, GameObject> _mappedPrefabs = new Dictionary<string, GameObject>();

        private void OnValidate()
        {
            _mappedPrefabs.Clear();

            foreach (PrefabEntry entry in prefabs)
            {
                _mappedPrefabs.Add(entry.key, entry.prefab);
            }
        }

        public GameObject Fetch(string key)
        {
            return _mappedPrefabs[key];
        }
    }
}