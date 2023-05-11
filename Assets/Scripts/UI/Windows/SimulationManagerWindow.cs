using System;
using System.Collections;
using ButterBoard.Simulation;
using ButterBoard.UI.DraggableWindows;
using ButterBoard.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ButterBoard.UI.Windows
{
    [WindowKey("UI/Windows/SimulationManager")]
    public class SimulationManagerWindow : CreatableWindow<SimulationManagerWindow>
    {
        private readonly SimulationStatusTickReader _tickReader = new SimulationStatusTickReader();
        private float _previousIntervalFPS = 0;
        private float _previousIntervalTPS = 0;
        private int frameCount = 0;

        [SerializeField]
        private AnimationCurve sliderValueCurve = new AnimationCurve();
        
        [SerializeField]
        private TextMeshProUGUI tickRateDisplay = null!;

        [SerializeField]
        private Slider slider = null!;

        [SerializeField]
        private Animator tickPulseAnimator = null!;

        [SerializeField]
        private GameObject tooFastIndicator = null!;

        [SerializeField]
        private int tooFastThreshold = 60;

        [SerializeField]
        [Tooltip("The time in seconds between fps counter updates")]
        private float uiUpdateIntervalSeconds = 1;

        [SerializeField]
        private float[] sliderSnapValues = Array.Empty<float>();

        [SerializeField]
        private float sliderSnapThreshold = 3;

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

            // create inverse of sliderValueCurve - allows setting slider value from TicksPerSecond value
            AnimationCurve inverseCurve = new AnimationCurve();
            for (int i = 0; i < sliderValueCurve.length; i++)
            {
                Keyframe inverseKey = new Keyframe(sliderValueCurve.keys[i].value, sliderValueCurve.keys[i].time);
                inverseCurve.AddKey(inverseKey);
            }
            
            slider.value = SimulationManager.Instance.TicksPerSecond;
            slider.onValueChanged.AddListener(SetSimulationSpeed);
            UpdateTooFastIndicator();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            SimulationManager.Instance.DeRegisterTickObject(_tickReader);
        }

        private void Update()
        {
            frameCount++;

            if (_tickReader.TickOccured)
            {
                tickPulseAnimator.Play("TickIndicatorPulse", -1, 0f);
                _tickReader.TickOccured = false;
            }
        }

        private IEnumerator UpdateUI()
        {
            DateTime lastTickTime = DateTime.Now;
            while (enabled)
            {
                // wait x seconds
                yield return new WaitForSecondsRealtime(uiUpdateIntervalSeconds);

                // then update rates in ui
                PushRateValues(DateTime.Now - lastTickTime);
                lastTickTime = DateTime.Now;

                // then reset counters
                frameCount = 0;
                _tickReader.RestartCount();
            }
        }

        private void PushRateValues(TimeSpan timeSinceLastPush)
        {
            float secondsPassed = (float)timeSinceLastPush.TotalSeconds;
            float frames = frameCount / secondsPassed;
            float ticks = _tickReader.GetTickRate(secondsPassed);

            // get average over 2 push intervals
            float displayFrames = (frames + _previousIntervalFPS) / 2;
            float displayTicks = (ticks + _previousIntervalTPS) / 2;
            
            tickRateDisplay.SetText($"FPS/TPS: {displayFrames :0.00}/{displayTicks :0.00}");
            
            _previousIntervalFPS = frames;
            _previousIntervalTPS = ticks;
        }

        private void SetSimulationSpeed(float sliderValue)
        {
            // only snap if LShift is held
            if (!Input.GetKeyDown(KeyCode.LeftShift))
            {
                // loop through all slider snap values
                foreach (float snapValue in sliderSnapValues)
                {
                    // check if value is inside threshold range
                    if (Mathf.Abs(sliderValue - snapValue) < sliderSnapThreshold)
                    {
                        // snap to value and set on slider
                        sliderValue = snapValue;
                        slider.SetValueWithoutNotify(sliderValue);
                    }
                }
            }

            // get as int and set simulation speed
            int tpsRounded = Mathf.RoundToInt(sliderValue);
            SimulationManager.Instance.TicksPerSecond = tpsRounded;

            // make tooltip bigger than normal
            // kinda ew, would prefer something in TooltipManager for setting size
            string tooltipString = "<size=20>Speed: " + (tpsRounded / 60f).ToString("0.00") + "x\nTPS: " + tpsRounded + "\n</size>";
            TooltipManager.Instance.SetTooltipOverride(tooltipString, TooltipMode.HELD);
            
            UpdateTooFastIndicator();
        }

        private void UpdateTooFastIndicator()
        {
            tooFastIndicator.SetActive(SimulationManager.Instance.TicksPerSecond >= tooFastThreshold);
        }
    }

    /// <summary>
    /// An <see cref="ITickableObject"/> used to measure the performing tick rate of the simulation.
    /// </summary>
    public class SimulationStatusTickReader : ITickableObject
    {
        private int _recordedTickCount = 0;

        /// <summary>
        /// Gets or Sets a value indicating whether a tick has occured since the last frame.
        /// </summary>
        public bool TickOccured { get; set; }

        public void DoTick()
        {
            _recordedTickCount++;
            TickOccured = true;
        }

        public void RestartCount()
        {
            _recordedTickCount = 0;
        }

        public float GetTickRate(float secondsPassed)
        {
            return _recordedTickCount / secondsPassed;
        }
    }
}