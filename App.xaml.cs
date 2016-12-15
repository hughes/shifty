using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.InteropServices;

namespace Shifty
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);


        protected override void OnStartup(StartupEventArgs e)
        {
            Console.WriteLine("hello");
            //var HotKeyManager = new MainWindow();

            // RegisterHotKey (Hangle, Hotkey Identifier, Modifiers, Key)
            //RegisterHotKey(HotKeyManager.Handle, 123, Constants.ALT + Constants.SHIFT, (int)Keys.P);
            //RegisterHotKey(HotKeyManager.Handle, 234, Constants.ALT + Constants.SHIFT, (int)Keys.O);
        }

    }

}
