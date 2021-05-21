using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IPAplicatie
{
    class GridItem : LayoutItem
    {
        public readonly static int NumberItems = 5;

        public GridItem(int width, int height, int id, string nameParam, string secondaryParam) : base(width, height, id, nameParam, secondaryParam)
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
            secondary.Top = (int)(width * 0.82) + name.Height;
            secondary.Font = new System.Drawing.Font(FontName, (float)(0.035 * height), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        }
    }
}