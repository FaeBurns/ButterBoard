using System.Collections.Generic;
using UnityEngine;

namespace ButterBoard.FloatingGrid
{
    public class GridPlaceable : MonoBehaviour
    {
        public IReadOnlyList<GridPin> Pins => Pins;

        [SerializeField]
        private List<GridPin> pins = new List<GridPin>();
    }
}