using System;
using UnityEngine;

namespace ButterBoard
{
    public class BackgroundPositioner : MonoBehaviour
    {
        private Camera _camera = null!;

        [SerializeField]
        private SpriteRenderer backgroundSpriteRenderer = null!;

        [SerializeField]
        private int moveThreshold = 5;

        private void Awake()
        {
            _camera = Camera.main!;
            OnValidate();
        }

        private void OnValidate()
        {
            transform.localScale = new Vector3(moveThreshold, moveThreshold, 1);
        }

        private void LateUpdate()
        {
            Vector2 cameraPosition = _camera.transform.position;
            Vector2 flooredCameraPosition = new Vector2(Mathf.Floor(cameraPosition.x / moveThreshold), Mathf.Floor(cameraPosition.y / moveThreshold));
            Vector2 scaledBackPosition = flooredCameraPosition * moveThreshold;
            transform.position = new Vector3(scaledBackPosition.x, scaledBackPosition.y, 0);

            UpdateTilingSize();
        }

        private void UpdateTilingSize()
        {
            Vector2 viewSize = GetCameraViewSize();

            // scale viewSize by size of individual tile
            Vector2 scaledSize = viewSize / moveThreshold;

            // snap to 2s - looks nicer
            Vector2 finalSnappedSize = SnapVectorCeil(scaledSize, 2);

            // add 2 to account for camera offset
            finalSnappedSize += new Vector2(2, 2);
            backgroundSpriteRenderer.size = finalSnappedSize;
        }

        private Vector2 GetCameraViewSize()
        {
            float cameraHeight = _camera.orthographicSize * 2;
            return new Vector2(cameraHeight * _camera.aspect, cameraHeight);
        }

        private Vector2 SnapVectorFloor(Vector2 vector, float snap)
        {
            return new Vector2(Mathf.Floor(vector.x / snap) * snap, Mathf.Floor(vector.y / snap) * snap);
        }

        private Vector2 SnapVectorCeil(Vector2 vector, float snap)
        {
            return new Vector2(Mathf.Ceil(vector.x / snap) * snap, Mathf.Ceil(vector.y / snap) * snap);
        }
    }
}