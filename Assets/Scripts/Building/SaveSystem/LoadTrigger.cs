using UnityEngine;
using UnityEngine.SceneManagement;

namespace ButterBoard.Building.SaveSystem
{
    public class LoadTrigger : MonoBehaviour
    {
        public SaveData SaveData { get; set; } = null!;
        public string SourceFilePath { get; set; } = null!;
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SaveLoadManager.Instance.LoadImmediate(SaveData, SourceFilePath);
            Destroy(gameObject);
        }
    }
}