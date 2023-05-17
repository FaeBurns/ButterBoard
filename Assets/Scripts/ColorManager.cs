using UnityEngine;

namespace ButterBoard
{
    public class ColorManager : SingletonBehaviour<ColorManager>
    {
        public Color gridLineUnpoweredColor = new Color(0, 0, 0, 1);
        public Color gridLinePoweredColor = new Color(0, 0, 0, 1);
    }
}