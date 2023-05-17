using ButterBoard.Cables;
using ButterBoard.FloatingGrid;
using ButterBoard.Lookup;
using UnityEditor;
using UnityEditor.SceneManagement;
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

            if (GUILayout.Button("Add GridLineIndicators"))
            {
                GridHost gridHost = (GridHost)target;

                foreach (GridPointConnectedRow connectedRow in gridHost.ConnectedRows)
                {
                    Transform firstChild = connectedRow.transform.GetChild(0);
                    Transform lastChild = connectedRow.transform.GetChild(connectedRow.transform.childCount - 1);

                    Vector2 difference = lastChild.transform.position - firstChild.transform.position;
                    Vector2 localMidpoint = difference / 2;

                    GameObject lineDisplayObject = (GameObject)PrefabUtility.InstantiatePrefab(AssetSource.Fetch<GameObject>("Editor/GridLineIndicator")!, connectedRow.transform);
                    GridLineIndicator lineIndicator = lineDisplayObject.GetComponent<GridLineIndicator>();
                    connectedRow.lineIndicator = lineIndicator;

                    StageUtility.PlaceGameObjectInCurrentStage(lineDisplayObject);
                    lineDisplayObject.transform.localPosition = localMidpoint;
                    lineDisplayObject.transform.SetAsFirstSibling();
                    lineDisplayObject.transform.localScale = GetLineIndicatorScale(difference);

                    PrefabUtility.RecordPrefabInstancePropertyModifications(connectedRow);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(lineIndicator);
                }

                PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            }

            if (GUILayout.Button("Fix GridLineIndicator Z"))
            {
                GridHost gridHost = (GridHost)target;
                foreach (GridLineIndicator lineIndicator in gridHost.GetComponentsInChildren<GridLineIndicator>())
                {
                    lineIndicator.transform.localPosition = new Vector3(lineIndicator.transform.localPosition.x, lineIndicator.transform.localPosition.y, 0.1f);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(lineIndicator.transform);
                }
            }

            if (GUILayout.Button("Fix ConnectedRows"))
            {
                GridHost gridHost = (GridHost)target;

                for (int i = 0; i < gridHost.transform.childCount; i++)
                {
                    Transform childTransform = gridHost.transform.GetChild(i);
                    GridPointConnectedRow connectedRow = childTransform.GetComponent<GridPointConnectedRow>();

                    if (connectedRow == null)
                        continue;

                    gridHost.ConnectedRows.Add(connectedRow);
                }
                PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            }
        }

        private Vector3 GetLineIndicatorScale(Vector2 difference)
        {
            if (difference.x > difference.y)
                return new Vector3(difference.x, 0.1f, 1f);
            else
                return new Vector3(0.1f, difference.y, 1f);
        }
    }
}