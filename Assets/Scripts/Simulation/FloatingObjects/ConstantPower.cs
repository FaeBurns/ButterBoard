using System;
using ButterBoard.FloatingGrid;
using Coil;
using UnityEngine;

namespace ButterBoard.Simulation.FloatingObjects
{
    public class ConstantPower : TickableBehaviour, ITickableObject
    {
        [SerializeField]
        private GridPoint powerPoint = null!;

        protected override void Awake()
        {
            powerPoint.Wire = new Wire();
            powerPoint.ConnectionStateChanged += OnConnectionStateChanged;
            base.Awake();
        }

        private void OnConnectionStateChanged(object sender, EventArgs e)
        {
            SimulationManager.Instance.RegisterTickObject(this);
        }

        public override void DoTick()
        {
            // push power to point
            PowerManager.Power(powerPoint);

            // deregister on first PushValues power is constant and should never need to be pushed again
            SimulationManager.Instance.DeRegisterTickObject(this);
        }
    }
}