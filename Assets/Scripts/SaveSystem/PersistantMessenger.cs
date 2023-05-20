using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ButterBoard.SaveSystem
{
    public class PersistantMessenger : MonoBehaviour
    {
        /// <summary>
        /// Gets or Sets the action that is executed once a new scene has been loaded.
        /// </summary>
        public Action? OnLoadAction { get; set; }
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Destroy(gameObject);
            
            OnLoadAction?.Invoke();
        }

        public static void CreateInstance(Action onLoadAction)
        {
            new GameObject().AddComponent<PersistantMessenger>().OnLoadAction = onLoadAction;
        }
    }
}