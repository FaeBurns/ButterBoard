using UnityEngine;
using UnityEngine.SceneManagement;

namespace ButterBoard.Building.SaveSystem
{
    public class LoadTrigger : MonoBehaviour
    {
        public SaveData SaveData { get; set; } = null!;
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SaveLoadManager.Instance.LoadImmediate(SaveData);
            Destroy(gameObject);
        }
    }
}