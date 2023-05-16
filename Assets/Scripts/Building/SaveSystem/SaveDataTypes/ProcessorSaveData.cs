using System;
using Newtonsoft.Json;

namespace ButterBoard.Building.SaveSystem.SaveDataTypes
{
    public class ProcessorSaveData : PlaceableSaveData
    {
        [JsonProperty]
        public string ProgramText { get; set; } = String.Empty;

        [JsonProperty]
        public bool IsProgramTextValid { get; set; }
    }
}