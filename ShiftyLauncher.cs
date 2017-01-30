using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using System.Reflection;

namespace Shifty
{
    class ShiftyLauncher
    {
        static string appGuid =
           ((GuidAttribute)Assembly.GetExecutingAssembly().
               GetCustomAttributes(typeof(GuidAttribute), false).
                   GetValue(0)).Value.ToString();

        static string mutextId = string.Format("Global\\{{{0}}}", appGuid);
        static Mutex mutex = new Mutex(true, mutextId);

        [STAThread]
        public static void Main()
        {
            int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine(string.Format("launcher thread id: {0}", threadId));
            // if the mutex is aquired, there is no app to switch to
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Console.WriteLine("Got mutex, launching");
                App app = new App();
                app.InitializeComponent();
                app.Run();
                
                mutex.ReleaseMutex();
            }

            // otherwise emit a signal that the running app should listen to
            // when the app hears the signal, it will focus the window
            else
            {
                Console.WriteLine("Couldn't get mutex, broadcasting");
                NativeMethods.PostMessage(
                    (IntPtr)NativeMethods.HWND_BROADCAST,
                    NativeMethods.WM_SHOWSHIFTY,
                    IntPtr.Zero,
                    IntPtr.Zero);
            }

            Environment.Exit(0);
        }
    }
}
