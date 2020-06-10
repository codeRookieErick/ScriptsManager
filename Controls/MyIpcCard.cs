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
