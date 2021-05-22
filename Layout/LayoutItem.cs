/**************************************************************************
 *                                                                        *
 *  File:        LayoutItem.cs                                            *
 *  Description: Abstract object that will aid us into setting the        *
 *               layout for the graphical user interface                  *
 *                                                                        *
 *  This program is free software; you can redistribute it and/or modify  *
 *  it under the terms of the GNU General Public License as published by  *
 *  the Free Software Foundation. This program is distributed in the      *
 *  hope that it will be useful, but WITHOUT ANY WARRANTY; without even   *
 *  the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR   *
 *  PURPOSE. See the GNU General Public License for more details.         *
 *                                                                        *
 **************************************************************************/

using System;
using System.Windows.Forms;
using System.Drawing;

namespace Layout
{
    public abstract class LayoutItem
    {
        public static readonly string FontName = "Microsoft PhagsPa";

        protected Panel panel;
        protected PictureBox thumbnail;
        protected Label name;
        protected Label secondary;

        // Un obiect de tip LayoutItem va fi compus din: 
        //  - Un panou ce va contine elemntele din obiect
        //  - Un obiect de tip pictureBox pentru afisarea unei imagini (Nu este implementata deocamdata)
        //  - Doua obiecte de tip label unul pentru numele obiectului si celalalt pentru detalii despre obiect
        protected LayoutItem(int width, int height, int id, string nameParam, string secondaryParam, Bitmap bmp)
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
            thumbnail.Image = bmp;
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

        // Metoda de eliminare a elementelor din obiect
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

        // Metoda de setare a evenimentelor unui obiect
        // Am creat-o pentru a simplifica generarea unui obiect din aceast tip de clasa 
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
