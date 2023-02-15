﻿using BeanCore.Unity.ReferenceResolver;
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

            if (GUILayout.Button("Clear Grid"))
            {
                editorGridBuilder.ResolveReferences();
                if (editorGridBuilder.activeHost != null)
                    editorGridBuilder.GridBuilder.Clear(editorGridBuilder.activeHost);
            }

            if (GUILayout.Button("Build Grid"))
            {
                editorGridBuilder.ResolveReferences();
                if (editorGridBuilder.activeHost != null)
                    editorGridBuilder.GridBuilder.Clear(editorGridBuilder.activeHost);
                editorGridBuilder.activeHost = editorGridBuilder.GridBuilder.Build(editorGridBuilder.targetTransform, editorGridBuilder.width, editorGridBuilder.height, editorGridBuilder.spacing, editorGridBuilder.gridOffsetType);
            }
        }
    }
}