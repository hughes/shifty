using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Shifty
{
    class ShiftyLauncher
    {
        static Mutex mutex = new Mutex(true, "ShiftyMutex");

        [STAThread]
        public static void Main()
        {
            // if the mutex is aquired, there is no app to switch to
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Console.WriteLine("App not yet running");
                App app = new App();
                app.InitializeComponent();
                app.Run();

                Console.WriteLine("App closed, releasing mutex");
                mutex.ReleaseMutex();
            }

            // otherwise emit a signal that the running app should listen to
            // when the app hears the signal, it will focus the window
            else
            {
                Console.WriteLine("Mutex unavailable, switching windows");
                NativeMethods.PostMessage(
                    (IntPtr)NativeMethods.HWND_BROADCAST,
                    NativeMethods.WM_SHOWSHIFTY,
                    IntPtr.Zero,
                    IntPtr.Zero);
            }
        }
    }
}
