namespace CMSlib.ConsoleModule.InputStates
{
    public class ClickInputState : BaseInputState
    {
        public ControlKeyState ControlKeyState { get; }
        public ButtonState ButtonState { get; }
        public (int X, int Y) MouseCoordinates { get; }
        
        internal ClickInputState(InputRecord record)
        {
            Type = InputType.Click;
            ControlKeyState = record.MouseEvent.ControlKeyState;
            MouseCoordinates = (record.MouseEvent.MousePosition.X, record.MouseEvent.MousePosition.Y);
            ButtonState = record.MouseEvent.ButtonState;
        }
    }
}