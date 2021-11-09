namespace CMSlib.ConsoleModule.InputStates
{
    /// <summary>
    /// Abstract base for the InputStates in the input events for BaseModule & ModuleManager
    /// </summary>
    public abstract class BaseInputState
    {
        /// <summary>
        /// Represents the type of input of this state
        /// </summary>
        public enum InputType
        {
            Click,
            Key,
            WindowResize,
            MouseMove
        }

        /// <summary>
        /// The type of this input state
        /// </summary>
        public InputType Type { get; protected set; }
    }
}