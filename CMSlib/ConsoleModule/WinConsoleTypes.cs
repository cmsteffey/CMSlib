using System.Runtime.InteropServices;
using System;

namespace CMSlib.ConsoleModule
{
    
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct KeyEventRecord
    {
        [FieldOffset(0), MarshalAs(UnmanagedType.Bool)]
        public bool bKeyDown;

        [FieldOffset(4), MarshalAs(UnmanagedType.U2)]
        public ushort wRepeatCount;

        [FieldOffset(6), MarshalAs(UnmanagedType.U2)]
        public ushort wVirtualKeyCode;

        [FieldOffset(8), MarshalAs(UnmanagedType.U2)]
        public ushort wVirtualScanCode;
        
        [FieldOffset(12), MarshalAs(UnmanagedType.U4)]
        public ControlKeyState dwControlKeyState;

        [FieldOffset(10)] public char UnicodeChar;

        
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct MouseEventRecord
    {
        [FieldOffset(0)] public Coord MousePosition;
        [FieldOffset(4)] public ButtonState ButtonState;
        [FieldOffset(8)] public ControlKeyState ControlKeyState;
        [FieldOffset(12)] public EventFlags EventFlags;
    }
    [Flags]
    public enum ButtonState {
        Left1Pressed = 1,
        RightPressed = 2,
        Left2Pressed = 4,
        Left3Pressed = 8,
        Left4Pressed = 16,
    }
    [Flags]
    public enum EventFlags {
        MouseMoved = 1,
        DoubleClick = 2,
        MouseWheeled = 4,
        MouseHorizontalWheeled = 8
    }


    [Flags]
    public enum ControlKeyState : uint {
        RightAltPressed = 1,
        LeftAltPressed = 2,
        RightControlPressed = 4,
        LeftControlPressed = 8,
        ShiftPressed = 16,
        NumlockOn = 32,
        ScrollLockOn = 64,
        CapslockOn = 128,
        EnhancedKey = 256
    }
    public enum EventType : ushort {
        Focus = 0x0010,
        Key = 0x0001,
        Menu = 0x0008,
        Mouse = 0x0002,
        WindowBufferSize = 0x0004
    }
    [StructLayout (LayoutKind.Sequential)]
    public struct MenuEventRecord {
        public uint dwCommandId;
    }

    [StructLayout (LayoutKind.Sequential)]
    public struct FocusEventRecord {
        public uint bSetFocus;
    }
    [StructLayout (LayoutKind.Sequential)]
    public struct Coord {
        public short X;
        public short Y;

        public Coord (short X, short Y)
        {
            this.X = X;
            this.Y = Y;
        }
        

        public static bool operator ==(Coord a, Coord b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Coord a, Coord b)
        {
            return !a.Equals(b);
        }
        public override bool Equals(object? obj)
        {
            if (obj is null)
                return false;
            if (obj is not Coord c)
                return false;
            return c.X == X && c.Y == Y;
        }

        public override int GetHashCode()
        {
            return unchecked(((X) << 16) + Y);
        }

        public bool Inside(BaseModule module)
        {
            int xDiff = X - module.X;
            int yDiff = Y - module.Y;
            return xDiff >= 0 && xDiff < module.Width && yDiff >= 0 && yDiff< module.Height;
        }
    };
    [StructLayout (LayoutKind.Sequential)]
    public struct WindowBufferSizeRecord {
        public Coord size;
        public WindowBufferSizeRecord (short x, short y)
        {
            this.size = new Coord (x, y);
        }
    }
    
    [StructLayout(LayoutKind.Explicit)]
    public struct InputRecord
    {
        [FieldOffset(0)] public EventType EventType;
        [FieldOffset(4)] public KeyEventRecord KeyEvent;
        [FieldOffset(4)] public MouseEventRecord MouseEvent;
        [FieldOffset(4)] public WindowBufferSizeRecord WindowBufferSizeEvent;
        [FieldOffset(4)] public MenuEventRecord MenuEvent;
        [FieldOffset(4)] public FocusEventRecord FocusEvent;

        public static implicit operator InputRecord(ConsoleKeyInfo keyInfo)
        {
            var returns = new InputRecord {EventType = EventType.Key};
            var keyRecord = new KeyEventRecord
            {
                UnicodeChar = keyInfo.KeyChar,
                bKeyDown = true,
                wVirtualKeyCode = (ushort) keyInfo.Key,
                wVirtualScanCode = 0
            };
            if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Alt))
                keyRecord.dwControlKeyState |= ControlKeyState.LeftAltPressed;
            if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
                keyRecord.dwControlKeyState |= ControlKeyState.LeftControlPressed;
            if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift))
                keyRecord.dwControlKeyState |= ControlKeyState.ShiftPressed;
            returns.KeyEvent = keyRecord;
            keyRecord.wRepeatCount = 1;
            return returns;
        }

        public static implicit operator ConsoleKeyInfo(InputRecord record)
        {
            return new ConsoleKeyInfo(
                record.KeyEvent.UnicodeChar,
                (ConsoleKey) record.KeyEvent.wVirtualKeyCode,
                record.KeyEvent.dwControlKeyState.HasFlag(ControlKeyState.ShiftPressed),
                record.KeyEvent.dwControlKeyState.HasFlag(ControlKeyState.LeftAltPressed) ||
                record.KeyEvent.dwControlKeyState.HasFlag(ControlKeyState.RightAltPressed),
                record.KeyEvent.dwControlKeyState.HasFlag(ControlKeyState.LeftControlPressed) ||
                record.KeyEvent.dwControlKeyState.HasFlag(ControlKeyState.RightControlPressed)
            );
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FlashInfo
    {
        public uint cbSize;
        public IntPtr hWnd;
        public FlashFlags dwFlags;
        public uint uCount;
        public int dwTimeOut;
    }
    //cast 0 to enum to stop flash
    [Flags]
    public enum FlashFlags : int
    {
        /// <summary>
        /// Flash caption
        /// </summary>
        FlashCaption = 0x00000001,
        /// <summary>
        /// Flash taskbar icon
        /// </summary>
        FlashTray = 0x00000002,
        /// <summary>
        /// Flash taskbar icon & caption
        /// </summary>
        FlashAll = 0x00000003,
        /// <summary>
        /// Flash until window enum-casted 0 is called
        /// </summary>
        FlashTimer = 0x00000004,
        /// <summary>
        /// Flash until the window comes to the foreground
        /// </summary>
        FlashNoTimer = 0x0000000C
        
    }

}