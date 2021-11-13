using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using TermiosLib.TermiosEnums;

// TODO: Complete documentation in before next commit
namespace TermiosLib
{
    public class MacHandle
    {
        [DllImport("libSystem.B.dylib", SetLastError = true)]
        private static extern long read(long fileDes, out byte b, long count);

        [DllImport("libSystem.B.dylib", SetLastError = true)]
        private static extern long get_attr(long fileDes, out Termios termios);

        [DllImport("libSystem.B.dylib", SetLastError = true)]
        private static extern long set_attr(long fileDes, OptionalActions optionalActions, in Termios termios);

        [DllImport("libSystem.B.dylib", SetLastError = true)]
        private static extern long tc_drain(long fileDes);

        [DllImport("libSystem.B.dylib", SetLastError = true)]
        private static extern long tc_flow(long fileDes, LineCtrlFlags action);

        [DllImport("libSystem.B.dylib", SetLastError = true)]
        private static extern long tc_flush(long fileDes, LineCtrlFlags queueSelector);

        [DllImport("libSystem.B.dylib", SetLastError = true)]
        private static extern long tc_get_sid(long fileDes);

        [DllImport("libSystem.B.dylib", SetLastError = true)]
        private static extern long tc_send_break(long fileDes, long duration);

        private readonly long _fileDes;
        private readonly Termios _termios;

        private static readonly Dictionary<long, Termios> UsedFileDescriptors = new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileDes">File descriptor of the TTY compliant device </param>
        public MacHandle(long fileDes)
        {
            _fileDes = fileDes;

            if (UsedFileDescriptors.ContainsKey(fileDes))
                _termios = UsedFileDescriptors[fileDes];
            else
            {
                GetAttrs(out _termios);
                UsedFileDescriptors.Add(fileDes, _termios);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void EnableRaw()
        {
            GetAttrs(out Termios newState);

            newState.c_lflag &= ~(LocalFlags.ICanon | LocalFlags.IExten | LocalFlags.Echo | LocalFlags.ISig);
            newState.c_oflag &= ~OutputFlags.OPost;
            newState.c_iflag &= ~(InputFlags.IxOn | InputFlags.IStrip | InputFlags.ICrNl);
            newState.c_cflag |= ControlFlags.Cs8;

            SetAttrs(OptionalActions.TcSaNow, in newState);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetTerm()
        {
            SetAttrs(OptionalActions.TcSaNow, in _termios);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public long ReadByte(out byte b)
        {
            return read(_fileDes, out b, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="termios"></param>
        /// <exception cref="Win32Exception"></exception>
        public void GetAttrs(out Termios termios)
        {
            if (get_attr(_fileDes, out termios) == -1)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="optionalActions"></param>
        /// <param name="termios"></param>
        /// <exception cref="Win32Exception"></exception>
        private void SetAttrs(OptionalActions optionalActions, in Termios termios)
        {
            if (set_attr(_fileDes, optionalActions, in termios) == -1)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="optionalActions"></param>
        /// <param name="modify"></param>
        public void ModifyGlobalAttrs(OptionalActions optionalActions, ModifyAction modify)
        {
            GetAttrs(out Termios termios);
            modify(ref termios);
            SetAttrs(optionalActions, in termios);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="Win32Exception"></exception>
        public void DrainOutput()
        {
            if (tc_drain(_fileDes) == -1)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <exception cref="Win32Exception"></exception>
        public void FlowOutput(LineCtrlFlags action)
        {
            if (tc_flow(_fileDes, action) == -1)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queueSelector"></param>
        /// <exception cref="Win32Exception"></exception>
        public void FlushOutput(LineCtrlFlags queueSelector)
        {
            if (tc_flush(_fileDes, queueSelector) == -1)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Win32Exception"></exception>
        public long GetProcessGroupId()
        {
            var result = tc_get_sid(_fileDes);
            return result == -1 ? throw new Win32Exception(Marshal.GetLastWin32Error()) : result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="duration"></param>
        /// <exception cref="Win32Exception"></exception>
        public void SendBreak(long duration)
        {
            if (tc_send_break(_fileDes, duration) == -1)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// 
        /// </summary>
        public delegate void ModifyAction(ref Termios t);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="function"></param>
        public void StateSandbox(Action function)
        {
            GetAttrs(out Termios termios);
            function();
            SetAttrs(OptionalActions.TcSaNow, in termios);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="fallbackState"></param>
        public void FallbackOnFailure(Func<bool> predicate, ModifyAction fallbackState)
        {
            GetAttrs(out Termios termios);
            if (!predicate())
            {
                SetAttrs(OptionalActions.TcSaNow, in termios);
                ModifyGlobalAttrs(OptionalActions.TcSaNow, fallbackState);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        public void RevertOnFailure(Func<bool> predicate)
        {
            GetAttrs(out Termios prevState);
            if (!predicate())
                SetAttrs(OptionalActions.TcSaNow, in prevState);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GlobalStateString()
        {
            GetAttrs(out var termios);
            return termios.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string OriginalStateString()
        {
            return _termios.ToString();
        }
    }

    public class LinuxHandle
    {
        [DllImport("libc.so.6", SetLastError = true)]
        private static extern long read(long fileDes, out byte b, long count);

        [DllImport("libc.so.6", SetLastError = true)]
        private static extern long get_attr(long fileDes, out Termios termios);
        
        [DllImport("libc.so.6", SetLastError = true)]
        private static extern long set_attr(long fileDes, OptionalActions optionalActions, in Termios termios);
        
        [DllImport("libc.so.6", SetLastError = true)]
        private static extern long tc_drain(long fileDes);
        
        [DllImport("libc.so.6", SetLastError = true)]
        private static extern long tc_flow(long fileDes, LineCtrlFlags action);
        
        [DllImport("libc.so.6", SetLastError = true)]
        private static extern long tc_flush(long fileDes, LineCtrlFlags queueSelector);

        [DllImport("libc.so.6", SetLastError = true)]
        private static extern long tc_get_sid(long fileDes);

        [DllImport("libc.so.6", SetLastError = true)]
        private static extern long tc_send_break(long fileDes, long duration);

        private readonly long _fileDes;
        private readonly Termios _termios;

        private static readonly Dictionary<long, Termios> UsedFileDescriptors = new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileDes">File descriptor of the TTY compliant device </param>
        public LinuxHandle(long fileDes)
        {
            _fileDes = fileDes;

            if (UsedFileDescriptors.ContainsKey(fileDes))
                _termios = UsedFileDescriptors[fileDes];
            else
            {
                GetAttrs(out _termios);
                UsedFileDescriptors.Add(fileDes, _termios);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void EnableRaw()
        {
            GetAttrs(out Termios newState);

            newState.c_lflag &= ~(LocalFlags.ICanon | LocalFlags.IExten | LocalFlags.Echo | LocalFlags.ISig);
            newState.c_oflag &= ~OutputFlags.OPost;
            newState.c_iflag &= ~(InputFlags.IxOn | InputFlags.IStrip | InputFlags.ICrNl);
            newState.c_cflag |= ControlFlags.Cs8;

            SetAttrs(OptionalActions.TcSaNow, in newState);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetTerm()
        {
            SetAttrs(OptionalActions.TcSaNow, in _termios);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public long ReadByte(out byte b)
        {
            return read(_fileDes, out b, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="termios"></param>
        /// <exception cref="Win32Exception"></exception>
        public void GetAttrs(out Termios termios)
        {
            if (get_attr(_fileDes, out termios) == -1)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="optionalActions"></param>
        /// <param name="termios"></param>
        /// <exception cref="Win32Exception"></exception>
        private void SetAttrs(OptionalActions optionalActions, in Termios termios)
        {
            if (set_attr(_fileDes, optionalActions, in termios) == -1)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="optionalActions"></param>
        /// <param name="modify"></param>
        public void ModifyGlobalAttrs(OptionalActions optionalActions, ModifyAction modify)
        {
            GetAttrs(out Termios termios);
            modify(ref termios);
            SetAttrs(optionalActions, in termios);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="Win32Exception"></exception>
        public void DrainOutput()
        {
            if (tc_drain(_fileDes) == -1)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <exception cref="Win32Exception"></exception>
        public void FlowOutput(LineCtrlFlags action)
        {
            if (tc_flow(_fileDes, action) == -1)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queueSelector"></param>
        /// <exception cref="Win32Exception"></exception>
        public void FlushOutput(LineCtrlFlags queueSelector)
        {
            if (tc_flush(_fileDes, queueSelector) == -1)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Win32Exception"></exception>
        public long GetProcessGroupId()
        {
            var result = tc_get_sid(_fileDes);
            return result == -1 ? throw new Win32Exception(Marshal.GetLastWin32Error()) : result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="duration"></param>
        /// <exception cref="Win32Exception"></exception>
        public void SendBreak(long duration)
        {
            if (tc_send_break(_fileDes, duration) == -1)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// 
        /// </summary>
        public delegate void ModifyAction(ref Termios t);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="function"></param>
        public void StateSandbox(Action function)
        {
            GetAttrs(out Termios termios);
            function();
            SetAttrs(OptionalActions.TcSaNow, in termios);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="fallbackState"></param>
        public void FallbackOnFailure(Func<bool> predicate, ModifyAction fallbackState)
        {
            GetAttrs(out Termios termios);
            if (!predicate())
            {
                SetAttrs(OptionalActions.TcSaNow, in termios);
                ModifyGlobalAttrs(OptionalActions.TcSaNow, fallbackState);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        public void RevertOnFailure(Func<bool> predicate)
        {
            GetAttrs(out Termios prevState);
            if (!predicate())
                SetAttrs(OptionalActions.TcSaNow, in prevState);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GlobalStateString()
        {
            GetAttrs(out var termios);
            return termios.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string OriginalStateString()
        {
            return _termios.ToString();
        }
    }
}