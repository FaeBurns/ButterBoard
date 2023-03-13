using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ButterBoard.Lookup
{
    public static class AssetSource
    {
        private static bool _initialized = false;
        private static readonly Dictionary<string, SearchableAssetSource> _assetMapping = new Dictionary<string, SearchableAssetSource>();
        private static readonly Dictionary<string, Object[]> _folderAssetMapping = new Dictionary<string, Object[]>();

        public static void Init()
        {
            if (_initialized)
                throw new InvalidOperationException("Already Initialized");

            // get all searchable assets
            SearchableAssetSource[] assetSources = Resources.LoadAll<SearchableAssetSource>("");

            // add all to asset mapping
            foreach (SearchableAssetSource assetSource in assetSources)
            {
                _assetMapping.Add(assetSource.key.ToLower(), assetSource);
            }

            // get all searchable folders
            SearchableAssetFolder[] assetFolders = Resources.LoadAll<SearchableAssetFolder>("");

            // get all assets in searchable folders
            foreach (SearchableAssetFolder folder in assetFolders)
            {
                List<Object> folderContents = Resources.LoadAll(folder.path).ToList();

                // remove the folder from the found assets
                folderContents.Remove(folder);

                // add to asset mapping
                _folderAssetMapping.Add(folder.folderKey.ToLower(), folderContents.ToArray());
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

        public static T? Fetch<T>(string key)
            where T : Object
        {
            return Fetch(key) as T;
        }

        public static Object? Fetch(string key)
        {
            Verify();

            // ReSharper disable once ConvertIfStatementToReturnStatement
            // return null if not in mapping
            if (!_assetMapping.ContainsKey(key.ToLower()))
                return null;

            return _assetMapping[key.ToLower()].Fetch();
        }

        public static SearchableAssetSource FetchSource(string key)
        {
            Verify();
            return _assetMapping[key.ToLower()];
        }

        public static T[] FetchFolderContents<T>(string folderKey)
            where T : Object
        {
            Object[] rawResults = FetchFolderContents(folderKey);

            List<T> result = new List<T>(rawResults.Length);
            foreach (Object obj in rawResults)
            {
                if (obj is T tObj)
                    result.Add(tObj);
            }

            return result.ToArray();
        }

        public static Object[] FetchFolderContents(string folderKey)
        {
            Verify();

            return _folderAssetMapping[folderKey.ToLower()];
        }

        private static void Verify()
        {
            if (_initialized)
                return;

            Init();
        }
    }
}