using System;
using BeanCore.Unity.ReferenceResolver;
using BeanCore.Unity.ReferenceResolver.Attributes;
using ButterBoard.FloatingGrid.Placement;
using UnityEngine;

namespace ButterBoard
{
    [RequireComponent(typeof(PlacementManager))]
    public class InteractionModeSwitcher : MonoBehaviour
    {
        private IInteractionProvider? _activeProvider;

        [field: SerializeField]
        public InteractionMode ActiveMode { get; private set; }

        [BindComponent]
        public PlacementManager PlacementManager { get; private set; } = null!;

        [BindComponent]
        public InteractionManager InteractionManager { get; private set; } = null!;

        private void Awake()
        {
            this.ResolveReferences();
            TrySetMode(InteractionMode.PLACE);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                switch (ActiveMode)
                {
                    case InteractionMode.PLACE:
                        if (TrySetMode(InteractionMode.INTERACT))
                            ActiveMode = InteractionMode.INTERACT;
                        break;
                    case InteractionMode.INTERACT:
                        if (TrySetMode(InteractionMode.PLACE))
                            ActiveMode = InteractionMode.PLACE;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public bool TrySetMode(InteractionMode mode)
        {
            if (_activeProvider != null)
            {
                // check if it is safe to switch away
                if (!_activeProvider.CanInteractionSafelySwitch())
                    return false;

                // notify old interaction provider of switch away
                _activeProvider.OnSwitchAway();
            }

            // perform switch
            _activeProvider = mode switch
            {
                InteractionMode.PLACE => PlacementManager,
                InteractionMode.INTERACT => InteractionManager,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null),
            };

            // notify new provider of switch
            _activeProvider.OnSwitchTo();

            return true;
        }
    }

    [Serializable]
    public enum InteractionMode
    {
        PLACE,
        INTERACT,
    }

    public interface IInteractionProvider
    {
        public void OnSwitchTo();

        public void OnSwitchAway();

        public bool CanInteractionSafelySwitch();
    }
}