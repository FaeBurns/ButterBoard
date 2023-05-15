using System;
using System.Collections.Generic;
using System.Linq;

namespace ButterBoard.Building.BuildActions
{
    public class LoadingBuildAction : BuildAction
    {
        private readonly BuildAction[] _actions;
        
        public LoadingBuildAction(IEnumerable<BuildAction> actions)
        {
            _actions = actions.ToArray();
        }
        
        public override void Execute()
        {
            foreach (BuildAction action in _actions)
            {
                action.Execute();
            }
        }

        public override void UndoExecute()
        {
            throw new InvalidOperationException();
        }
    }
}