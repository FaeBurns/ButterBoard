using System;
using System.Collections.Generic;
using ButterBoard.FloatingGrid;
using ButterBoard.FloatingGrid.Placement.Placeables;
using ButterBoard.Simulation;
using UnityEngine;

namespace ButterBoard.Building
{
    public class BuildManager : SingletonBehaviour<BuildManager>
    {
        private static readonly Dictionary<int, BasePlaceable> _registeredPlaceables = new Dictionary<int, BasePlaceable>();
        private static int _nextId = 0;

        [SerializeField]
        private Transform floatingPlaceableHost = null!;

        [SerializeField]
        private LayerMask gridPointLayerMask;

        public static Transform GetFloatingPlaceableHost() => Instance.floatingPlaceableHost;

        /// <summary>
        /// Disconnects a <see cref="GridPoint"/> from a <see cref="GridPin"/> if it is connected to one.
        /// </summary>
        /// <param name="point">The point to disconnect.</param>
        public static void RemoveConnections(GridPoint point)
        {
            if (point.ConnectedPin == null)
                return;

            // if ConnectedPoint is providing power, it will be doing so from this pin
            // therefore it needs to be un-powered
            if (PowerManager.GetIsProvidingPower(point))
            {
                PowerManager.UnPower(point);
            }
            
            point.ConnectedPin.ConnectedPoint = null!;
            point.ConnectedPin = null;
        }

        /// <summary>
        /// Disconnects a <see cref="GridPin"/> from a <see cref="GridPoint"/> if it is connected to one.
        /// </summary>
        /// <param name="pin">The pin to disconnect.</param>
        public static void RemoveConnections(GridPin pin)
        {
            if (pin.ConnectedPoint == null)
                return;

            // if ConnectedPoint is providing power, it will be doing so from this pin
            // therefore it needs to be un-powered
            if (PowerManager.GetIsProvidingPower(pin))
            {
                PowerManager.UnPower(pin);
            }
            
            pin.ConnectedPoint.ConnectedPin = null;
            pin.ConnectedPoint = null!;
        }

        /// <summary>
        /// Connects a grid pin and point together.
        /// </summary>
        /// <param name="point">The point to connect.</param>
        /// <param name="pin">The pin to connect.</param>
        public static void Connect(GridPin pin, GridPoint point)
        {
            RemoveConnections(point);
            RemoveConnections(pin);

            point.ConnectedPin = pin;
            pin.ConnectedPoint = point;
        }

        /// <summary>
        /// Gets either a new register id or an existing one. If <paramref name="existingId"/> is -1, will return result from <see cref="GetNextRegistryId"/>, otherwise will return <paramref name="existingId"/>.
        /// </summary>
        /// <param name="existingId"></param>
        public static int GetNextOrExistingId(int existingId)
        {
            if (existingId == -1)
                return GetNextRegistryId();
            return existingId;
        }

        /// <summary>
        /// Gets the next unused register id.
        /// </summary>
        public static int GetNextRegistryId()
        {
            return _nextId++;
        }

        /// <summary>
        /// Registers a placeable by id in order to keep track of it across saves.
        /// </summary>
        /// <param name="placeable">The placeable to register.</param>
        /// <param name="id">The id to register the placeable with.</param>
        /// <returns><paramref name="id"/></returns>
        public static int RegisterPlaceable(BasePlaceable placeable, int id)
        {
            if (_registeredPlaceables.ContainsKey(id))
                throw new InvalidOperationException($"id {id} already exists");
            
            // register placeable
            _registeredPlaceables.Add(id, placeable);
            placeable.Key = id;

            return id;
        }

        /// <summary>
        /// Gets a placeable by its id. Assumes the id is valid.
        /// </summary>
        /// <param name="id">The id of the desired placeable.</param>
        /// <returns>The resulting placeable.</returns>
        public static BasePlaceable GetPlaceable(int id)
        {
            return _registeredPlaceables[id];
        }

        /// <summary>
        /// Gets a placeable by its id. Assumes the id is valid and points to a placeable of the desired type.
        /// </summary>
        /// <param name="id">The id of the desired placeable.</param>
        /// <typeparam name="T">The type of placeable to cast the placeable to.</typeparam>
        /// <returns>The resulting placeable.</returns>
        public static T GetPlaceable<T>(int id) where T : BasePlaceable
        {
            return (T)_registeredPlaceables[id];
        }

        /// <summary>
        /// Removes a registered placeable from the internal store
        /// </summary>
        /// <param name="key">The key of the placeable being removed.</param>
        public static void RemoveRegisteredPlaceable(int key)
        {
            _registeredPlaceables.Remove(key);
        }

        public static LayerMask GetGridPointLayerMask()
        {
            return Instance.gridPointLayerMask;
        }
    }
}