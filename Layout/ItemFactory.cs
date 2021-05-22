/**************************************************************************
 *                                                                        *
 *  File:        ItemFactory.cs                                           *
 *  Description: Factory object for generating layout items               *
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
