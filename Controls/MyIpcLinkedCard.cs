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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Controls
{
    public class MyIpcLinkedCard:MyIpcCard
    {
        public string Command { get; set; }
        public string Arguments { get; set; }

        public string LinkTitle { get => linkLabel.Text; set => linkLabel.Text = value; }
        LinkLabel linkLabel;

        public MyIpcLinkedCard()
        {
            Controls.Add(linkLabel = new LinkLabel { 
            
            });
            linkLabel.Click += LinkLabel_Click;
        }

        private void LinkLabel_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo { 
                FileName = Command,
                Arguments = Arguments
            });
        }
    }
}
