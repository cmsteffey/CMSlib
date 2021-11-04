namespace CMSlib.ConsoleModule.InputStates
{
    public class MouseMoveInputState : BaseInputState
    {
        /// <summary>
        /// The new coordinates of the mouse 
        /// </summary>
        public (int X, int Y) MouseCoordinates { get; }

        /// <summary>
        /// Constructs a MouseMoveInputState from the MouseEvent of an InputRecord
        /// </summary>
        /// <param name="record">The record to make the state from</param>
        internal MouseMoveInputState(InputRecord record)
        {
            Type = InputType.Click;
            MouseCoordinates = (record.MouseEvent.MousePosition.X, record.MouseEvent.MousePosition.Y);
        }
    }
}