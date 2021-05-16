using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IPAplicatie
{
    class PlaylistsListView
    {
        MainForm _mainForm;
        Panel _panel;
        ListItem[] _items;

        private readonly static int _height = 100;

        private readonly static int _itemsNo = 11;

        public PlaylistsListView(MainForm form, Panel panel)
        {
            _mainForm = form;
            _panel = panel;
        }

        public void CreatePlaylists()
        {
            _items = new ListItem[_itemsNo];

            for (int i = 0; i < _itemsNo; i++)
            {
                _items[i] = new ListItem(20, _height * i, _panel.Width, _height, i, "Playlist #" + (i + 1), "cateva melodii • 120 de minute", new EventHandler(ClickEvent), null);
                _panel.Controls.Add(_items[i].Panel);
            }

            _panel.Height = _height * _itemsNo;
        }

        public void Dispose()
        {
            _panel.Dispose();
        }

        private void ClickEvent(object sender, EventArgs e)
        {
            Panel parent = null;

            try
            {
                parent = (Panel)sender;
            }
            catch (Exception ex)
            {
                try
                {
                    parent = (Panel)((Label)sender).Parent;
                }
                catch (Exception exc)
                {
                    parent = (Panel)((PictureBox)sender).Parent;
                }
            }

            if (parent != null)
            {
                string id = parent.Name.Substring("panelItem".Length),
                    name = parent.Controls.Find("labelItemName" + id, false)[0].Text,
                    secondary = parent.Controls.Find("labelItemSecondary" + id, false)[0].Text;
            }
        }

        private void DoubleClickEvent(object sender, EventArgs args)
        { 
        
        }
    }
}
