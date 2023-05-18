using Coil;
using UnityEngine;

namespace ButterBoard.Cables
{
    public class GridLineIndicator : MonoBehaviour
    {
        private Wire _wire = null!;

        [SerializeField]
        private SpriteRenderer spriteRenderer = null!;

        public void Initialize(Wire wire)
        {
            _wire = wire;
            wire.StateChanged += OnWireStateChanged;
            OnWireStateChanged(null!, new PowerStateChangedEventArgs(false));
        }

        private void OnWireStateChanged(object sender, PowerStateChangedEventArgs e)
        {
            if (spriteRenderer != null)
                spriteRenderer.color = e.State ? ColorManager.Instance.gridLinePoweredColor : ColorManager.Instance.gridLineUnpoweredColor;
        }

        private void OnDestroy()
        {
            _wire.StateChanged -= OnWireStateChanged;
        }
    }
}