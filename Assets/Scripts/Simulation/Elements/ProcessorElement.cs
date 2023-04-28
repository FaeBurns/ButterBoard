using System;
using System.Collections.Generic;
using System.Text;
using ButterBoard.FloatingGrid;
using Toaster;
using Toaster.Execution;
using Toaster.Parsing;
using UnityEngine;

namespace ButterBoard.Simulation.Elements
{
    public class ProcessorElement : TickableBehaviourWithPins
    {
        private bool _previousResetValue;

        private Interpreter? _activeInterpreter;
        private TokenProgram? _activeValidTokenProgram;

        [SerializeField]
        private GridPin resetPin = null!;

        [SerializeField]
        private GridPin errorPin = null!;

        /// <summary>
        /// Gets the interpreter currently being run on this processor. Null if no program has been set.
        /// </summary>
        public Interpreter? ActiveInterpreter => _activeInterpreter;

        /// <summary>
        /// Gets the configuration of this processor.
        /// </summary>
        [field: SerializeField]
        public ExecutionConfig ExecutionConfig { get; private set; } = new ExecutionConfig();

        /// <summary>
        /// Gets or Sets the program text stored alongside this processor. This text is not assumed to be valid.
        /// </summary>
        public string UnvalidatedProgramText { get; set; } = String.Empty;

        /// <summary>
        /// Gets the program text of the last valid compiled program. Set on a valid result from <see cref="TrySetProgram"/>.
        /// </summary>
        public string ValidProgramText { get; private set; } = String.Empty;

        /// <summary>
        /// Gets a collection of all errors that have occured since the program was last compiled.
        /// </summary>
        public ErrorCollection ErrorCollection { get; } = new ErrorCollection();

        /// <summary>
        /// Gets a value indicating whether an error has occured.
        /// </summary>
        public bool HasErrored => ErrorCollection.HasErrors;

        /// <summary>
        /// Gets a value indicating whether there is an interpreter active. Does not check to see if any errors have occured
        /// </summary>
        public bool IsActive => ActiveInterpreter != null;

        public override void DoTick()
        {
            // check for error before power check (base.DoTick)
            if (HasErrored)
            {
                // only show error if there is power though
                PowerManager.SetPowerState(errorPin, HasErrored && GetHasPower());
            }

            // exit early if there is no power
            if (!GetHasPower())
            {
                // kinda really stinky here
                // if there is no power just set all output pins to false
                SetIndexedPinValues(0, IndexedPinCount - 1, new bool[IndexedPinCount]);

                return;
            }

            // exit early if no interpreter is set.
            if (_activeInterpreter == null)
                return;

            // get reset pin powered state
            bool resetPinValue = PowerManager.GetHasPower(resetPin);

            // if reset was not powered last tick and reset is currently powered
            if (!_previousResetValue && resetPinValue)
            {
                // record reset pin value
                _previousResetValue = resetPinValue;

                // perform reset
                Reset();

                // skip the rest of this tick
                return;
            }
            // reset is no longer powered but previous value tracker reports powered
            else if (_previousResetValue && !resetPinValue)
            {
                _previousResetValue = false;
            }

            // send values from all indexed pins to interpreter's PinController
            _activeInterpreter.PinController.SetInputPins(GetIndexedPinValues(0, IndexedPinCount - 1));

            // perform interpreter step
            // catch and raise any errors that are encountered
            try
            {
                _activeInterpreter.Step();
            }
            catch (Exception e)
            {
                ErrorCollection.RaiseError(
                    $"Exception occured during interpreter step on line. Exception message: {e.Message}",
                    _activeInterpreter.CurrentLineIndex,
                    0,
                    _activeValidTokenProgram!.FullProgramLines[_activeInterpreter.CurrentLineIndex].Length - 1);

                return;
            }

            // if an error was found
            if (_activeInterpreter.HasErrors)
            {
                // notify listeners of all errors
                foreach (Error error in _activeInterpreter.InstructionErrorCollection.Errors)
                {
                    ErrorCollection.Raise(error);
                }

                // don't allow output pins to get set
                return;
            }

            bool[] outputPinValues = _activeInterpreter.PinController.GetOutputPins();
            SetIndexedPinValues(0, IndexedPinCount - 1, outputPinValues);
        }

        /// <summary>
        /// Tries to set the active program to the supplied text. Will perform error detection and return any errors found.
        /// </summary>
        /// <param name="programText">The program to run.</param>
        /// <returns>A collection of all errors found during parsing.</returns>
        public ErrorCollection TrySetProgram(string programText)
        {
            // record program text
            UnvalidatedProgramText = programText;

            // tokenize program
            Parser parser = new Parser();
            TokenProgram tokenProgram = parser.Tokenize(programText);

            // return errors if found
            if (parser.Errors.HasErrors)
                return parser.Errors;

            // validate token structure
            TokenProgramValidator programValidator = new TokenProgramValidator();
            programValidator.Validate(tokenProgram, ExecutionConfig);

            // return errors if found
            if (programValidator.ErrorCollection.HasErrors)
                return programValidator.ErrorCollection;

            // record valid token program - used in reset
            _activeValidTokenProgram = tokenProgram;

            // set active interpreter
            _activeInterpreter = new Interpreter(ExecutionConfig, tokenProgram);

            // set valid program text
            ValidProgramText = programText;

            // clear execution error collection
            ErrorCollection.Clear();

            // return empty collection if no errors were found
            return new ErrorCollection();
        }

        /// <summary>
        /// Resets the currently running program. If no program is running, nothing happens.
        /// </summary>
        public void Reset()
        {
            // cannot reset if no program is set
            if (_activeInterpreter == null)
                return;

            // un-power error pin if powered
            PowerManager.UnPower(errorPin);

            // clear all current errors
            ErrorCollection.Clear();

            // create a new interpreter from the known valid program
            _activeInterpreter = new Interpreter(ExecutionConfig, _activeValidTokenProgram);
        }

        /// <summary>
        /// Stops the currently running program.
        /// </summary>
        public void Stop()
        {
            _activeInterpreter = null;

            // clear all pins and unset error
            SetIndexedPinValues(0, IndexedPinCount - 1, new bool[IndexedPinCount]);
            PowerManager.UnPower(errorPin);
        }
    }
}