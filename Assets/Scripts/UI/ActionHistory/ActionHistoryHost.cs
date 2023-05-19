using System;
using System.Collections.Generic;
using ButterBoard.Lookup;
using UnityEngine;

namespace ButterBoard.UI.ActionHistory
{
    public class ActionHistoryHost : SingletonBehaviour<ActionHistoryHost>
    {
        private LinkedList<GameObject> _hostingEntryObjects = new LinkedList<GameObject>();

        [SerializeField]
        private string prefabKey = String.Empty;

        [SerializeField]
        private int maxEntries = 5;

        private void Start()
        {
            Application.logMessageReceived += OnLogMessageReceived;
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
        }

        private void OnLogMessageReceived(string logString, string stacktrace, LogType type)
        {
            // skip if level is too low
            if (type == LogType.Log || type == LogType.Warning)
                return;
            
            // otherwise display issue
            PushMessage(logString);
        }

        public void PushMessage(string message, float lifetimeSeconds = 5)
        {
            // create new instance of message object
            GameObject prefab = AssetSource.Fetch<GameObject>(prefabKey)!;
            GameObject messageEntryObject = Instantiate(prefab, transform);
            
            // initialize with message
            ActionHistoryEntry entry = messageEntryObject.GetComponent<ActionHistoryEntry>();
            entry.InitializeMessage(message, 1 / lifetimeSeconds);

            // keep track and immediately destroy oldest one if there are too many 
            _hostingEntryObjects.AddLast(entry.gameObject);
            if (_hostingEntryObjects.Count > maxEntries)
            {
                GameObject removingEntry = _hostingEntryObjects.First.Value;
                _hostingEntryObjects.RemoveFirst();
                
                // if not already destroyed, remove immediately
                if (removingEntry != null)
                    Destroy(removingEntry);
            }

            // set to destroy in x seconds
            Destroy(messageEntryObject, lifetimeSeconds);
        }
    }
}