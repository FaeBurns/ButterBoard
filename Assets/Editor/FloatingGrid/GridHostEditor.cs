using ButterBoard.FloatingGrid;
using UnityEditor;
using UnityEngine;

namespace ButterBoard.Editor.FloatingGrid
{
    [CustomEditor(typeof(GridHost))]
    public class GridHostEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Index GridPoints"))
            {
                GridHost gridHost = (GridHost)target;
                int index = 0;
                foreach (GridPoint point in gridHost.GridPoints)
                {
                    point.PointIndex = index;
                    index++;
                    
                    PrefabUtility.RecordPrefabInstancePropertyModifications(point);
                }
                PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            }

            if (GUILayout.Button("Fix Radius"))
            {
                GridHost gridHost = (GridHost)target;
                
                foreach (GridPoint point in gridHost.GridPoints)
                {
                    point.Radius = 0.8f;
                    point.GetComponent<CircleCollider2D>().radius = 0.8f;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(point);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(point.GetComponent<CircleCollider2D>());
                }
                PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            }
        }
    }
}