using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScriptsManager
{
    public partial class NotifyForm : Form
    {
        protected NotifyForm()
        {
            InitializeComponent();
            ShowInTaskbar = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Hide();
        }

        static NotifyForm notifyForm;
        public static NotifyForm Instance {
            get
            {
                return notifyForm ?? (notifyForm = new NotifyForm());
            }   
        }

        bool hideCanceled = false;
        public void Show(string title, string message, int seconds = 5)
        {
            label1.Text = title;
            label2.Text = message;
            Show();
            new Thread(() => {
                Thread.Sleep(seconds * 1000);
                this.Invoke((MethodInvoker)(() => {
                    if(!hideCanceled)this.Hide();
                }));
            }).Start();
        }

       

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            float x = Screen.PrimaryScreen.WorkingArea.Width * 0.75F;
            float y = Screen.PrimaryScreen.WorkingArea.Height * 0.75F;
            Location = new Point((int)x, (int)y);
            Width = (int)(Screen.PrimaryScreen.WorkingArea.Width - x);
            Height = (int)(Screen.PrimaryScreen.WorkingArea.Height - y) - 20;
        }

        private void NotifyForm_MouseEnter(object sender, EventArgs e)
        {
            hideCanceled = true;
        }

        private void NotifyForm_MouseMove(object sender, MouseEventArgs e)
        {
            hideCanceled = true;

        }
    }
}
