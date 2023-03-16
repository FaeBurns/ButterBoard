using UnityEngine;

namespace ButterBoard
{
    public class InteractionManager : MonoBehaviour, IInteractionProvider
    {
        public void OnSwitchTo()
        {
            enabled = true;
        }

        public void OnSwitchAway()
        {
            enabled = false;
        }

        public bool CanInteractionSafelySwitch()
        {
            return true;
        }
    }
}