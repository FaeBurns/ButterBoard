﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ButterBoard.FloatingGrid.Placement.Services
{
    public abstract class PlacementService<T> : IPlacementService
        where T : BasePlaceable
    {
        protected readonly LerpSettings LerpSettings;
        protected readonly float DisplayZDistance;

        protected PlacementService(LerpSettings lerpSettings, float displayZDistance)
        {
            LerpSettings = lerpSettings;
            DisplayZDistance = displayZDistance;
        }

        /// <summary>
        /// Gets the context providing information about the current placement operation.
        /// Will be null if no placement operation has started.
        /// </summary>
        protected PlacementContext<T> Context { get; set; } = null!;

        /// <summary>
        /// Begins a new placement from the object's prefab.
        /// </summary>
        /// <param name="prefab">The prefab of the object being placed. The <see cref="BasePlaceable"/> script must be on the root object.</param>
        public virtual void BeginPrefabPlacement(GameObject prefab)
        {
            // create real and display objects
            GameObject placingObject = Object.Instantiate(prefab);
            GameObject displayDuplicate = Object.Instantiate(prefab);

            // get placeable component on placing object
            T? placeable = placingObject.GetComponent<T>();
            T displayPlaceable = displayDuplicate.GetComponent<T>();

            // throw if not found
            if (placeable == null)
                throw new ArgumentException($"Cannot begin placement of prefab {placingObject.name} as argument it does not have a {nameof(T)} component");

            // set context
            Context = new PlacementContext<T>(placingObject, displayDuplicate, placeable, displayPlaceable, PlacementType.PLACE);

            // hide and disable actual object
            Context.PlacingObject.SetActive(false);

            // set name of display object to aid debugging
            // notify the placeable that it is the display instance
            Context.DisplayPlaceable.name += "(Display)";
            Context.DisplayPlaceable.SetDisplay();
        }

        /// <summary>
        /// Begins movement of an existing <see cref="BasePlaceable"/>.
        /// </summary>
        /// <param name="target">The object being moved. The <see cref="BasePlaceable"/> script must be on the root object.</param>
        public virtual void BeginMovePlacement(GameObject target)
        {
            // get placeable component on target object
            T? placeable = target.GetComponent<T>();

            // throw if not found
            if (placeable == null)
                throw new ArgumentException($"Cannot begin movement of object {target.name} as argument {nameof(target)} does not have a {nameof(T)} component");

            // invoke pickup - should disable/disconnect ect. all components that are used during tick.
            placeable.Pickup?.Invoke();

            // create duplicate used in display
            GameObject displayDuplicate = Object.Instantiate(target);

            T displayPlaceable = displayDuplicate.GetComponent<T>();

            // set context
            Context = new PlacementContext<T>(target, displayDuplicate, placeable, displayPlaceable, PlacementType.MOVE);

            // hide and disable actual object
            Context.PlacingObject.SetActive(false);

            // set name of display object to aid debugging
            // notify the placeable that it is the display instance
            Context.DisplayPlaceable.name += "(Display)";
            Context.DisplayPlaceable.SetDisplay();
        }

        public void TryCommitPlacement()
        {
            if (CommitPlacement())
            {
                Context.State = PlacementState.FINALIZE;
            }
        }

        /// <summary>
        /// Checks to see if the current context contains a valid placement position and
        /// </summary>
        /// <returns>True if the placement was successful and this <see cref="PlacementService{T}"/> can be safely discarded.</returns>
        protected abstract bool CommitPlacement();

        /// <summary>
        /// Update this placement service.
        /// </summary>
        /// <returns>A bool that indicates if placement has concluded. True if this service is done, False if not</returns>
        public virtual bool Update(Vector3 targetPosition, Quaternion targetRotation)
        {
            switch (Context.State)
            {
                case PlacementState.POSITION:
                    UpdatePosition(targetPosition, targetRotation);
                    return false;
                case PlacementState.FINALIZE:
                    return UpdateFinalize();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Cancels placement and deletes the held object.
        /// </summary>
        public virtual void CancelPlacement()
        {
            // invoke remove event
            Context.Placeable.Remove?.Invoke();

            Object.Destroy(Context.PlacingObject);
            Object.Destroy(Context.DisplayObject);
        }

        /// <summary>
        /// Removes any now unnecessary resources.
        /// </summary>
        public virtual void CompletePlacement()
        {
            Object.Destroy(Context.DisplayObject);

            Context.PlacingObject.SetActive(true);
            Context.Placeable.Place?.Invoke();
        }

        /// <summary>
        /// Updates this placement service while the context is in the <see cref="PlacementState.POSITION"/> state.
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetRotation"></param>
        protected abstract void UpdatePosition(Vector3 targetPosition, Quaternion targetRotation);

        /// <summary>
        /// Updates this placement service while the context is in the <see cref="PlacementState.FINALIZE"/> state.
        /// </summary>
        /// <returns></returns>
        protected virtual bool UpdateFinalize()
        {
            // get current transform
            Vector2 currentDisplayPosition = Context.DisplayObject.transform.position;
            Quaternion currentDisplayRotation = Context.DisplayObject.transform.rotation;

            // get target transform
            Vector2 targetPosition = Context.PlacingObject.transform.position;
            Quaternion targetRotation = Context.PlacingObject.transform.rotation;

            // get lerp target
            Vector3 lerpPosition = Vector2.Lerp(currentDisplayPosition, targetPosition, LerpSettings.TranslateLerp);
            lerpPosition.z = DisplayZDistance;
            Quaternion lerpRotation = Quaternion.Lerp(currentDisplayRotation, targetRotation, LerpSettings.RotateLerp);

            // set display object to use lerp data
            Context.DisplayObject.transform.position = lerpPosition;
            Context.DisplayObject.transform.rotation = lerpRotation;

            // check approximate position and rotation
            bool approximatePosition = currentDisplayPosition.ApproximateDistance(targetPosition, 0.01f);
            bool approximateRotation = Mathf.Abs(Quaternion.Dot(currentDisplayRotation, targetRotation)).Approximately(1);

            if (approximatePosition && approximateRotation)
            {
                Debug.Log($"current: {currentDisplayPosition} | lerp: {lerpPosition} | target {targetPosition}");

                // don't need to bother updating position/rotation
                // display object is deleted immediately after
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets all objects with a <see cref="TComponent"/> component in overlap range of the target.
        /// </summary>
        /// <param name="target">The target object used to define the position and rotation. Size is received from <see cref="PlacementContext{T}.Size">Context.Size</see></param>
        /// <typeparam name="TComponent">The type of component to search for.</typeparam>
        /// <returns></returns>
        protected List<TComponent> GetOverlaps<TComponent>(BasePlaceable target) where TComponent : Component
        {
            return GetOverlaps<TComponent>(target.transform.position, Context.Size, target.transform.rotation.eulerAngles.z);
        }

        /// <summary>
        /// Gets all objects with a component of type <see cref="TComponent"/> that overlaps with the specified area.
        /// </summary>
        /// <param name="position">The center position of the overlap area.</param>
        /// <param name="size">The size (extents * 2) of the overlap area.</param>
        /// <param name="rotation">The rotation of the overlap area.</param>
        /// <typeparam name="TComponent">The type of component to search for.</typeparam>
        /// <returns></returns>
        protected List<TComponent> GetOverlaps<TComponent>(Vector2 position, Vector2 size, float rotation)
        {
            Collider2D[] overlaps = Physics2D.OverlapBoxAll(position, size, rotation);
            List<TComponent> result = new List<TComponent>();
            foreach (Collider2D overlap in overlaps)
            {
                TComponent component = overlap.GetComponent<TComponent>();
                if (component != null)
                    result.Add(component);
            }
            return result;
        }

        /// <summary>
        /// <para>Sets the position and rotation of the placing object to the arguments specified.</para>
        /// <para>Sets the position and rotation to an interpolated value near to the targeted values as specified by the LerpSettings object parameter</para>
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        protected void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            Context.PlacingObject.transform.SetPositionAndRotation(position, rotation);

            Vector3 displayPosition = Vector3.Lerp(Context.DisplayObject.transform.position, position, LerpSettings.TranslateLerp);
            displayPosition.z = DisplayZDistance;
            Quaternion displayRotation = Quaternion.Lerp(Context.DisplayObject.transform.rotation, rotation, LerpSettings.RotateLerp);

            Context.DisplayObject.transform.SetPositionAndRotation(displayPosition, displayRotation);
        }
    }
}