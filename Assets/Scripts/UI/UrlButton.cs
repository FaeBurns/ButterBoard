using System;
using UnityEngine;
using UnityEngine.UI;

namespace ButterBoard.UI
{
    [RequireComponent(typeof(Button))]
    public class UrlButton : MonoBehaviour
    {
        [SerializeField]
        private string url = String.Empty;

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            Application.OpenURL(url);
        }
    }
}