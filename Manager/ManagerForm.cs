using ScriptsManager.Controls;
using ScriptsManager.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SIGN = ScriptsManager.Utils.ScriptsAdministrator.ScriptManagerSignal;

namespace ScriptsManager
{
    public partial class ManagerForm : Form
    {
        string definitionsPath = ".\\scripts\\_definitions";
        int consolePort = 45047;
        int managerPort = 45048;

        RotableCollection<EventHandler> eventHandlers = new RotableCollection<EventHandler>();
        ScriptsAdministrator Administrator;
        LimitedQueue<string> CommandsHistory = new LimitedQueue<string>(32);
        public ManagerForm()
        {
            InitializeComponent();
            definitionsPath = ConfigurationManager.AppSettings["definitionsPath"];
            if(string.IsNullOrEmpty(definitionsPath)) definitionsPath = ".\\scripts\\_definitions";

            if (int.TryParse(ConfigurationManager.AppSettings["managerPort"], out int managerPort))
            {
                this.managerPort = managerPort;
            }
            if (int.TryParse(ConfigurationManager.AppSettings["consolePort"], out int consolePort))
            {
                this.consolePort = consolePort;
            }


            Text += $" (console : {consolePort}, manager : {managerPort})";
            Administrator = new ScriptsAdministrator(consolePort, managerPort, definitionsPath);
            Administrator.OnScriptAdded += ScriptsAdministratorOnScriptAdded;
            Administrator.Print += ScriptsAdministratorPrint;
            Administrator.OnAdministratorSignalReceived += ScriptsAdministratorOnAdministratorSignalReceived;
        }

        private void ScriptsAdministratorOnAdministratorSignalReceived(object sender, string e)
        {
            switch (e)
            {
                case SIGN.BRING_FRONT:
                    BringToFront();
                    break;
                case SIGN.KILL_MANAGER:
                    Close();
                    break;
                case SIGN.RESTART_MANAGER:
                    Application.Restart();
                    break;
                case SIGN.SHOW_NEXT:
                    timer2_Tick(sender, EventArgs.Empty);
                    break;
                case SIGN.START_PRESENTATION:
                    timer2.Enabled = true;
                    break;
                case SIGN.STOP_PRESENTATION:
                    timer2.Enabled = false;
                    break;
            }
        }

        private void ScriptsAdministratorPrint(object sender, (string message, bool output) e)
        {
            string data = e.message;
            CommandsHistory.Enqueue(data);
            this.Invoke((MethodInvoker)(() => {
                textBox3.Lines = CommandsHistory.ToArray();
            }));
        }

        private void ScriptsAdministratorOnScriptAdded(object sender, ScriptProcessManager e)
        {
            this.Invoke((MethodInvoker)(() => {
                OnScriptAdded(e);
            }));
        }

        public void OnScriptAdded(ScriptProcessManager scriptProcessManager)
        {
            Label label = new Label
            {
                Text = scriptProcessManager.GetValue("name", "..."),
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoEllipsis = true,
                Padding = new Padding(4)

            };

            ProcessView processView = new ProcessView(scriptProcessManager) { Dock = DockStyle.Fill, Visible = false };
            panel3.Controls.Add(processView);
            EventHandler labelClick = (o, e) => {
                panel2.Controls.OfType<Label>().ToList().ForEach(l => {
                    l.ForeColor = l == label ? Color.FromArgb(0, 122, 204):Color.White;
                });
                panel3.Controls.OfType<ProcessView>().ToList().ForEach(c => {
                    if (c == processView)
                    {
                        c.Running = true;
                        c.Visible = true;
                    }
                    else
                    {
                        c.Running = false;
                        c.Visible = false;
                    }
                });
            };
            label.Click += labelClick;
            eventHandlers.Add(labelClick);
            panel2.Controls.Add(label);
        }

        private void ManagerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Administrator.Dispose();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer2.Enabled = !timer2.Enabled;
            button1.Text = timer2.Enabled ? "Detener Presentacion (5 seg) -->" : "Presentacion -->";
            if (timer2.Enabled) timer2_Tick(sender, e);
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            eventHandlers.Rotate(1);
            if(eventHandlers.Count > 0)
            {
                eventHandlers[0](sender, e);
            }
        }

        private void ManagerForm_Load(object sender, EventArgs e)
        {
            button1.PerformClick();
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                if (!string.IsNullOrEmpty(textBox2.Text))
                {
                    Administrator.OnCommandReceived(textBox2.Text.Replace(Environment.NewLine, " "));
                    textBox2.Text = "";
                }
            }
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            textBox2.Focus();
        }

        private void textBox1_TextChanged2(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(textBox1.Text))
            {
                panel2.Controls.OfType<Label>().ToList().ForEach(l => l.Visible = true);
            }
            else
            {
                panel2.Controls.OfType<Label>().ToList().ForEach(l => l.Visible = l.Text.ToLower().Contains(textBox1.Text.ToLower()));
            }
        }

        private void textBox3_Click(object sender, EventArgs e)
        {
            
            textBox2.Focus();
        }

        private void panel2_MouseClick(object sender, MouseEventArgs e)
        {
            textBox1.Focus();
        }
    }
}
