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
        const int MAX_MESSAGES_COUNT = 32;
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
                manager.ProcessOutput += ManagerProcessOutput;
                manager.ProcessError += ManagerProcessError;
                manager.Controls.ToList().ForEach(c => ManagerControlAdded(this, c.Value));
            }
        }

        void PrintMessage(string message, Color color)
        {
            this.Invoke((MethodInvoker)(() => {
                lock (MessagesHistory)
                {
                    MessagesHistory.Enqueue(message);
                    while(MessagesHistory.Count > MAX_MESSAGES_COUNT)
                    {
                        MessagesHistory.Dequeue();
                    }
                    textBox2.Lines = MessagesHistory.Reverse().ToArray();
                    textBox2.ForeColor = color;
                }        
            }));
        }

        private void ManagerProcessError(string obj)
        {
            PrintMessage(obj, Color.Red);
        }

        private void ManagerProcessOutput(string obj)
        {
            PrintMessage(obj, Color.White);
        }

        private void ManagerNotificationSended(object sender, (string title, string message) e)
        {
            this.Invoke((MethodInvoker)(() => {
                NotifyForm.Instance.Show(e.title, e.message);
            }));
        }

        private void ManagerMessageReceived(object sender, string e)
        {
            PrintMessage(e, Color.White);
        }

        private void panel3_ControlAdded(object sender, ControlEventArgs e)
        {
            e.Control.Dock = DockStyle.Top;
            e.Control.ForeColor = Color.FromArgb(240, 240, 240);
            e.Control.BackColor = panel3.BackColor;
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
            PrintMessage(data, Color.White);
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
                    //textBox2.Text = "";
                    panel3.Controls.Clear();
                    Manager?.Controls?.Clear();
                }

                if (Manager?.Process?.HasExited??true)
                {
                    processIdLabel.Text = "Proccess has exited or not started yet.";
                    processIdLabel.ForeColor = Color.Red;
                }
                else
                {
                    processIdLabel.Text = $"PID: {Manager.Process?.Id.ToString()}";
                    processIdLabel.ForeColor = Color.White;
                }

                pictureBox3.BackColor = Manager.BackgroundColor;
                pictureBox1.Image = Manager.Icon;

                button1.Visible = Manager.Status == ScriptProcessManager.ProcessStatus.Running;
                button2.Visible = Manager.Status == ScriptProcessManager.ProcessStatus.Stopped;
            }
            finally
            {

            }
        }


        public ProcessView(ScriptProcessManager manager)
        {
            InitializeComponent();
            Manager = manager;
            Dock = DockStyle.Top;
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
            Manager.Reset();
        }

        private void panel2_MouseClick(object sender, MouseEventArgs e)
        {
            textBox2.Focus();
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Control)
            {
                
                textBox1.Focus();
                string keyPressed = e.KeyCode.ToString();
                if(keyPressed.Length == 1)
                {
                    textBox1.Text += keyPressed;
                }
                textBox1.SelectionStart = textBox1.Text.Length;
            }
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
