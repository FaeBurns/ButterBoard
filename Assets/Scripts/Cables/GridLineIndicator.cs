using Coil;
using UnityEngine;

namespace ButterBoard.Cables
{
    public class GridLineIndicator : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer spriteRenderer = null!;

        public void Initialize(Wire wire)
        {
            wire.StateChanged += OnWireStateChanged;
            OnWireStateChanged(null!, new PowerStateChangedEventArgs(false));
        }

        private void OnWireStateChanged(object sender, PowerStateChangedEventArgs e)
        {
            spriteRenderer.color = e.State ? ColorManager.Instance.gridLinePoweredColor : ColorManager.Instance.gridLineUnpoweredColor;
        }
    }
}