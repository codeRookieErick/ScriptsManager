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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Controls
{
    public class MyIpcCard:MyCard
    {
        Label contentLabel;
        public string Content
        {
            get => contentLabel?.Text ?? "";
            set
            {
                if (contentLabel.Text != null)
                {
                    contentLabel.Text = value;
                }
            }
        }
        public MyIpcCard()
        {

            Controls.Add(contentLabel = new Label()
            {
                AutoSize = true,
                Padding = new Padding(8)
            });
        }

    }

    public class MyCard : Panel
    {
        protected Label titleLabel;
        public string Title
        {
            get => titleLabel?.Text ?? "";
            set
            {
                if (titleLabel.Text != null)
                {
                    titleLabel.Text = value;
                }
            }
        }
        public MyCard()
        {
            this.Dock = DockStyle.Top;

            Controls.Add(titleLabel = new Label()
            {
                AutoSize = true,
                Padding = new Padding(8)
            });
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            UpdateControls();
            if (titleLabel != null)
            {
                titleLabel.Font = new System.Drawing.Font(titleLabel.Font, System.Drawing.FontStyle.Bold);
            }

        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            e.Control.Dock = DockStyle.Top;
            e.Control.BringToFront();
            UpdateControls();
        }

        void UpdateControls()
        {
            int heigth = this.Padding.Vertical;
            if(this.Controls.Count > 0)
            {
                heigth += Controls.OfType<Control>().Select(c => c.Height).Sum();
            }
            this.Height = heigth;
        }
    }
}
