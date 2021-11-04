namespace CMSlib.ConsoleModule.InputStates
{
    public class ClickInputState : BaseInputState
    {
        /// <summary>
        /// Represents the state of shift, ctrl, and alt at the time of the click
        /// </summary>
        public ControlKeyState ControlKeyState { get; }

        /// <summary>
        /// Represents which buttons on the mouse were down at the time of this click
        /// </summary>
        public ButtonState ButtonState { get; }

        /// <summary>
        /// The coordinates in console cells at the time of this click
        /// </summary>
        public (int X, int Y) MouseCoordinates { get; }

        /// <summary>
        /// Constructs a ClickInputState from the MouseEvent of an InputRecord
        /// </summary>
        /// <param name="record">The record to make the state from</param>
        internal ClickInputState(InputRecord record)
        {
            Type = InputType.Click;
            ControlKeyState = record.MouseEvent.ControlKeyState;
            MouseCoordinates = (record.MouseEvent.MousePosition.X, record.MouseEvent.MousePosition.Y);
            ButtonState = record.MouseEvent.ButtonState;
        }
    }
}