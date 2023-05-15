using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ButterBoard.Building.SaveSystem
{
    public class SaveLoadManager : SingletonBehaviour<SaveLoadManager>
    {
        [SerializeField]
        private string mainSceneName = String.Empty;

        [SerializeField]
        private string debugSaveLoadPath = String.Empty;
        
        public string LastLoadedFilePath { get; private set; } = String.Empty;
        
        public void Save(string fileName)
        {
            string directoryName = Path.GetDirectoryName(fileName)!;
            if (!Directory.Exists(directoryName))
                throw new DirectoryNotFoundException($"Directory {directoryName} could not be found");

            List<BuildAction> actions = BuildActionManager.Instance.GetLifetimeActions();

            SaveData saveData = new SaveData(actions);

            string jsonData = JsonConvert.SerializeObject(saveData, Formatting.Indented, GetJsonSettings());
            File.WriteAllText(fileName, jsonData);
        }

        public void Load(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("Could not load file, file does not exist", fileName);
            
            SaveData? saveData = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(fileName), GetJsonSettings());
            
            // ReSharper disable once JoinNullCheckWithUsage
            if (saveData == null)
                throw new FileLoadException("File contained invalid data and could not be loaded", fileName);
            
            LastLoadedFilePath = fileName;

            new GameObject().AddComponent<LoadTrigger>().SaveData = saveData;

            SceneManager.LoadScene(mainSceneName);
        }

        public void LoadImmediate(SaveData saveData)
        {
            BuildManager.ResetRegistry();

            foreach (BuildAction buildAction in saveData.SavedBuildActions)
            {
                // use BuildActionManager - still need to keep track of these ones for the next save
                BuildActionManager.Instance.PushAndExecuteAction(buildAction);
            }
            
            // clear undo and redo stack - don't want to be able to undo loaded actions
            BuildActionManager.Instance.ClearUndoStack();
            
            // make placement possible
            BuildManager.UpdateNextPlaceableId();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.J))
                Save(debugSaveLoadPath);
            
            else if (Input.GetKeyDown(KeyCode.K))
            {
                Load(debugSaveLoadPath);
            }
        }
        
        private JsonSerializerSettings GetJsonSettings()
        {
            return new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                Converters =
                {
                    new Vector2JsonConverter(),
                },
            };
        }
    }
}