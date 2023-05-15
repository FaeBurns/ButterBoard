using ButterBoard.Lookup;
using Toaster.Definition;
using UnityEngine;

namespace ButterBoard
{
    public class GameManager : MonoBehaviour
    {
        private static bool _hasTriggered = false;
        
        private void Awake()
        {
            if (_hasTriggered)
                return;
            _hasTriggered = true;
            
            // load all instruction definitions from file
            TextAsset instructionsTextAsset = AssetSource.Fetch<TextAsset>("InstructionDefinitions")!;
            InstructionManager.LoadSignatures(instructionsTextAsset.text);

            Application.targetFrameRate = 60;
        }
    }
}