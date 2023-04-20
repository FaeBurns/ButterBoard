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
            base.Awake();
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