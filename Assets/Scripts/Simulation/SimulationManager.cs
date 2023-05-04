using System.Collections.Generic;
using Coil.Connections;
using UnityEngine;

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

        [SerializeField]
        [Range(1, 300)]
        private int tickInterval = 1;

        [SerializeField]
        [Range(1, 5)]
        private int burstTickCount = 1;

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
            currentTickProgress += (Time.deltaTime * 60);
            if (currentTickProgress + float.Epsilon >= tickInterval)
            {
                currentTickProgress = 0;
                for (int i = 0; i < burstTickCount; i++)
                {
                    DoTick();
                }
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