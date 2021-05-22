﻿using System.Drawing;

namespace Layout
{
    public class GridItem : LayoutItem
    {
        public readonly static int NumberItems = 5; // Vor exista 5 elemnte pe un rand

        public GridItem(int width, int height, int id, string nameParam, string secondaryParam, Bitmap bmp) : base(width, height, id, nameParam, secondaryParam, bmp)
        {
            int x = 20 + (width * (id % NumberItems));
            int y = (height * (id / NumberItems));

            panel.Left = x;
            panel.Top = y;
            panel.Width = width;
            panel.Height = height;

            thumbnail.Left = (int)(width * 0.15);
            thumbnail.Top = (int)(width * 0.05);
            thumbnail.Width = (int)(width * 0.7);
            thumbnail.Height = (int)(width * 0.7);

            name.Left = (int)(width * 0.05);
            name.Top = (int)(width * 0.8);
            name.Font = new System.Drawing.Font(FontName, (float)(0.06 * height), System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            name.MaximumSize = new Size(100, (int)(0.22 * height));
            name.AutoSize = true;

            secondary.Left = (int)(width * 0.055);
            secondary.Top = (int)(width * 0.8) + name.Height;
            secondary.Font = new System.Drawing.Font(FontName, (float)(0.035 * height), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        }
    }
}