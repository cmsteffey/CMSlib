namespace CMSlib.ConsoleModule.InputStates
{
    public class MouseMoveInputState : BaseInputState
    {
        public (int X, int Y)  MouseCoordinates { get; }
        internal MouseMoveInputState(InputRecord record)
        {
            Type = InputType.Click;
            MouseCoordinates = (record.MouseEvent.MousePosition.X, record.MouseEvent.MousePosition.Y);
        }
    }
}