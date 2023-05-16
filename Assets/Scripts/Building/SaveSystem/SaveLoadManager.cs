using System;
using System.Collections.Generic;
using System.IO;
using ButterBoard.FloatingGrid.Placement.Placeables;
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

            IEnumerable<PlaceableSaveData> placeableSaveData = SaveIndividualData();
            SaveData saveData = new SaveData(actions, placeableSaveData)
            {
                CameraPosition = Camera.main!.transform.position,
                CameraZoom = Camera.main!.GetComponent<CameraController>().Zoom,
            };

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

            // load data for individual ISaveLoadListener<> types
            LoadIndividualData(saveData);

            Camera.main!.transform.position = new Vector3(saveData.CameraPosition.x, saveData.CameraPosition.y, -100);
            Camera.main!.GetComponent<CameraController>().Zoom = saveData.CameraZoom;
        }

        private static void LoadIndividualData(SaveData saveData)
        {
            foreach (PlaceableSaveData placeableSaveData in saveData.ObjectSaveData)
            {
                BasePlaceable placeable = BuildManager.GetPlaceable(placeableSaveData.PlaceableKey);

                // get all child components implementing the listener
                ISaveLoadListener[] components = placeable.GetComponentsInChildren<ISaveLoadListener>();

                // loop through all of them
                foreach (Component component in components)
                {
                    // skip them if they are not direct children of the searching placeable - e.g GridPlaceable on FloatingPlaceable
                    if (component.GetComponentInParent<BasePlaceable>() == placeable)
                    {
                        // load data
                        // ReSharper disable once SuspiciousTypeConversion.Global
                        ISaveLoadListener listener = (ISaveLoadListener)component;
                        listener.Load(placeableSaveData);
                    }
                }
            }
        }

        private static List<PlaceableSaveData> SaveIndividualData()
        {
            List<PlaceableSaveData> result = new List<PlaceableSaveData>();
            
            // keep track of those that have already been saved as searching through all children can cause some to show up multiple times
            HashSet<ISaveLoadListener> alreadySavedComponents = new HashSet<ISaveLoadListener>();
            foreach (BasePlaceable placeable in BuildManager.GetAllRegisteredPlaceables())
            {
                ISaveLoadListener[] components = placeable.GetComponentsInChildren<ISaveLoadListener>();

                foreach (ISaveLoadListener saveLoadListener in components)
                {
                    // skip if not a direct child
                    if(((Component)saveLoadListener).GetComponentInParent<BasePlaceable>() != placeable)
                        continue;
                    
                    // skip if already saved
                    if (alreadySavedComponents.Contains(saveLoadListener))
                        continue;
    
                    PlaceableSaveData saveData = saveLoadListener.Save();
                    saveData.PlaceableKey = placeable.Key;
                    result.Add(saveData);

                    alreadySavedComponents.Add(saveLoadListener);
                }
            }

            return result;
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