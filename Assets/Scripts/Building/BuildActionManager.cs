using System;
using System.Collections.Generic;
using ButterBoard.FloatingGrid.Placement;
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
            
        /// <summary>
        /// Executes an action and pushes it to the undo stack. Clears the redo stack.
        /// <param name="action">The action to perform.</param>
        /// </summary>
        public void PushAndExecuteAction(BuildAction action)
        {
            action.Execute();
            _undoStack.Push(action);
            _redoStack.Clear();
        }

        /// <summary>
        /// Pushes an action to the undo stack without executing it. Clears the redo stack
        /// </summary>
        /// <param name="action"></param>
        public void PushNoExecuteAction(BuildAction action)
        {
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
            _redoStack.Push(action);
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
            _undoStack.Push(action);
        }

        private void Update()
        {
            // bool controlDown = true;
            // controlDown = controlDown && Input.GetKey(KeyCode.LeftControl);
            // controlDown = controlDown && Input.GetKey(KeyCode.RightControl);
            //
            // if (!controlDown)
            //     return;
            
            // don't allow any actions while placing
            if (PlacementManager.Instance.Placing)
                return;
            
            if (Input.GetKeyDown(KeyCode.Z))
                Undo();
            
            if (Input.GetKeyDown(KeyCode.Y))
                Redo();
        }
    }
}