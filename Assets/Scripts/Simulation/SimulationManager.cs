using System.Collections.Generic;
using Coil.Connections;
using UnityEngine;
using UnityEngine.Serialization;

namespace ButterBoard.Simulation
{
    /// <summary>
    /// Manages the execution of a set of methods on all registered <see cref="ITickableObject"/> implementing classes.
    /// Aims to execute ticks in a way that eliminates execution-order based issues.
    /// </summary>
    public class SimulationManager : SingletonBehaviour<SimulationManager>
    {
        private readonly Queue<ITickableObject> _queuedAdditions = new Queue<ITickableObject>();
        private readonly Queue<ITickableObject> _queuedRemovals = new Queue<ITickableObject>();
        private readonly List<ITickableObject> _tickObjects = new List<ITickableObject>();

        [SerializeField]
        private float currentTickProgress;

        [field: FormerlySerializedAs("tickInterval")]
        [field: SerializeField]
        [field: Range(1, 300)]
        public int TicksPerSecond { get; set; } = 60;

        public bool IsTickInProgress { get; private set; }

        /// <summary>
        /// Gets the amount of ticks that have occured since startup.
        /// </summary>
        public int TickCount { get; private set; }

        /// <summary>
        /// The <see cref="ConnectionManager"/> instance used to manage connecting and disconnecting wires.
        /// </summary>
        public ConnectionManager ConnectionManager { get; } = new ConnectionManager();

        private void Update()
        {
            currentTickProgress += Time.deltaTime;
            float tickInterval = 1f / (float)TicksPerSecond;
            
            // if enough time has not yet passed, skip this frame
            // otherwise DoTick while there is time left
            while (currentTickProgress >= tickInterval)
            {
                currentTickProgress -= tickInterval;
                DoTick();
            }
        }

        public void DoTick()
        {
            IsTickInProgress = true;

            // add all pending tickObjects
            _tickObjects.AddRange(_queuedAdditions);
            _queuedAdditions.Clear();

            // loop through all queued removals
            while (_queuedRemovals.Count > 0)
            {
                // remove from _tickObjects
                _tickObjects.Remove(_queuedRemovals.Dequeue());
            }

            // execute tick _before_ clearing all values so the previous tick's values are still here

            // run tick on all registered tickable objects
            foreach (ITickableObject tickObject in _tickObjects)
            {
                tickObject.DoTick();
            }

            // apply changes made to power simulation
            PowerManager.ApplyChanges();

            IsTickInProgress = false;

            TickCount++;
        }

        public void RegisterTickObject(ITickableObject tickableObject)
        {
            _queuedAdditions.Enqueue(tickableObject);
        }

        public void DeRegisterTickObject(ITickableObject tickableObject)
        {
            _queuedRemovals.Enqueue(tickableObject);
        }
    }
}