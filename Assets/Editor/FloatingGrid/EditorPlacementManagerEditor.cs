using BeanCore.Unity.ReferenceResolver;
using ButterBoard.FloatingGrid;
using ButterBoard.FloatingGrid.Placement;
using ButterBoard.Lookup;
using UnityEditor;
using UnityEngine;

namespace ButterBoard.Editor.FloatingGrid
{
    [CustomEditor(typeof(EditorPlacementManager))]
    public class EditorPlacementManagerEditor : UnityEditor.Editor
    {
        // ReSharper disable Unity.PerformanceAnalysis
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // only in play mode
            if (!Application.isPlaying)
                return;

            EditorPlacementManager editorManager = (EditorPlacementManager)target;
            editorManager.ResolveReferences();
            PlacementManager manager = editorManager.PlacementManager;

            EditorGUI.BeginDisabledGroup(manager.Placing);

            if (GUILayout.Button("Begin Test Placement (Grid)"))
            {
                GameObject prefab = AssetSource.Fetch<GameObject>("FloatingGrid/SampleGridPlaceable")!;
                manager.BeginPlace(prefab);
            }

            if (GUILayout.Button("Begin Test Placement (Floating)"))
            {
                GameObject prefab = AssetSource.Fetch<GameObject>("FloatingGrid/Breadboard")!;
                manager.BeginPlace(prefab);
            }

            if (GUILayout.Button("Begin Test Placement (Cable)"))
            {
                GameObject prefab = AssetSource.Fetch<GameObject>("FloatingGrid/Cable" + editorManager.CableColour.ToString())!;
                manager.BeginPlace(prefab);
            }

            if (GUILayout.Button("Begin Test Placement (Input)"))
            {
                GameObject prefab = AssetSource.Fetch<GameObject>(editorManager.TestPlacementAsset)!;
                manager.BeginPlace(prefab);
            }

            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(!manager.Placing);

            if (GUILayout.Button("Cancel Test Placement"))
            {
                manager.Cancel();
            }

            EditorGUI.EndDisabledGroup();
        }
    }
}