using System;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace ButterBoard.Lookup
{
    [CreateAssetMenu(fileName = "New Asset Source", menuName = "Assets/New Asset Source", order = 0)]
    public class SearchableAssetSource : ScriptableObject
    {
        private void Awake()
        {
            if (string.IsNullOrEmpty(key))
            {
                key = name;
            }
        }

        [SerializeField]
        public string key = "";

        [SerializeField]
        private Object storedObject = null!;

        public Object Fetch()
        {
            return storedObject;
        }
    }
}