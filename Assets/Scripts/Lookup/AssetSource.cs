using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ButterBoard.Lookup
{
    public static class AssetSource
    {
        private static bool _initialized = false;
        private static readonly Dictionary<string, SearchableAssetSource> _assetMapping = new Dictionary<string, SearchableAssetSource>();

        public static void Init()
        {
            if (_initialized)
                throw new InvalidOperationException("Already Initialized");

            SearchableAssetSource[] assetSources = Resources.LoadAll<SearchableAssetSource>("");

            foreach (SearchableAssetSource assetSource in assetSources)
            {
                _assetMapping.Add(assetSource.key.ToLower(), assetSource);
            }

            _initialized = true;
        }

        public static void Add(string key, SearchableAssetSource assetSource)
        {
            _assetMapping.Add(key.ToLower(), assetSource);
        }

        public static void Replace(string key, SearchableAssetSource assetSource)
        {
            _assetMapping[key.ToLower()] = assetSource;
        }

        public static T Fetch<T>(string key)
            where T : Object
        {
            Verify();
            return (T)Fetch(key.ToLower());
        }

        public static Object Fetch(string key)
        {
            Verify();
            return _assetMapping[key.ToLower()].Fetch();
        }

        public static SearchableAssetSource FetchSource(string key)
        {
            Verify();
            return _assetMapping[key.ToLower()];
        }

        private static void Verify()
        {
            if (_initialized)
                return;

            Init();
        }
    }
}