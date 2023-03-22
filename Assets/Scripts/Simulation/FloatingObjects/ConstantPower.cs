using System;
using ButterBoard.FloatingGrid;
using Coil;
using Coil.Connections;
using UnityEngine;

namespace ButterBoard.Simulation.FloatingObjects
{
    public class ConstantPower : MonoBehaviour, ITickableObject
    {
        private bool _placed = false;

        [SerializeField]
        private GridPoint powerPoint = null!;

        private void Awake()
        {
            powerPoint.Wire = new Wire(new SynchronizedValueSource());
        }

        public void DoTick()
        {
        }

        public void PushValues()
        {
            powerPoint.Wire.Push(new BoolValue(true));
        }

        public void Cleanse()
        {
        }

        public void OnPlaced()
        {
            if (_placed)
                return;

            SimulationManager.Instance.RegisterTickObject(this);

            _placed = true;
        }

        public void OnRemove()
        {
            SimulationManager.Instance.DeRegisterTickObject(this);

            _placed = false;
        }
    }
}