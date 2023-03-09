using System.Linq;
using BeanCore.Unity.ReferenceResolver;
using ButterBoard.FloatingGrid;
using UnityEditor;
using UnityEngine;

namespace ButterBoard.Editor.FloatingGrid
{
    [CustomEditor(typeof(EditorGridBuilder))]
    [CanEditMultipleObjects]
    public class EditorGridBuilderEditor : UnityEditor.Editor
    {
        // ReSharper disable Unity.PerformanceAnalysis
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGridBuilder editorGridBuilder = (EditorGridBuilder)target;

            if (GUILayout.Button("Clear All Children"))
            {
                while (editorGridBuilder.targetTransform.childCount > 0)
                {
                    DestroyImmediate(editorGridBuilder.targetTransform.GetChild(0).gameObject);
                }
            }

            if (GUILayout.Button("Build Grid"))
            {
                editorGridBuilder.ResolveReferences();
                if (editorGridBuilder.activeHost != null)
                    editorGridBuilder.GridBuilder.Clear(editorGridBuilder.activeHost);
                editorGridBuilder.GridBuilder.Build(editorGridBuilder.targetTransform, editorGridBuilder.width, editorGridBuilder.height, editorGridBuilder.spacing, editorGridBuilder.gridOffsetType);
            }

            if (GUILayout.Button("Unify Host (Warning, Dangerous)"))
            {
                GridHost targetGridHost = editorGridBuilder.targetTransform.GetComponentInChildren<GridHost>();
                GridPoint[] childPoints = targetGridHost.GetComponentsInChildren<GridPoint>();

                foreach (GridPoint gridPoint in childPoints)
                {
                    gridPoint.Initialize(targetGridHost);
                }

                targetGridHost.ConnectedRows.Clear();
                targetGridHost.ConnectedRows.AddRange(targetGridHost.GetComponentsInChildren<GridPointConnectedRow>());

                targetGridHost.GridPoints.Clear();
                targetGridHost.GridPoints.AddRange(childPoints);
            }
        }
    }
}