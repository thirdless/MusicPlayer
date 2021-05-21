using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace IPAplicatie
{
    class ViewManager
    {
        MainForm _mainForm; // Referinta la obiectul MainForm de care ne vom folosi pentru a accesa metodele publice necesare afisarii in panou
        Panel _panel; // Panoul in care se vor introduce elementele la momentul generarii
        LayoutItem[] _items; // Vectorul in care vom stoca elementele din layout
        ItemFactory _factory; // Instanta pentru obiectul de tip fabrica ce va avea rolul de a genera elementele din vectorul "_items"
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

        // Sterge elemntele grafice din vector
        public void CleanupItems()
        {
            if (_items != null && _items.Length > 0)
                foreach (LayoutItem item in _items)
                {
                    item.Dispose();
                }
            _items = null;
        }

        // Metoda ce genereaza si afiseaza in panou elementele grafice in functie de layout
        public void CreateRoutine(Dictionary<string, string> playLists)
        {
            int height = 0,
                width = 0,
                type = 0;

            string check = _mainForm.CheckView(); // Se stocheaza numele panoului vizibil din prezent

            // Se determina ce tip de layout se va folosi
            // In cazul in care ne dorim sa afisam panoul acasa sau panoul playlist vom afisa elementele intr-un layout de tip tabel
            // In cazul in care ne dorim sa afisam melodiile dintr-un playlist vom afisa elemntele intr-un layout de tip lista
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

        // In majoritatea cazurilor de afisare a elemtelor grafica dintr-o lista se apeleaza mai intai procedura de eliminare a elementelor pentru inlocuirea acestora cu noile elemente
        // Aceasta metoda a fost creata pentru simplificarea apelulilor
        public void CreatePlaylists(Dictionary<string, string> playLists)
        {
            CleanupItems();
            CreateRoutine(playLists);
        }
        
        // Este necesara modificarea panoului in care se vor afisa elemntele in unele din evenimentele din MainForm
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

        // Este necesar pentru a scoate in evidenta elementul pe care se afla mouse-ul
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

        // Este necesar pentru anularea evidentierii unui element care a primit inainte de acesta evenimentul de Enter
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

        // Acest eveniment va afisa o lista de optiuni in momentul in care se apasa click-dreapta
        // Pentru coordonatele mouse-ului se va apela o metoda public din referinta catre MainForm
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

        // Va accesa un element din panou
        // Daca acesta este de tip playlist atunci se va afisa continutul playlist-ului si se va actualiza baza de date
        // Iar daca elemntul ales este de tip melodie atunci se va reda melodia si se va actualiza baza de date si referinta catre MainForm
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

                if (viewName == "acasa" || viewName == "") // Se trateaza cazul in care elementele de acces se afla pe panoul acasa; deoarece pe acest panou se afla si elemnte de tip melodie si elemnte de tip playlist
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
                if (viewName == "playlistsList") // Se trateaza cazul in care elementele de acces se afla pe un panou de playlist-uri
                {
                    _mainForm.DisplayPlayList(parent.Controls.Find("labelItemName" + parent.Name.Substring("panelItem".Length), false)[0].Text);
                }
                else if (viewName == "playlist" || viewName == "search") // Se trateaza cazul in care elementele de acces se afla intr-o list de melodii
                {
                    if (_mainForm.currentOperation != null && _mainForm.currentOperation.IsAlive)
                        _mainForm.currentOperation.Abort();

                    _mainForm.SetMedia(parent.Controls.Find("labelItemName" + parent.Name.Substring("panelItem".Length), false)[0].Text);

                    if (viewName == "playlist")
                    {
                        string currentPlaylist = _mainForm.Controls.Find("panelPlaylist", false)[0].Controls.Find("labelPlaylistSongsTitle", false)[0].Text;

                        _mainForm.RefreshRecentPlaylists(currentPlaylist);

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
