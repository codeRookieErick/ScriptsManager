using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScriptsManager.Utils
{
    public static class WinApiFunctions
    {
        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint msg, uint wParam, int lParam);

        public static class Screen
        {
            const int SC_MONITORPOWER = 0xF170;
            const int WM_SYSCOMMAND = 0x0112;
            public enum MonitorState
            {
                On = -1,
                Off = 2,
                Standby = 1
            }
            public static void SetStatus(IntPtr windowHandler, MonitorState monitorState)
            {
                SendMessage(windowHandler, WM_SYSCOMMAND, SC_MONITORPOWER, (int)monitorState);
            }
        }
    }
}
