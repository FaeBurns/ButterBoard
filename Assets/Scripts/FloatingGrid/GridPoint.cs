using JetBrains.Annotations;
using UnityEngine;

namespace ButterBoard.FloatingGrid
{
    public class GridPoint : MonoBehaviour
    {
        public GridPin? ConnectedPin { get; private set; }

        public bool Open => ConnectedPin == null;

        public void Connect(GridPin target)
        {
            ConnectedPin = target;
        }

        public void Free()
        {
            ConnectedPin = null;
        }
    }
}