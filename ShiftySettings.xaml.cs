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
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.ComponentModel;

namespace Shifty
{
    // this is required in order to get the size of the monitor
    static class ExtensionsForWPF
    {
        public static System.Windows.Forms.Screen GetScreen(IntPtr handle)
        {
            return System.Windows.Forms.Screen.FromHandle(handle);
        }
    }

    public class OpenCommand : ICommand
    {
        public void Execute(object parameter)
        {
            NativeMethods.PostMessage(
               (IntPtr)NativeMethods.HWND_BROADCAST,
               NativeMethods.WM_SHOWSHIFTY,
               IntPtr.Zero,
               IntPtr.Zero);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }

    public class ExitCommand : ICommand
    {
        public void Execute(object parameter)
        {
            System.Windows.Application.Current.Shutdown();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }

    public partial class MainWindow : Window
    {
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const short SWP_NOMOVE = 0X2;
        const short SWP_NOSIZE = 1;
        const short SWP_NOZORDER = 0X4;
        const int SWP_SHOWWINDOW = 0x0040;
        const int SW_SHOWNORMAL = 1;

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

            //var exitBinding = new CommandBinding(Commands.Exit, DoExit, CommandCanExecute);
            Assembly assembly = Assembly.GetExecutingAssembly();
            versionText.Text = String.Format("shifty {0}", assembly.GetName().Version.ToString());

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

        private void UpdateScreenSize(IntPtr handle)
        {
            screenSize = ExtensionsForWPF.GetScreen(handle);
        }

        public Action<object, KeyPressedEventArgs> ShifterFunc(Func<int> x, Func<int> y, Func<int> width, Func<int> height)
        {
            Console.WriteLine("Generated shifter function");
            Action<object, KeyPressedEventArgs> shifter = delegate (object sender, KeyPressedEventArgs e)
            {
                // get the handle for the focused window
                var handle = GetForegroundWindow();
                Console.WriteLine("Activated shifter function");
                UpdateScreenSize(handle);
                MoveWindowTo(handle, screenSize.Bounds.Left + x(), screenSize.Bounds.Top + y(), width(), height());
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

        private void MoveWindowTo(IntPtr handle, int x, int y, int width, int height)
        {

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

            // record the placement of the target window
            var placement = NativeMethods.GetPlacement(handle);

            // ensure the window is not maximized
            if (placement.showCmd == NativeMethods.ShowWindowCommands.Maximized)
            {
                Console.WriteLine("Unmaximizing");
                ShowWindow(handle, SW_SHOWNORMAL);
            }

            // move the window to the specified location
            SetWindowPos(handle, 0, x, y, width, height, SWP_NOZORDER | SWP_SHOWWINDOW);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            Console.WriteLine("OnSourceInitialized");
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        protected IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // this gets called a lot
            // Console.WriteLine("Got a message");
            if (msg == NativeMethods.WM_SHOWSHIFTY)
            {
                Console.WriteLine("Totally handled that guys");
                ShowWindow();
                handled = true;
            }
            return IntPtr.Zero;
        }

        private void ShowWindow()
        {
            this.Show();

            // ensure the window is not minimzed
            if (WindowState == WindowState.Minimized)
            {
                Console.WriteLine("Unminimizing");
                WindowState = WindowState.Normal;
                
            }

            // ensure the window is on top
            bool top = Topmost;
            Topmost = true;
            Topmost = top;

            // steal focus
            Activate();
        }

        private void OpenGithub(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;

        }

        protected override void OnStateChanged(EventArgs e)
        {
            if(WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
            base.OnStateChanged(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            base.OnClosing(e);
        }

        private void CommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            Console.WriteLine("someone asked if i can execute");
            e.CanExecute = true;
        }
    }
}
