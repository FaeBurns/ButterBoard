using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using ButterBoard.Building.BuildActions;
using ButterBoard.FloatingGrid.Placement;
using ButterBoard.UI.ActionHistory;
using UnityEngine;

namespace ButterBoard.Building
{
    /// <summary>
    /// Manages doing, undoing, and redoing <see cref="BuildAction"/> actions.
    /// </summary>
    public class BuildActionManager : SingletonBehaviour<BuildActionManager>
    {
        private readonly Stack<BuildAction> _undoStack = new Stack<BuildAction>();
        private readonly Stack<BuildAction> _redoStack = new Stack<BuildAction>();
        private readonly List<BuildAction> _lifetimeActionList = new List<BuildAction>();

        /// <summary>
        /// Executes an action and pushes it to the undo stack. Clears the redo stack.
        /// <param name="action">The action to perform.</param>
        /// </summary>
        public void PushAndExecuteAction(BuildAction action)
        {
            action.Execute();

            _lifetimeActionList.Add(action);
            _undoStack.Push(action);
            _redoStack.Clear();
        }

        /// <summary>
        /// Pushes an action to the undo stack without executing it. Clears the redo stack
        /// </summary>
        /// <param name="action"></param>
        public void PushActionNoExecute(BuildAction action)
        {
            _lifetimeActionList.Add(action);

            _undoStack.Push(action);
            _redoStack.Clear();
        }

        /// <summary>
        /// Undoes the last performed action and adds it to the undo stack.
        /// </summary>
        public void Undo()
        {
            if (_undoStack.Count == 0)
                return;

            BuildAction action = _undoStack.Pop();
            action.UndoExecute();

            // add the reverse of the last action
            _lifetimeActionList.Add(new ReverseAction(action));
            _redoStack.Push(action);

            ActionHistoryHost.Instance.PushMessage($"Undo {GetActionDisplayName(action)}");
        }

        /// <summary>
        /// Redoes the last undone action and adds it to the undo stack.
        /// </summary>
        public void Redo()
        {
            if (_redoStack.Count == 0)
                return;

            BuildAction action = _redoStack.Pop();
            action.Execute();

            // re-add action removed in undo
            _lifetimeActionList.Add(action);
            _undoStack.Push(action);

            ActionHistoryHost.Instance.PushMessage($"Redo {GetActionDisplayName(action)}");
        }

        /// <summary>
        /// Clears both the undo and redo stacks.
        /// </summary>
        public void ClearUndoStack()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }

        public List<BuildAction> GetLifetimeActions()
        {
            return _lifetimeActionList;
        }

        private void Update()
        {

#if !UNITY_EDITOR
            // only require control if not in the editor - editor window hooks Ctrl+z/y inputs
            bool controlDown = true;
            controlDown = controlDown && Input.GetKey(KeyCode.LeftControl);
            controlDown = controlDown && Input.GetKey(KeyCode.RightControl);

            if (!controlDown)
                return;
#endif

            // don't allow any actions while placing
            if (PlacementManager.Instance.Placing)
                return;

            if (Input.GetKeyDown(KeyCode.Z))
            {
                // (Ctrl+)Shift+Z is redo, without shift is undo
                if (Input.GetKey(KeyCode.LeftShift))
                    Redo();
                else
                    Undo();
            }

            if (Input.GetKeyDown(KeyCode.Y))
                Redo();
        }

        private string GetActionDisplayName(BuildAction action)
        {
            return action.GetType().GetCustomAttribute<DisplayNameAttribute>().DisplayName;
        }
    }
}