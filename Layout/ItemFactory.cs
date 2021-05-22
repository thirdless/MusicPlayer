﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Layout
{
    public class ItemFactory
    {
        public LayoutItem GetItem(int type, int width, int height, int id, string name, string secondary, Bitmap bmp)
        {
            switch (type)
            {
                case 0:
                    return new ListItem(width, height, id, name, secondary, bmp);
                case 1:
                    return new GridItem(width, height, id, name, secondary, bmp);
                default:
                    return null;
            }
        }
    }
}
