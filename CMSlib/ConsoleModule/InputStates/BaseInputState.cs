namespace CMSlib.ConsoleModule.InputStates
{
    public abstract class BaseInputState
    {
        public enum InputType
        {
            Click, 
            Key,
            WindowResize,
            MouseMove
        }
        public InputType Type { get; protected set; }
    }
}