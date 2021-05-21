using System.Drawing;

namespace Layout
{
    public class ListItem : LayoutItem
    {
        public ListItem(int width, int height, int id, string nameParam, string secondaryParam, Bitmap bmp) : base(width, height, id, nameParam, secondaryParam, bmp)
        {
            int x = 20;
            int y = height * id;

            panel.Left = x;
            panel.Top = y;
            panel.Width = width;
            panel.Height = height;

            thumbnail.Left = (int)(height * 0.1);
            thumbnail.Top = (int)(height * 0.1);
            thumbnail.Width = (int)(height * 0.8);
            thumbnail.Height = (int)(height * 0.8);

            name.Left = (int)(height * 1);
            name.Top = (int)(height * 0.2);
            name.Font = new System.Drawing.Font(FontName, (float)(0.2 * height), System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

            secondary.Left = (int)(height * 1.03);
            secondary.Top = (int)(height * 0.2) + name.Height;
            secondary.Font = new System.Drawing.Font(FontName, (float)(0.15 * height), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        }
    }
}