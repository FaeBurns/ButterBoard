using System.Collections.Generic;
using ButterBoard.FloatingGrid;
using Coil;

namespace ButterBoard.Simulation
{
    /// <summary>
    /// Keeps track of how many times a wire has been powered and powers and un-powers them accordingly.
    /// </summary>
    public static class PowerManager
    {
        private static readonly HashSet<GridPoint> _poweredPoints = new HashSet<GridPoint>();
        private static readonly Dictionary<Wire, int> _poweredWireCounts = new Dictionary<Wire, int>();
        private static readonly Queue<PowerOperation> _queuedPowerOperations = new Queue<PowerOperation>();

        /// <summary>
        /// Applies changes made during Power and UnPower operations.
        /// </summary>
        public static void ApplyChanges()
        {
            while (_queuedPowerOperations.Count > 0)
            {
                PowerOperation operation = _queuedPowerOperations.Dequeue();

                if (operation.State)
                    PowerImmediate(operation.Point);
                else
                    UnPowerImmediate(operation.Point);
            }
        }

        /// <summary>
        /// Powers a <see cref="GridPin">GridPin's</see> connected <see cref="GridPoint"/>.
        /// </summary>
        /// <param name="pin">The pin that will be receiving the power.</param>
        public static void Power(GridPin pin)
        {
            Power(pin.ConnectedPoint);
        }

        /// <summary>
        /// Powers a <see cref="GridPoint"/>.
        /// </summary>
        /// <param name="point">The point to power.</param>
        public static void Power(GridPoint point)
        {
            SetPowerState(point, true);
        }

        /// <summary>
        /// Applies changes made to a <see cref="GridPoint">GridPoint's</see> power status.
        /// </summary>
        /// <param name="point">The point to apply changes to.</param>
        private static void PowerImmediate(GridPoint point)
        {
            // exit if already powered
            if (_poweredPoints.Contains(point))
                return;

            // record point
            _poweredPoints.Add(point);

            // power point's wire
            PowerImmediate(point.Wire);
        }

        /// <summary>
        /// Marks a wire as having another incoming power source.
        /// </summary>
        /// <param name="wire">The wire to mark.</param>
        private static void PowerImmediate(Wire wire)
        {
            // if wire not recorded, record and power wire
            if (!_poweredWireCounts.ContainsKey(wire))
            {
                wire.Power();
                _poweredWireCounts.Add(wire, 0);
            }

            // increase wire count
            _poweredWireCounts[wire]++;
        }

        /// <summary>
        /// Un-powers a <see cref="GridPin">GridPin's</see> connected <see cref="GridPoint"/>.
        /// </summary>
        /// <param name="pin">The pin whose power should be removed.</param>
        public static void UnPower(GridPin pin)
        {
            UnPower(pin.ConnectedPoint);
        }

        /// <summary>
        /// Un-powers a <see cref="GridPoint"/>.
        /// </summary>
        /// <param name="point">The point to un-power.</param>
        public static void UnPower(GridPoint point)
        {
            SetPowerState(point, false);
        }

        private static void UnPowerImmediate(GridPoint point)
        {
            // only un-power if point is already powered
            if (_poweredPoints.Contains(point))
            {
                _poweredPoints.Remove(point);

                UnPowerImmediate(point.Wire);
            }
        }

        /// <summary>
        /// Marks a wire as having one less power source.
        /// </summary>
        /// <param name="wire">The wire to mark as unpowered.</param>
        private static void UnPowerImmediate(Wire wire)
        {
            if (_poweredWireCounts.TryGetValue(wire, out int powerCount))
            {
                // if this is the final remove
                if (powerCount == 1)
                {
                    // de-power wire
                    wire.UnPower();

                    // and remove wire record
                    _poweredWireCounts.Remove(wire);

                    // exit early
                    return;
                }

                // remove one from record
                _poweredWireCounts[wire]--;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the specified pin is powered.
        /// </summary>
        /// <param name="pin">The pin to check.</param>
        /// <returns></returns>
        public static bool GetHasPower(GridPin pin)
        {
            return GetHasPower(pin.ConnectedPoint.Wire);
        }

        /// <summary>
        /// Gets a value indicating whether the specified point is powered.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns></returns>
        public static bool GetHasPower(GridPoint point)
        {
            return GetHasPower(point.Wire);
        }

        /// <summary>
        /// Gets a value indicating whether the specified wire is powered.
        /// </summary>
        /// <param name="wire">The wire to check.</param>
        /// <returns></returns>
        private static bool GetHasPower(Wire wire)
        {
            return wire.Peek();
        }

        /// <summary>
        /// Gets a value indicating whether the specified <see cref="GridPin"/> is providing power.
        /// </summary>
        /// <param name="pin">The pin to check.</param>
        /// <returns></returns>
        public static bool GetIsProvidingPower(GridPin pin)
        {
            return GetIsProvidingPower(pin.ConnectedPoint);
        }

        /// <summary>
        /// Gets a value indicating whether the specified <see cref="GridPoint"/> is providing power.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns></returns>
        public static bool GetIsProvidingPower(GridPoint point)
        {
            return _poweredPoints.Contains(point);
        }

        /// <summary>
        /// Calls Power or UnPower based on the value of <paramref name="state"/>.
        /// </summary>
        /// <param name="pin">The point to operate on.</param>
        /// <param name="state">A value indicating whether <paramref name="pin"/> should be powered or not.</param>
        public static void SetPowerState(GridPin pin, bool state)
        {
            SetPowerState(pin.ConnectedPoint, state);
        }

        /// <summary>
        /// Calls Power or UnPower based on the value of <paramref name="state"/>.
        /// </summary>
        /// <param name="point">The point to operate on.</param>
        /// <param name="state">A value indicating whether <paramref name="point"/> should be powered or not.</param>
        public static void SetPowerState(GridPoint point, bool state)
        {
            _queuedPowerOperations.Enqueue(new PowerOperation(point, state));
        }
    }

    public class PowerOperation
    {
        public GridPoint Point { get; }
        public bool State { get; }

        public PowerOperation(GridPoint point, bool state)
        {
            Point = point;
            State = state;
        }
    }
}