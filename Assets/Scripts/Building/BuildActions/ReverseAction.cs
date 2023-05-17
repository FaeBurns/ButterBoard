using Newtonsoft.Json;

namespace ButterBoard.Building.BuildActions
{
    public class ReverseAction : BuildAction
    {
        [JsonProperty]
        private BuildAction _action;

        public ReverseAction(BuildAction action)
        {
            _action = action;
        }

        public override void Execute()
        {
            _action.UndoExecute();
        }

        public override void UndoExecute()
        {
            _action.Execute();
        }
    }
}