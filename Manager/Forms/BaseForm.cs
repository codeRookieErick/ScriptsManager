using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScriptsManager.Forms
{
    public partial class BaseForm : Form
    {
        new string Text
        {
            get => label1.Text;
            set => label1.Text = value;
        }
        public Color FormColor
        {
            get => panel1.BackColor;
            set => panel1.BackColor = value;
        }
        public Color FormForeColor
        {
            get => panel1.ForeColor;
            set => panel1.ForeColor = value;
        }
        public Image FormIcon { get => pictureBox1.Image; set => pictureBox1.Image = value; }


        (string message, Color color, Image icon) status;
        public (string message, Color color, Image icon) Status
        {
            get => status;
            set
            {
                status.message = value.message ?? "...";
                status.color = value.color == Color.Empty ? Color.FromArgb(0, 122, 204) : value.color;
                status.icon = value.icon ?? new Bitmap(1,1);
                label2.Text = status.message;
                panel2.BackColor = status.color;
                pictureBox2.Image = status.icon;
            }
        }

        public BaseForm()
        {
            InitializeComponent();
            Extenssions.GetChildsIterative(this).ToList().ForEach(c =>
                { 
                    c.MouseMove += (o, e) => CheckMovement(); 
                }           
            );
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }


        Point LastLocation;
        void CheckMovement()
        {
            if (LastLocation != default)
            {
                Point delta = new Point(MousePosition.X - LastLocation.X, MousePosition.Y - LastLocation.Y);
                if (MouseButtons == MouseButtons.Left)
                {
                    var location = new Point(Location.X + delta.X, Location.Y + delta.Y);
                    Location = location;
                }
            }
            LastLocation = MousePosition;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            CheckMovement();   
            base.OnMouseMove(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            UpdateToggleMaximizedButton();
        }

        void UpdateToggleMaximizedButton()
        {
            button2.Text = WindowState == FormWindowState.Maximized ? "V" : "□";
        }

        private void ToggleMaximized(object sender, EventArgs e)
        {
            WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
            UpdateToggleMaximizedButton();
        }

        private void Minimize(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }
    }

    public static class Extenssions
    {
        public static IEnumerable<Control> GetChildsIterative(this Control self)
        {
            foreach(var c in self.Controls.OfType<Control>())
            {
                foreach(var child in c.GetChildsIterative())
                {
                    yield return child;
                }
                yield return c;
            }
        }
    }
}