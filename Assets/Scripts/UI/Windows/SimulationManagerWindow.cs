using System;
using System.Collections;
using System.Collections.Generic;
using ButterBoard.Simulation;
using ButterBoard.UI.DraggableWindows;
using TMPro;
using UnityEngine;

namespace ButterBoard.UI.Windows
{
    [WindowKey("UI/Windows/SimulationManager")]
    public class SimulationManagerWindow : CreatableWindow<SimulationManagerWindow>
    {
        private readonly SimulationStatusTickReader _tickReader = new SimulationStatusTickReader();
        private int frameCount = 0;
        private float currentAverageFrameRate = 0f;

        [SerializeField]
        private TextMeshProUGUI tickRateDisplay = null!;

        [SerializeField]
        private TextMeshProUGUI speedStatusDisplay = null!;

        public SimulationManagerWindow()
        {
            Opened += OnOpened;
            Closed += OnClosed;
        }

        private void OnEnable()
        {
            StartCoroutine(UpdateUI());
        }

        private void OnOpened(object sender, EventArgs e)
        {
            SimulationManager.Instance.RegisterTickObject(_tickReader);
        }

        private void OnClosed(object sender, EventArgs e)
        {
            SimulationManager.Instance.DeRegisterTickObject(_tickReader);
        }

        private void Update()
        {
            frameCount++;
            currentAverageFrameRate += (Time.unscaledDeltaTime - currentAverageFrameRate) * 0.03f;
        }

        private IEnumerator UpdateUI()
        {
            while (enabled)
            {
                yield return new WaitForSecondsRealtime(1);
                PushRateValues();
            }
        }

        private void PushRateValues()
        {
            tickRateDisplay.SetText($"FPS/TPS: {(1f/currentAverageFrameRate) :.2}/{(1f/_tickReader.CurrentAverageTickRate) :.2}");
        }
    }

    /// <summary>
    /// An <see cref="ITickableObject"/> used to measure the performing tick rate of the simulation.
    /// </summary>
    public class SimulationStatusTickReader : ITickableObject
    {
        private DateTime _lastTickTime = DateTime.Now;
        public float CurrentTickRate { get; private set; }
        public float CurrentAverageTickRate { get; private set; }

        public void DoTick()
        {
            TimeSpan timeDiff = DateTime.Now - _lastTickTime;
            _lastTickTime = DateTime.Now;

            CurrentTickRate = 1f / (float)timeDiff.TotalSeconds;

            // https://answers.unity.com/questions/326621/how-to-calculate-an-average-fps.html
            // moving average
            CurrentAverageTickRate += (CurrentTickRate - CurrentAverageTickRate) * 0.03f;
        }
    }
}