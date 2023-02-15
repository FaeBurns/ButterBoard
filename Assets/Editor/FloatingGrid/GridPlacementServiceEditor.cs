using ButterBoard.FloatingGrid;
using ButterBoard.Lookup;
using UnityEditor;
using UnityEngine;

namespace ButterBoard.Editor.FloatingGrid
{
    [CustomEditor(typeof(GridPlacementService))]
    public class GridPlacementServiceEditor : UnityEditor.Editor
    {
        // ReSharper disable Unity.PerformanceAnalysis
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // only in play mode
            if (!Application.isPlaying)
                return;

            GridPlacementService service = (GridPlacementService)target;

            EditorGUI.BeginDisabledGroup(service.Placing);

            if (GUILayout.Button("Begin Test Placement"))
            {
                GameObject prefab = AssetSource.Fetch<GameObject>("FloatingGrid/SamplePlaceable");
                service.BeginPlacement(prefab);
            }

            EditorGUI.EndDisabledGroup();
        }
    }
}