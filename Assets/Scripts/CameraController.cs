using System;
using BeanCore.Unity.ReferenceResolver;
using BeanCore.Unity.ReferenceResolver.Attributes;
using UnityEngine;

namespace ButterBoard
{
    public class CameraController : ReferenceResolvedBehaviour
    {
        [BindComponent]
        private Camera _camera = null!;

        [Header("Movement")]
        [SerializeField]
        private float speed = 0.1f;

        [Header("Zoom")]
        [SerializeField]
        private float zoom = 5f;

        [SerializeField]
        private AnimationCurve zoomCurve = new AnimationCurve();

        [SerializeField]
        private float zoomSpeed = 0.1f;

        [SerializeField]
        private float minZoom = 1;

        [SerializeField]
        private float maxZoom = 8;

        private void LateUpdate()
        {
            UpdatePosition();
            UpdateZoom();
        }

        private void UpdatePosition()
        {
            // Instant Acceleration
            // Smooth stop

            // get raw velocity
            Vector3 rawVelocity = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);

            // get smoothed velocity
            Vector3 velocity = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);

            // default final velocity to raw velocity
            Vector3 finalVelocity = rawVelocity;

            // if there is no input
            // use smoothed velocity
            if (rawVelocity.sqrMagnitude == 0)
                finalVelocity = velocity;

            // normalize velocity but keep magnitude
            finalVelocity = finalVelocity.normalized * finalVelocity.magnitude;

            // scale by frame time
            finalVelocity *= Time.deltaTime;

            // add velocity to position
            // multiply by zoom to allow for more control while zoomed in
            transform.position += finalVelocity * (speed * zoom);
        }

        private void UpdateZoom()
        {
            // get zoom delta
            float zoomDelta = Input.GetAxisRaw("Mouse ScrollWheel");

            // negative speed fixes zoom direction
            zoom += zoomDelta * -zoomSpeed;

            // clamp to allowed values
            zoom = Mathf.Clamp(zoom, minZoom, maxZoom);

            // evaluate zoom along curve
            float actualZoom = zoomCurve.Evaluate(zoom);

            // update in camera
            _camera.orthographicSize = actualZoom;
        }
    }
}