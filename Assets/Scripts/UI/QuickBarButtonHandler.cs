using System;
using System.IO;
using AnotherFileBrowser.Windows;
using ButterBoard.Building.SaveSystem;
using ButterBoard.UI.Windows;
using UnityEngine;

namespace ButterBoard.UI
{
    public class QuickBarButtonHandler : MonoBehaviour
    {
        private BrowserProperties _browserProperties = null!;
        
        private SimulationManagerWindow? _simulationManagerWindow;
        private DialogWindow? _exitDialogWindow;
        private DialogWindow? _resetDialogWindow;

        public void Awake()
        {
            string basePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            
            _browserProperties = new BrowserProperties()
            {
                title = "",
                filter = "json files (*.json)|*.json",
                initialDir = Path.Combine(basePath, "My Games", "ButterBoard"),
                restoreDirectory = true,
            };

            if (!Directory.Exists(Path.Combine(basePath, "My Games")))
                Directory.CreateDirectory(Path.Combine(basePath, "My Games"));

            if (!Directory.Exists(Path.Combine(basePath, "My Games", "ButterBoard")))
                Directory.CreateDirectory(Path.Combine(basePath, "My Games", "ButterBoard"));
        }

        public void OpenSimulationManagerWindow()
        {
            if (_simulationManagerWindow != null)
            {
                _simulationManagerWindow.BringToFront();
                return;
            }

            _simulationManagerWindow = SimulationManagerWindow.CreateWindow();
        }

        public void ExitButton()
        {
            _exitDialogWindow ??= DialogWindow.CreateWindow();
            _exitDialogWindow.BringToFront();
            _exitDialogWindow.SetDialog("Are you sure you want to exit?", "Are you sure?", ExitButtonResult, "Exit", "Cancel");
        }

        public void SaveButton()
        {
            // if a file is currently loaded
            if (!String.IsNullOrEmpty(SaveLoadManager.Instance.LastLoadedFilePath))
            {
                SaveTo(SaveLoadManager.Instance.LastLoadedFilePath);
                return;
            }
                
            new FileBrowser().SaveFileBrowser(_browserProperties, "save", ".json", SaveTo);
        }

        public void SaveAsButton()
        {
            new FileBrowser().SaveFileBrowser(_browserProperties, "save", ".json", SaveTo);
        }

        public void LoadButton()
        {
            new FileBrowser().OpenFileBrowser(_browserProperties, LoadFrom);
        }

        public void ResetButton()
        {
            _resetDialogWindow ??= DialogWindow.CreateWindow();
            _resetDialogWindow.BringToFront();
            _resetDialogWindow.SetDialog("Are you sure you want to reset? This cannot be undone.", "Are you sure?", ResetButtonResult, "Restart", "Cancel");
        }

        private void SaveTo(string path)
        {
            SaveLoadManager.Instance.Save(path);
        }

        private void LoadFrom(string path)
        {
            SaveLoadManager.Instance.Load(path);
        }

        private void ExitButtonResult(bool result)
        {
            if (!result)
                return;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ResetButtonResult(bool result)
        {
            if (!result)
                return;
            
            SaveLoadManager.Instance.Reset();
        }
    }
}