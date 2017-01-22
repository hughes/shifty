﻿using System;
using System.Runtime.InteropServices;

// this class just wraps some Win32 stuffthat we're going to use
internal class NativeMethods
{
    public const int HWND_BROADCAST = 0xffff;
    public static readonly int WM_SHOWSHIFTY = RegisterWindowMessage("WM_SHOWSHIFTY");
    [DllImport("user32")]
    public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
    [DllImport("user32")]
    public static extern int RegisterWindowMessage(string message);
}
