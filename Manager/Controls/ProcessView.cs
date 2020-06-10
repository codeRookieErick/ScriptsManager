using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScriptsManager.Utils;

namespace ScriptsManager.Controls
{
    public partial class ProcessView : UserControl
    {
        RotableCollection<string> CommandsHistory = new RotableCollection<string>();
        Queue<string> MessagesHistory = new Queue<string>();
        public ProcessView()
        {
            InitializeComponent();
        }

        public bool Running
        {
            get => timer1.Enabled;
            set => timer1.Enabled = value;
        }

        ScriptProcessManager manager;
        public ScriptProcessManager Manager
        {
            get
            {
                return manager;
            }
            set
            {
                if (manager == value) return;
                if(manager != null)
                {
                    manager.ValueChanged -= ManagerValueChanged;
                    manager.DataReceived -= ManagerDataReceived;
                    manager.ControlAdded -= ManagerControlAdded;
                    manager.ControlRemoved -= ManagerControlRemoved;
                    manager.MessageReceived -= ManagerMessageReceived;
                    manager.NotificationSended -= ManagerNotificationSended;
                    panel3.Controls.Clear();
                }
                manager = value;
                if (value == null) return;
                manager.ValueChanged += ManagerValueChanged;
                manager.DataReceived += ManagerDataReceived;
                manager.ControlAdded += ManagerControlAdded;
                manager.ControlRemoved += ManagerControlRemoved;
                manager.MessageReceived += ManagerMessageReceived;
                manager.NotificationSended += ManagerNotificationSended;
                manager.Controls.ToList().ForEach(c => ManagerControlAdded(this, c.Value));
            }
        }

        private void ManagerNotificationSended(object sender, (string title, string message) e)
        {
            this.Invoke((MethodInvoker)(() => {
                NotifyForm.Instance.Show(e.title, e.message);
            }));
        }

        private void ManagerMessageReceived(object sender, string e)
        {
            MessagesHistory.Enqueue(e);
            while (MessagesHistory.Count > 10) MessagesHistory.Dequeue();
            this.Invoke((MethodInvoker)(() => {
                UpdateMessagesView();
            }));
        }

        private void panel3_ControlAdded(object sender, ControlEventArgs e)
        {
            e.Control.Dock = DockStyle.Top;
        }

        private void ManagerControlAdded(object sender, Control e)
        {
            this.Invoke((MethodInvoker)(() => {
                panel3.Controls.Add(e);
            }));
        }
        private void ManagerControlRemoved(object sender, Control e)
        {
            this.Invoke((MethodInvoker)(() => {
                if (panel3.Controls.Contains(e)) panel3.Controls.Remove(e);
            }));
        }

        private void ManagerDataReceived(object sender, string e)
        {
            this.Invoke((MethodInvoker)(() => {
                manager.Execute(e);
            }));
        }

        void Send(string data)
        {
            this.manager?.Send(data);
        }

        private void ManagerValueChanged(object sender, string e)
        {
            UpdateProcessData();
        }

        public void UpdateProcessData()
        {
            try
            {
                if(Manager.Status == ScriptProcessManager.ProcessStatus.Running)
                {
                    label1.Text = string.Format("{0}", Manager.GetValue("name", "..."));
                    label2.Text = string.Format("({0})", Manager.GetValue("filename", "..."));
                    label4.Text = 
                        string.Format(
                            "Working directory: {0} => Script: {1}",
                            Manager.GetValue("workingDirectory", "..."),
                            Manager.GetValue("script", "...")
                            );
                    label5.Text = string.Format("Processor time: {0} ms", Manager.Process?.TotalProcessorTime);
                    label6.Text = string.Format("Start time: {0}", Manager.Process?.StartTime);

                }
                else
                {
                    label1.Text = string.Format("{0}", Manager.GetValue("name", "..."));
                    label2.Text = string.Format("({0})", Manager.GetValue("filename", "..."));
                    label4.Text = string.Format("Working directory: {0}", Manager.GetValue("workingDirectory", "..."));
                    label5.Text = string.Format("Processor time: not running");
                    label6.Text = string.Format("Start time: not running", Manager.Process?.StartTime);
                    textBox2.Text = "";
                    panel3.Controls.Clear();
                    Manager?.Controls?.Clear();
                }

                if (Manager?.Process.HasExited??true)
                {
                    label7.Text = "Proccess has exited or not started yet.";
                    label7.ForeColor = Color.Red;
                }
                else
                {
                    label7.Text = $"PID: {Manager.Process?.Id.ToString()}";
                    label7.ForeColor = Color.Black;
                }

                pictureBox2.BackColor = pictureBox3.BackColor = Manager.BackgroundColor;
                pictureBox1.Image = Manager.Icon;

                button1.Visible = Manager.Status == ScriptProcessManager.ProcessStatus.Running;
                button2.Visible = Manager.Status == ScriptProcessManager.ProcessStatus.Stopped;
            }
            finally
            {

            }
        }

        void UpdateMessagesView()
        {
            textBox2.Text = "";
            textBox2.Lines = MessagesHistory.ToArray();
            //MessagesHistory.ForEach(m => textBox2.Text = m + textBox2.Text);
        }

        public ProcessView(ScriptProcessManager manager)
        {
            InitializeComponent();
            Manager = manager;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            UpdateProcessData();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SendCommand();
        }

        void SendCommand()
        {
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                while (CommandsHistory.Count > 100) CommandsHistory.RemoveAt(0);
                Send(textBox1.Text);
                CommandsHistory.Add(textBox1.Text);
                textBox1.Text = string.Empty;
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter) SendCommand();
            if (e.KeyData == Keys.Up)
            {
                textBox1.Text = CommandsHistory.LastOrDefault() ?? "";
                CommandsHistory.Rotate(1);
            }
            if (e.KeyData == Keys.Down)
            {
                textBox1.Text = CommandsHistory.LastOrDefault() ?? "";
                CommandsHistory.Rotate(-1);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Manager.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Manager.Kill();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            manager.Reset();
        }
    }

    class RotableCollection<T> : List<T>
    {
        int max;
        public RotableCollection(int max = 100)
        {
            this.max = max;
        }

        public new void Add(T element)
        {
            while (Count > max) RemoveAt(0);
            base.Add(element);
        }

        public void Rotate(int index)
        {
            if (Count == 0) return;
            if (index > 0)
            {
                for (; index > 0; index--)
                {
                    T val = this.Last();
                    RemoveAt(Count - 1);
                    Reverse();
                    Add(val);
                    Reverse();
                }
            }
            else
            {
                for (; index < 0; index++)
                {
                    Reverse();
                    T val = this.Last();
                    RemoveAt(Count - 1);
                    Reverse();
                    Add(val);
                }
            }
        }
    }

}
