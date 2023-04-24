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
        private bool _hasErrored;

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
        /// Event fired when a runtime error is encountered. Can occur multiple times in one tick if multiple errors occur.
        /// </summary>
        public event EventHandler<ErrorMessageEventArgs>? RuntimeError;

        public override void DoTick()
        {
            // check for error before power check (base.DoTick)
            if (_hasErrored)
            {
                // only show error if there is power though
                PowerManager.SetPowerState(errorPin, _hasErrored && GetHasPower());
            }

            // exit early if there is no power
            if (!GetHasPower())
            {
                // kinda really stinky here
                // if there is no power just set all output pins to false
                SetIndexedPinValues(0, IndexedPinCount, new bool[IndexedPinCount]);
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

            // send values from all indexed pins to interpreter's PinController
            _activeInterpreter.PinController.SetInputPins(GetIndexedPinValues(0, IndexedPinCount));

            // perform interpreter step
            // catch and raise any errors that are encountered
            try
            {
                _activeInterpreter.Step();
            }
            catch (Exception e)
            {
                RuntimeError?.Invoke(this, new ErrorMessageEventArgs($"Exception occured during interpreter step. Exception message: {e.Message}", null));

                _hasErrored = true;

                return;
            }

            // if an error was found
            if (_activeInterpreter.HasErrors)
            {
                // notify listeners of all errors
                foreach (Error error in _activeInterpreter.InstructionErrorCollection.Errors)
                {
                    RuntimeError?.Invoke(this, new ErrorMessageEventArgs(error.Message, error));
                }

                _hasErrored = true;

                // don't allow output pins to get set
                return;
            }

            bool[] outputPinValues = _activeInterpreter.PinController.GetOutputPins();
            SetIndexedPinValues(0, IndexedPinCount, outputPinValues);
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

            // create a new interpreter from the known valid program
            _activeInterpreter = new Interpreter(ExecutionConfig, _activeValidTokenProgram);
        }
    }

    /// <summary>
    /// A class responsible for keeping record of all errors sent out by a <see cref="ProcessorElement"/>. Can be emptied via <see cref="Reset"/>.
    /// </summary>
    public class ProcessorErrorRecorder
    {
        private readonly List<string> _errors = new List<string>();

        /// <summary>
        /// Gets the recorded errors.
        /// </summary>
        public IEnumerable<string> Errors => _errors;

        /// <summary>
        /// Gets the amount of errors on record.
        /// </summary>
        public int ErrorCount => _errors.Count;

        public ProcessorErrorRecorder(ProcessorElement processor)
        {
            processor.RuntimeError += OnRuntimeError;
        }

        private void OnRuntimeError(object sender, ErrorMessageEventArgs e)
        {
            // if no error was supplied
            // add to record and exit
            if (e.Error == null)
            {
                _errors.Add(e.Message);
                return;
            }

            // otherwise add message from error
            _errors.Add(e.Error.ToString());
        }

        /// <summary>
        /// Clears the record of errors.
        /// </summary>
        public void Reset()
        {
            _errors.Clear();
        }

        /// <summary>
        /// Gets a complete string containing all errors.
        /// </summary>
        /// <returns>The errors as a single string separated by newlines.</returns>
        public string GetCompleteErrorString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string error in _errors)
            {
                stringBuilder.AppendLine(error);
            }

            return stringBuilder.ToString();
        }
    }

    /// <summary>
    /// <see cref="EventArgs"/> for use with <see cref="ProcessorElement.RuntimeError">ProcessorElement.RuntimeError</see>.
    /// </summary>
    public class ErrorMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the error contained in the message, if there is one.
        /// </summary>
        public Error? Error { get; }

        public ErrorMessageEventArgs(string message, Error? error)
        {
            Message = message;
            Error = error;
        }
    }
}