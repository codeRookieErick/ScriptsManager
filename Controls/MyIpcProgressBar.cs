using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Controls
{
    public class MyIpcProgressBar : Panel
    {
        float maximun = 0;
        public float Maximun { get => maximun; set { maximun = value; UpdateBar(); } }
        float value = 0;
        public float Value { get => value; set { this.value = value; UpdateBar(); } }

        public float Percentage { get => maximun == 0 ? 0F : value / maximun; }

        public Color BarBackColor { get => bar.BackColor; set => bar.BackColor = value; }

        Color barBarColor = Color.DodgerBlue;
        public Color BarBarColor { get => barBarColor; set { bar.BackColor = value; UpdateBar(); } }

        public string Title { get => titleLabel.Text; set => titleLabel.Text = value; }

        Label titleLabel, messageLabel;
        PictureBox bar;
        void UpdateBar()
        {
            Brush bruss = new SolidBrush(barBarColor);
            Graphics g = bar.CreateGraphics();
            g.Clear(bar.BackColor);
            g.FillRectangle(bruss, 0, 0, Percentage * bar.Width , bar.Height);
            messageLabel.Text = $"{value}/{maximun} ({Percentage * 100}%)";
        }
        public MyIpcProgressBar()
        {
            Padding = new Padding(8);
            Panel panel = new Panel() { Dock = DockStyle.Fill };
            panel.Controls.Add(messageLabel = new Label { Dock = DockStyle.Bottom });
            panel.Controls.Add(bar = new PictureBox { Dock = DockStyle.Top, Height = 20 });
            panel.Controls.Add(titleLabel = new Label { Dock = DockStyle.Top });
            Controls.Add(panel);
            Title = "---";
            BackColor = Color.FromArgb(240, 240, 240);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            this.Height = this.Padding.Vertical + titleLabel.Height + messageLabel.Height + bar.Height;
            titleLabel.Font = new Font(titleLabel.Font, FontStyle.Bold);
        }
    }
}
