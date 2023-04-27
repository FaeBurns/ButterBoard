using System;
using ButterBoard.Simulation.Elements;
using ButterBoard.UI.DraggableWindows;
using TMPro;
using UnityEngine;

namespace ButterBoard.UI.Processor
{
    public class ProcessorStatusWindow : CreatableWindow<ProcessorStatusWindow>
    {
        [SerializeField]
        private TextMeshProUGUI errorTextBlock = null!;

        private void Update()
        {

        }
    }
}