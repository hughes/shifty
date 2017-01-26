using System;
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

    internal enum ShowWindowCommands : int
    {
        Hide = 0,
        Normal = 1,
        Minimized = 2,
        Maximized = 3,
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public ShowWindowCommands showCmd;
        public System.Drawing.Point ptMinPosition;
        public System.Drawing.Point ptMaxPosition;
        public System.Drawing.Rectangle rcNormalPosition;
    }


    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetWindowPlacement(
    IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

    public static WINDOWPLACEMENT GetPlacement(IntPtr hwnd)
    {
        WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
        placement.length = Marshal.SizeOf(placement);
        GetWindowPlacement(hwnd, ref placement);
        return placement;
    }
}
