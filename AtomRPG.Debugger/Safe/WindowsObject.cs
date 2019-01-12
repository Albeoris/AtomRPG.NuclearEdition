using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace Memoria.Debugger
{
    internal sealed class WindowsObject
    {
        private readonly IntPtr _handle;

        private WindowsObject(IntPtr handle)
        {
            _handle = handle;
        }

        public Int32 GetProcessId()
        {
            Int32 processId;
            Int32 threadId = User32.GetWindowThreadProcessId(_handle, out processId);
            if (threadId == 0)
                throw new Win32Exception();
            return processId;
        }

        public Boolean Close()
        {
            return User32.CloseWindow(_handle);
        }

        public static WindowsObject Wait(String windowTitle, String[] childTexts, CancellationToken ct, out Int32 result)
        {
            IntPtr handle = IntPtr.Zero;

            Int32 messageIndex = -1;

            User32.EnumWindowsPredicate checkChild = null;
            checkChild = (current, stateRef) =>
            {
                messageIndex = User32.IsTitleEquals(current, childTexts);
                if (messageIndex != -1)
                {
                    handle = current;
                    return false;
                }

                ct.ThrowIfCancellationRequested();
                if (!User32.EnumChildWindows(current, checkChild, IntPtr.Zero) && handle != IntPtr.Zero)
                    return false;

                return true;
            };

            User32.EnumWindowsPredicate check = (current, stateRef) =>
            {
                if (User32.IsTitleEquals(current, windowTitle) != -1)
                {
                    if (!User32.EnumChildWindows(current, checkChild, IntPtr.Zero) && handle != IntPtr.Zero)
                    {
                        handle = current;
                        return false;
                    }
                }

                ct.ThrowIfCancellationRequested();
                return true;
            };

            while (User32.EnumWindows(check, IntPtr.Zero))
                Thread.Sleep(1000);

            result = messageIndex;
            return new WindowsObject(handle);
        }
    }
}