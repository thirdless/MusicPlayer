using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IPAplicatie
{
    abstract class LayoutItem
    {
        public static readonly string FontName = "Microsoft PhagsPa";

        protected Panel panel;
        protected PictureBox thumbnail;
        protected Label name;
        protected Label secondary;

        protected LayoutItem(int width, int height, int id, string nameParam, string secondaryParam)
        {
            //
            // Panel
            //
            panel = new Panel();
            panel.Name = "panelItem" + id;
            panel.Cursor = Cursors.Hand;

            //
            // Thumbnail
            //
            thumbnail = new PictureBox();
            thumbnail.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            thumbnail.Image = global::IPAplicatie.Properties.Resources.music;
            thumbnail.Name = "imageItem" + id;
            thumbnail.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;

            //
            // Name
            //
            name = new Label();
            name.AutoSize = true;
            name.Name = "labelItemName" + id;
            name.Text = nameParam;
            name.BackColor = System.Drawing.Color.Transparent;
            name.ForeColor = System.Drawing.Color.White;

            //
            // Secondary
            //
            secondary = new Label();
            secondary.AutoSize = true;
            secondary.Name = "labelItemSecondary" + id;
            secondary.Text = secondaryParam;
            secondary.BackColor = System.Drawing.Color.Transparent;
            secondary.ForeColor = System.Drawing.Color.White;

            panel.Controls.Add(thumbnail);
            panel.Controls.Add(name);
            panel.Controls.Add(secondary);
        }

        public void Dispose()
        {
            for (int i = 0; i < panel.Controls.Count; ++i)
            {
                try
                {
                    panel.Controls[i].Dispose();
                }
                catch
                {
                    panel.Controls.RemoveAt(i);
                }
            }

            panel.Dispose();
        }

        public void SetEvents(EventHandler enterEvent, EventHandler leaveEvent, MouseEventHandler rightClick, EventHandler doubleClick)
        {
            panel.MouseEnter += enterEvent;
            panel.MouseLeave += leaveEvent;
            panel.MouseClick += rightClick;
            panel.DoubleClick += doubleClick;

            thumbnail.MouseEnter += enterEvent;
            thumbnail.MouseLeave += leaveEvent;
            thumbnail.MouseClick += rightClick;
            thumbnail.DoubleClick += doubleClick;

            name.MouseEnter += enterEvent;
            name.MouseLeave += leaveEvent;
            name.MouseClick += rightClick;
            name.DoubleClick += doubleClick;

            secondary.MouseEnter += enterEvent;
            secondary.MouseLeave += leaveEvent;
            secondary.MouseClick += rightClick;
            secondary.DoubleClick += doubleClick;
        }

        public Label ItemName
        {
            get { return name; }
        }

        public Label ItemSecondary
        {
            get { return secondary; }
        }

        public Panel Panel
        {
            get { return panel; }
        }
    }
}
