using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ButterBoard.UI
{
    public class ButtonSelectionSetter : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(RemoveSelection);
        }

        public void RemoveSelection()
        {
            EventSystem.current.SetSelectedGameObject(null!);
        }
    }
}