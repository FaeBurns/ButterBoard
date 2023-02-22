using System;
using BeanCore.Unity.ReferenceResolver;
using BeanCore.Unity.ReferenceResolver.Attributes;
using UnityEngine;

namespace ButterBoard.FloatingGrid
{
    public class GridPin : ReferenceResolvedBehaviour
    {
        [field: SerializeField]
        public SpriteRenderer[] PinSprites { get; private set; } = Array.Empty<SpriteRenderer>();

        public GridPoint? ConnectedPoint { get; private set; }

        [BindComponent(Parent = true)]
        public GridPlaceable Host { get; private set; } = null!;

        public void Connect(GridPoint target)
        {
            ConnectedPoint = target;
        }

        public void Free()
        {
            ConnectedPoint = null;
        }

        public void DisplayIssue(PinPlacementIssueType issueType)
        {
            Color tintColor = issueType switch
            {
                PinPlacementIssueType.INVALID_HOST => Color.yellow,
                PinPlacementIssueType.PORT_BLOCKED => Color.red,
                PinPlacementIssueType.PORT_OCCUPIED => Color.red,
                PinPlacementIssueType.PORT_NOT_FOUND => Color.magenta,
                PinPlacementIssueType.EXISTING_INVALID_CONNECTION => Color.blue,
                _ => throw new ArgumentOutOfRangeException(nameof(issueType), issueType, null),
            };

            foreach (SpriteRenderer spriteRenderer in PinSprites)
            {
                spriteRenderer.color = tintColor;
            }
        }

        public void ClearIssue()
        {
            foreach (SpriteRenderer spriteRenderer in PinSprites)
            {
                spriteRenderer.color = Color.white;
            }
        }
    }
}