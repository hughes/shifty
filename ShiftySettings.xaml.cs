using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Shifty
{
    // this is required in order to get the size of the monitor
    static class ExtensionsForWPF
    {
        public static System.Windows.Forms.Screen GetScreen(this Window window)
        {
            return System.Windows.Forms.Screen.FromHandle(new WindowInteropHelper(window).Handle);
        }
    }

    public partial class MainWindow : Window
    {
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        const short SWP_NOMOVE = 0X2;
        const short SWP_NOSIZE = 1;
        const short SWP_NOZORDER = 0X4;
        const int SWP_SHOWWINDOW = 0x0040;

        KeyboardHook LeftHook = new Shifty.KeyboardHook();
        KeyboardHook RightHook = new Shifty.KeyboardHook();
        KeyboardHook TopHook = new Shifty.KeyboardHook();
        KeyboardHook BottomHook = new Shifty.KeyboardHook();

        KeyboardHook TopLeftHook = new Shifty.KeyboardHook();
        KeyboardHook TopRightHook = new Shifty.KeyboardHook();
        KeyboardHook BottomLeftHook = new Shifty.KeyboardHook();
        KeyboardHook BottomRightHook = new Shifty.KeyboardHook();

        KeyboardHook CenterHook = new Shifty.KeyboardHook();

        private Screen screenSize;

        public MainWindow()
        {
            InitializeComponent();
            Console.WriteLine("Initialized");

            ModifierKeys modifier = ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Win;
            
            // todo: clean this up and load settings from file
            LeftHook.KeyPressed += new EventHandler<KeyPressedEventArgs>(
                ShifterFunc(Zero, Zero, Half(GetWidth), GetHeight));
            LeftHook.RegisterHotKey(modifier, Keys.NumPad4);

            RightHook.KeyPressed += new EventHandler<KeyPressedEventArgs>(
                ShifterFunc(Half(GetWidth), Zero, Half(GetWidth), GetHeight));
            RightHook.RegisterHotKey(modifier, Keys.NumPad6);

            TopHook.KeyPressed += new EventHandler<KeyPressedEventArgs>(
                ShifterFunc(Zero, Zero, GetWidth, Half(GetHeight)));
            TopHook.RegisterHotKey(modifier, Keys.NumPad8);

            BottomHook.KeyPressed += new EventHandler<KeyPressedEventArgs>(
                ShifterFunc(Zero, Half(GetHeight), GetWidth, Half(GetHeight)));
            BottomHook.RegisterHotKey(modifier, Keys.NumPad2);

            TopLeftHook.KeyPressed += new EventHandler<KeyPressedEventArgs>(
                ShifterFunc(Zero, Zero, Half(GetWidth), Half(GetHeight)));
            TopLeftHook.RegisterHotKey(modifier, Keys.NumPad7);

            TopRightHook.KeyPressed += new EventHandler<KeyPressedEventArgs>(
                ShifterFunc(Half(GetWidth), Zero, Half(GetWidth), Half(GetHeight)));
            TopRightHook.RegisterHotKey(modifier, Keys.NumPad9);

            BottomLeftHook.KeyPressed += new EventHandler<KeyPressedEventArgs>(
                ShifterFunc(Zero, Half(GetHeight), Half(GetWidth), Half(GetHeight)));
            BottomLeftHook.RegisterHotKey(modifier, Keys.NumPad1);

            BottomRightHook.KeyPressed += new EventHandler<KeyPressedEventArgs>(
                ShifterFunc(Half(GetWidth), Half(GetHeight), Half(GetWidth), Half(GetHeight)));
            BottomRightHook.RegisterHotKey(modifier, Keys.NumPad3);

            CenterHook.KeyPressed += new EventHandler<KeyPressedEventArgs>(
                ShifterFunc(Half(Half(GetWidth)), Half(Half(GetHeight)), Half(GetWidth), Half(GetHeight)));
            CenterHook.RegisterHotKey(modifier, Keys.NumPad5);

        }

        private void UpdateScreenSize()
        {
            screenSize = ExtensionsForWPF.GetScreen(this);
        }

        public Action<object, KeyPressedEventArgs> ShifterFunc(Func<int> x, Func<int> y, Func<int> width, Func<int> height)
        {
            Console.WriteLine("Generated shifter function");
            Action<object, KeyPressedEventArgs> shifter = delegate (object sender, KeyPressedEventArgs e)
            {
                Console.WriteLine("Activated shifter function");
                UpdateScreenSize();
                MoveWindowTo(x(), y(), width(), height());
            };

            return shifter;
        }

        private int Zero()
        {
            return 0;
        }
        
        private int GetWidth()
        {
            return (int)screenSize.WorkingArea.Width;
        }
        
        private int GetHeight()
        {
            return (int)screenSize.WorkingArea.Height;
        }

        private Func<int> Half(Func<int> getValue)
        {
            // is this really... reasonable?
            return delegate ()
            {
                return getValue() / 2;
            };
        }

        private void MoveWindowTo(int x, int y, int width, int height)
        {
            // get the handle for the focused window
            var handle = GetForegroundWindow();

            // attempt to log the action
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                String windowName = Buff.ToString();
                String msg = String.Format("Moved window named {0} to position: {1}, {2}, {3}, {4}",
                    windowName, x, y, width, height);
                Console.WriteLine(msg);
            }

            // move the window to the specified location
            SetWindowPos(handle, 0, x, y, width, height, SWP_NOZORDER | SWP_SHOWWINDOW);
        }

        private void OpenGithub(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

    }
}
