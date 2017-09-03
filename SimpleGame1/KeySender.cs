using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleGame1
{
    internal class KeySender
    {
        public string WindowName { get; set; }

        [DllImport("user32.dll")]
        public static extern int FindWindow(
            string lpClassName, // class name 
            string lpWindowName // window name 
            );

        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(
            int hWnd // handle to window
            );

        public KeySender(string windowName)
        {
            WindowName = string.IsNullOrEmpty(windowName) ? "Untitled - notepad" : windowName;
        }

        public void send(string key)
        {
            var iHandle = FindWindow(null, WindowName);
            SetForegroundWindow(iHandle);
            SendKeys.SendWait(key);
        }
    }
}
