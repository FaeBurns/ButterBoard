using System;
using UnityEngine;

namespace ButterBoard.UI
{
    public class WorldCanvasReferencer : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Canvas>().worldCamera = Camera.main;
        }
    }
}