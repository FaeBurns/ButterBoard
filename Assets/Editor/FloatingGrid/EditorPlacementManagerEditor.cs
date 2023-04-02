using BeanCore.Unity.ReferenceResolver;
using ButterBoard.FloatingGrid.Placement;
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

            if (GUILayout.Button("Begin Placement (Processor)"))
            {
                manager.BeginPlace("FloatingGrid/Buildables/Processor_24");
            }

            if (GUILayout.Button("Begin Placement (Board)"))
            {
                manager.BeginPlace("FloatingGrid/Floating/Breadboard_30x5");
            }

            if (GUILayout.Button("Begin Placement (Cable)"))
            {
                manager.BeginPlace("FloatingGrid/Cable" + editorManager.CableColour.ToString());
            }

            if (GUILayout.Button("Begin Placement (Input)"))
            {
                manager.BeginPlace(editorManager.TestPlacementAsset);
            }

            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(!manager.Placing);

            if (GUILayout.Button("Cancel Placement"))
            {
                manager.Cancel();
            }

            EditorGUI.EndDisabledGroup();
        }
    }
}