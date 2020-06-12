    /*
    ScriptsManager, Administrador de scripts
    Copyright (C) 2020 Erick Mora

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.

    erickfernandomoraramirez@gmail.com
    erickmoradev@gmail.com
    https://dev.moradev.dev/myportfolio
    */

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
