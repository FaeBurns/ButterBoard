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

            hostPlaceable.Pickup.AddListener(OnPickup);
        }

        public abstract void DoTick();

        public void Register()
        {
            SimulationManager.Instance.RegisterTickObject(this);
        }

        public void DeRegister()
        {
            SimulationManager.Instance.DeRegisterTickObject(this);
        }

        protected virtual void OnPickup() { }
    }
}