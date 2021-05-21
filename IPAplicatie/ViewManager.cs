using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IPAplicatie
{
    class ViewManager
    {
        MainForm _mainForm;
        Panel _panel;
        LayoutItem[] _items;
        ItemFactory _factory;
        string _selectedPlaylist;
        string _selectedSong;

        public ViewManager(MainForm form, Panel panel)
        {
            _factory = new ItemFactory();
            _mainForm = form;
            _panel = panel;
            _selectedPlaylist = "";
            _selectedSong = "";
        }

        public void CleanupItems()
        {
            if (_items != null && _items.Length > 0)
                foreach (LayoutItem item in _items)
                {
                    item.Dispose();
                }
            _items = null;
        }


        public void CreateRoutine(Dictionary<string, string> playLists)
        {
            int height = 0,
                width = 0,
                type = 0;

            string check = _mainForm.CheckView();

            if (check == "playlistsList" || check == "acasa" || check == "")
            {
                height = 200;
                width = _panel.Width / GridItem.NumberItems;
                type = 1;
                _panel.Height = (height * (playLists.Keys.Count / GridItem.NumberItems + 1));
            }
            else if (check == "playlist" || check == "search")
            {
                height = 70;
                width = _panel.Width;
                _panel.Height = height * playLists.Keys.Count;
            }

            if (_items == null)
                _items = new LayoutItem[playLists.Keys.Count];
            else
            {
                LayoutItem[] temp = new LayoutItem[_items.Length + playLists.Keys.Count];

                for (int i = 0; i < _items.Length; ++i)
                    temp[i + playLists.Keys.Count] = _items[i];

                _items = temp;
            }

            for (int i = 0; i < playLists.Keys.Count; ++i)
            {
                _items[i] = _factory.GetItem(type, width, height, i, playLists.Keys.ElementAt(i), playLists.Values.ElementAt(i));
                _items[i].SetEvents(new EventHandler(EnterEvent), new EventHandler(LeaveEvent), new MouseEventHandler(RightClickEvent), new EventHandler(DoubleClickEvent));
                _panel.Controls.Add(_items[i].Panel);
            }
        }

        public void CreatePlaylists(Dictionary<string, string> playLists)
        {
            CleanupItems();
            CreateRoutine(playLists);
        }
        
        public Panel SetPanel
        {
            set
            {
                _panel = value;
            }
        }

        public string GetSelectedPlaylist
        {
            get
            {
                return _selectedPlaylist;
            }
        }
        public string GetSelectedSong
        {
            get
            {
                return _selectedSong;
            }
        }

        private void EnterEvent(object sender, EventArgs args)
        {
            Panel parent = null;

            try
            {
                parent = (Panel)sender;
            }
            catch
            {
                try
                {
                    parent = (Panel)((Label)sender).Parent;
                }
                catch
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
            catch
            {
                try
                {
                    parent = (Panel)((Label)sender).Parent;
                }
                catch
                {
                    parent = (Panel)((PictureBox)sender).Parent;
                }
            }

            parent.BackColor = System.Drawing.Color.Transparent;
        }

        private void RightClickEvent(object sender, MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Right)
            {
                Panel parent = null;

                try
                {
                    parent = (Panel)sender;
                }
                catch
                {
                    try
                    {
                        parent = (Panel)((Label)sender).Parent;
                    }
                    catch
                    {
                        parent = (Panel)((PictureBox)sender).Parent;
                    }
                }
                
                if(parent != null)
                {
                    if (_mainForm.CheckView() == "playlistsList")
                    {
                        _selectedPlaylist = parent.Controls.Find("labelItemName" + parent.Name.Substring("panelItem".Length), false)[0].Text;

                        _mainForm.ShowPlaylistContext();
                    }
                    else
                    if (_mainForm.CheckView() == "playlist" || _mainForm.CheckView() == "search")
                    {
                        _selectedSong = parent.Controls.Find("labelItemName" + parent.Name.Substring("panelItem".Length), false)[0].Text;
                        
                        _mainForm.ShowSongsContext();
                    }
                }
            }
        }

        private void DoubleClickEvent(object sender, EventArgs args)
        {

            Panel parent = null;

            try
            {
                parent = (Panel)sender;
            }
            catch
            {
                try
                {
                    parent = (Panel)((Label)sender).Parent;
                }
                catch
                {
                    parent = (Panel)((PictureBox)sender).Parent;
                }
            }

            if (parent != null)
            {
                string viewName = _mainForm.CheckView();

                if (viewName == "acasa" || viewName == "")
                {
                    string name = ((Panel)(parent.Parent)).Name;
                    if (name == "panelAcasaPlaylisturi")
                        _mainForm.DisplayPlayList(parent.Controls.Find("labelItemName" + parent.Name.Substring("panelItem".Length), false)[0].Text);
                    else if (name == "panelAcasaMelodii")
                    {
                        if (_mainForm.currentOperation != null && _mainForm.currentOperation.IsAlive)
                            _mainForm.currentOperation.Abort();

                        _mainForm.SetMedia(parent.Controls.Find("labelItemName" + parent.Name.Substring("panelItem".Length), false)[0].Text);
                        _mainForm.SetCurrentPlaylist = "Melodii Recente";
                        _mainForm.ShowAcasa();
                    }
                }
                else
                if (viewName == "playlistsList")
                {
                    _mainForm.DisplayPlayList(parent.Controls.Find("labelItemName" + parent.Name.Substring("panelItem".Length), false)[0].Text);
                }
                else if (viewName == "playlist" || viewName == "search")
                {
                    if (_mainForm.currentOperation != null && _mainForm.currentOperation.IsAlive)
                        _mainForm.currentOperation.Abort();

                    _mainForm.SetMedia(parent.Controls.Find("labelItemName" + parent.Name.Substring("panelItem".Length), false)[0].Text);

                    if (viewName == "playlist")
                    {
                        string currentPlaylist = _mainForm.Controls.Find("panelPlaylist", false)[0].Controls.Find("labelPlaylistSongsTitle", false)[0].Text;
                        
                        _mainForm.SetCurrentPlaylist = currentPlaylist;

                        if (currentPlaylist == "Melodii Recente")
                        {
                            _mainForm.DisplayPlayList(currentPlaylist);
                        }
                    }
                    else
                        _mainForm.SetCurrentPlaylist = "";
                }
            }
        }
    }
}
