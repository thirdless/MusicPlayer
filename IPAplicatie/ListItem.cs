using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IPAplicatie
{
    class ListItem
    {
        Panel _panel;
        PictureBox _thumbnail;
        Label _name;
        Label _secondary;
        static private string _fontName = "Microsoft PhagsPa";

        public ListItem(int x, int y, int width, int height, int id, string name, string secondary, EventHandler click, EventHandler doubleClick, int style = 0)
        {
            if (style == 0)
            {

            }
            else if (style == 1)
            {

            }
            else
            {

            }
            //
            // Panel
            //
            _panel = new Panel();
            _panel.Name = "panelItem" + id;
            _panel.Left = x;
            _panel.Top = y;
            _panel.Width = width;
            _panel.Height = height;
            _panel.Click += click;
            _panel.DoubleClick += doubleClick;
            _panel.Cursor = Cursors.Hand;

            //
            // Thumbnail
            //
            _thumbnail = new PictureBox();
            _thumbnail.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            _thumbnail.Image = global::IPAplicatie.Properties.Resources.music;
            _thumbnail.Name = "imageItem" + id;
            _thumbnail.Left = (int)(height * 0.1);
            _thumbnail.Top = (int)(height * 0.1);
            _thumbnail.Width = (int)(height * 0.8);
            _thumbnail.Height = (int)(height * 0.8);
            _thumbnail.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            _thumbnail.Click += click;
            _thumbnail.DoubleClick += doubleClick;

            //
            // Name
            //
            _name = new Label();
            _name.AutoSize = true;
            _name.Name = "labelItemName" + id;
            _name.Text = name;
            _name.Left = (int)(height * 1);
            _name.Top = (int)(height * 0.2);
            _name.BackColor = System.Drawing.Color.Transparent;
            _name.Font = new System.Drawing.Font(_fontName, 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            _name.ForeColor = System.Drawing.Color.White;
            _name.Click += click;
            _name.DoubleClick += doubleClick;

            //
            // Secondary
            //
            _secondary = new Label();
            _secondary.AutoSize = true;
            _secondary.Name = "labelItemSecondary" + id;
            _secondary.Text = secondary;
            _secondary.Left = (int)(height * 1.03);
            _secondary.Top = (int)(height * 0.3) + _name.Height;
            _secondary.BackColor = System.Drawing.Color.Transparent;
            _secondary.Font = new System.Drawing.Font(_fontName, 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            _secondary.ForeColor = System.Drawing.Color.White;
            _secondary.Click += click;
            _secondary.DoubleClick += doubleClick;

            _panel.Controls.Add(_thumbnail);
            _panel.Controls.Add(_name);
            _panel.Controls.Add(_secondary);
        }

        public void Dispose()
        {
            for (int i = 0; i < _panel.Controls.Count; ++i)
            {
                try
                {
                    _panel.Controls[i].Dispose();
                }
                catch (Exception ex)
                {
                    _panel.Controls.RemoveAt(i);
                }
            }

            _panel.Dispose();
        }
    
        public Label ItemName
        {
            get { return _name; }
        }

        public Label ItemSecondary
        {
            get { return _secondary; }
        }

        public Panel Panel
        {
            get { return _panel; }
        }
    }
}
