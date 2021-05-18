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

        public void CreatePlaylists(Dictionary<string, string> playLists)
        {
            if (_items != null &&_items.Length > 0)
                foreach (ListItem item in _items)
                    item.Dispose();

            _items = new ListItem[playLists.Keys.Count];

            for (int i = 0; i < playLists.Keys.Count; ++i)
            {
                _items[i] = new ListItem(20, _height * i, _panel.Width, _height, i, playLists.Keys.ElementAt(i), playLists.Values.ElementAt(i), new EventHandler(EnterEvent), new EventHandler(LeaveEvent), new MouseEventHandler(RightClickEvent), new EventHandler(DoubleClickEvent));
                _panel.Controls.Add(_items[i].Panel);
            }

            _panel.Height = _height * _itemsNo;

        }
        
        public Panel SetPanel
        {
            set
            {
                _panel = value;
            }
        }

        private void EnterEvent(object sender, EventArgs args)
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

            parent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
        }

        private void LeaveEvent(object sender, EventArgs args)
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

            parent.BackColor = System.Drawing.Color.Transparent;
        }

        private void RightClickEvent(object sender, MouseEventArgs args)
        {
            //if (args.Button == MouseButtons.Right)

        }

        private void DoubleClickEvent(object sender, EventArgs args)
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
                if (_mainForm.CheckView() == "playlistsList")
                {
                    _mainForm.DisplayPlayList(parent.Controls.Find("labelItemName" + parent.Name.Substring("panelItem".Length), false)[0].Text);
                }
                else
                if (_mainForm.CheckView() == "playlist" || _mainForm.CheckView() == "search")
                {
                    _mainForm.SetMedia(parent.Controls.Find("labelItemName" + parent.Name.Substring("panelItem".Length), false)[0].Text);
                    if (_mainForm.CheckView() == "playlist")
                        _mainForm.SetCurrentPlaylist = _mainForm.Controls.Find("panelPlaylist", false)[0].Controls.Find("labelPlaylistSongsTitle", false)[0].Text;
                    else
                        _mainForm.SetCurrentPlaylist = "";
                }
            }
        }
    }
}
