using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ButterBoard.UI.ActionHistory
{
    public class ActionHistoryEntry : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI textMesh = null!;

        [SerializeField]
        private RectTransform textMeshRectTransform = null!;

        [SerializeField]
        private RectTransform imageRectTransform = null!;

        [SerializeField]
        private Animator animator = null!;

        public void InitializeMessage(string text, float lifetimeSeconds)
        {
            textMesh.SetText(text);

            animator.speed = lifetimeSeconds;

            StartCoroutine(UpdateSize());
        }

        private IEnumerator UpdateSize()
        {
            yield return null;
            
            // cannot use sizeDelta on textMeshRectTransform - ContentSizeFitter keeps it at 0, 0
            imageRectTransform.sizeDelta = textMeshRectTransform.rect.size;
        }
    }
}