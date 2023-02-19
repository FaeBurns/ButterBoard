using ButterBoard.FloatingGrid;
using ButterBoard.FloatingGrid.Placement;
using ButterBoard.Lookup;
using UnityEditor;
using UnityEngine;

namespace ButterBoard.Editor.FloatingGrid
{
    [CustomEditor(typeof(PlacementService))]
    public class GridPlacementServiceEditor : UnityEditor.Editor
    {
        // ReSharper disable Unity.PerformanceAnalysis
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // only in play mode
            if (!Application.isPlaying)
                return;

            PlacementService service = (PlacementService)target;

            EditorGUI.BeginDisabledGroup(service.Placing);

            if (GUILayout.Button("Begin Test Placement"))
            {
                GameObject prefab = AssetSource.Fetch<GameObject>("FloatingGrid/SamplePlaceable");
                service.BeginPrefabPlacement(prefab);
            }

            EditorGUI.EndDisabledGroup();
        }
    }
}