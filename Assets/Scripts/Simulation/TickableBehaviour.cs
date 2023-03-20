using System;
using ButterBoard.FloatingGrid;
using ButterBoard.FloatingGrid.Placement.Placeables;
using UnityEngine;

namespace ButterBoard.Simulation
{
    public abstract class TickableBehaviour : MonoBehaviour, ITickableObject
    {
        protected virtual void Awake()
        {
            // try and find placeable on object
            BasePlaceable hostPlaceable = GetComponent<BasePlaceable>();
            if (hostPlaceable == null)
                return;

            hostPlaceable.Place.AddListener(Register);
            hostPlaceable.Pickup.AddListener(DeRegister);
            hostPlaceable.Remove.AddListener(DeRegister);
        }

        [SerializeField]
        protected GridPin powerPin = null!;

        [SerializeField]
        protected GridPin groundPin = null!;

        protected bool GetHasPower()
        {
            return powerPin.ConnectedPoint.Wire.Peek().Value && SimulationManager.Instance.ConnectedToGround(groundPin.ConnectedPoint.Wire);
        }

        public virtual void DoTick()
        {
            Tick();
        }

        public virtual void PushValues() { }

        public virtual void Cleanse() { }

        protected abstract void Tick();

        private void Register()
        {
            SimulationManager.Instance.RegisterTickObject(this);
        }

        private void DeRegister()
        {
            SimulationManager.Instance.DeRegisterTickObject(this);
        }
    }
}