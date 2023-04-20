using ButterBoard.Lookup;
using Toaster.Definition;
using UnityEngine;

namespace ButterBoard
{
    public class GameManager : MonoBehaviour
    {
        private void Awake()
        {
            // load all instruction definitions from file
            TextAsset instructionsTextAsset = AssetSource.Fetch<TextAsset>("InstructionDefinitions")!;
            InstructionManager.LoadSignatures(instructionsTextAsset.text);
        }
    }
}