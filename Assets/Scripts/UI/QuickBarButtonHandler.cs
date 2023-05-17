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
            if (_exitDialogWindow == null)
                _exitDialogWindow = DialogWindow.CreateWindow();
            _exitDialogWindow.BringToFront();

            TimeSpan timeSinceLastSave = DateTime.Now - SaveLoadManager.Instance.LastSaveTime;
            string lastSaveString = ToReadableString(timeSinceLastSave, 5);

            if (String.IsNullOrEmpty(SaveLoadManager.Instance.LastLoadedFilePath))
                lastSaveString = "never";

            _exitDialogWindow.SetDialog($"Are you sure you want to exit?\nLast save {lastSaveString}.", "Are you sure?", ExitButtonResult, "Exit", "Cancel");
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
            if (_resetDialogWindow == null)
                _resetDialogWindow = DialogWindow.CreateWindow();

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

        private string ToReadableString(TimeSpan span, float justNowSecondsThreshold)
        {
            // https://gist.github.com/Rychu-Pawel/fefb89e21b764e97e4993ff517ff0129

            if (span.TotalSeconds < justNowSecondsThreshold)
                return "just now";

            return span switch
            {
                { TotalDays: > 1 } => $"{span.Days:0} {PluralizeString("day", span.Days)} ago",
                { TotalHours: > 1 } => $"{span.Hours:0} {PluralizeString("hour", span.Hours)} ago",
                { Minutes: > 1 } => $"{span.Minutes:0} {PluralizeString("minute", span.Minutes)} ago",
                _ => $"{span.Seconds:0} {PluralizeString("second", span.Days)} ago",
            };
        }

        private string PluralizeString(string baseString, int amount)
        {
            if (amount == 1)
                return baseString;

            if (baseString.EndsWith('s'))
                return baseString + "es";

            return baseString + "s";
        }
    }
}