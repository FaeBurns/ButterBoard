using System;
using ButterBoard.Simulation.Elements;
using TMPro;
using UnityEngine;

namespace ButterBoard.UI.Processor
{
    public class ProcessorStatusWindow : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI errorTextBlock = null!;

        public void Initialize(ProcessorErrorRecorder errorRecorder, ProcessorElement processorElement)
        {

        }

        private void Update()
        {
            
        }
    }
}