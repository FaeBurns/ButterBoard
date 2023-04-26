using ButterBoard;
using ButterBoard.UI.Processor;
using NUnit.Framework;
using UnityEngine;

namespace Tests.PlayMode
{
    [SetUpFixture]
    public class TestSetup
    {
        [OneTimeSetUp]
        public void Setup()
        {
            GameObject gameManager = new GameObject();
            gameManager.AddComponent<TextHighlightSettings>();
            gameManager.AddComponent<GameManager>();
        }
    }
}