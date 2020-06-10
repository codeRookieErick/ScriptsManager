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
