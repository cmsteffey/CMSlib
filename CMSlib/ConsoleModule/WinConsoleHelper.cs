using System;
using System.Runtime.InteropServices;

namespace CMSlib.ConsoleModule
{
    public partial class WinConsoleHelper : IConsoleHelper
    {
        
        
        public InputRecord? ReadInput()
        {
            IntPtr inputHandle = GetStdHandle(-10);
            
            int hResult = WaitForSingleObject(inputHandle, -1);
            IntPtr recordPtr = Marshal.AllocHGlobal(Marshal.SizeOf<InputRecord>());

            if (hResult == int.MaxValue) return null;
            try
            {
                if (!ReadConsoleInput(inputHandle, recordPtr, 1, out uint numberOfEventsRead)) return null;
                if (numberOfEventsRead == 0u) return null;
                return Marshal.PtrToStructure(recordPtr, typeof(InputRecord)) as InputRecord?;
            }
            catch
            {
                return null;
            }
            finally
            {
                Marshal.FreeHGlobal(recordPtr);
            }

        }
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);
        
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadConsoleInput(
            IntPtr  hConsoleInput,
            IntPtr recordBuffer,
            uint  nLength,
            out uint lpNumberOfEventsRead
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int WaitForSingleObject(IntPtr hHandle, int dwMilliseconds);
    }
}