using ButterBoard.UI.Windows;
using UnityEngine;

namespace ButterBoard.UI
{
    public class QuickBarButtonHandler : MonoBehaviour
    {
        private SimulationManagerWindow? _simulationManagerWindow;

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
            
        }
    }
}