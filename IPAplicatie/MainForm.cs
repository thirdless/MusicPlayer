/**************************************************************************
 *                                                                        *
 *  File:        MainForm.cs                                              *
 *  Description: Implements a way for the user to interact with the GUI   *
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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace IPAplicatie
{
    public partial class MainForm : Form
    {
        private readonly Dictionary<string, Panel> _views;

        public readonly static string EqualizerPath = "equalizer.data";

        ViewManager _viewManager; // Instanta a obiectului ViewManager ce va avea rolul de generare a layout-urilor

        SQLiteManager.SQLManager _sqlManager; // Instanta a obiectului SQLManager ce se va ocupa cu comunicarea cu baza de date

        MediaPlayer.MusicPlayer _player; // Instanta a obiectului MusicPlayer ce se va ocupa cu redarea melodiei

        private int _currentSongDuration; // Variabila "_currentSongDuration" contribuie la afisarea duratei 

        string _selectedPlaylist = ""; // Va pastra playlist-ul care reda melodiile la un moment dat; Este necesar pentru reda melodii din playlist-ul curent in momentul in care apasam pe butoanele de inainte si inapoi

        public Thread currentOperation; // Pentru a pastra fluiditatea interfetei grafice vom lansa metodele blocante intr-un thread separat

        public int volumeValue;

        public MainForm()
        {
            InitializeComponent();

            // Se stocheaza panourile principale
            _views = new Dictionary<string, Panel>()
            {
                { "acasa", panelAcasa },                    // Afiseaza melodiile si panourile recente
                { "playlistsList", panelPlaylistsList },    // Afiseaza lista cu playlist-uri iar accesarea unui playlist va redirectiona utilizatorul pe panoul panelPlaylist
                { "search", panelSearch },                  // Afiseaza lista melodiilor care au in nume textul din panoul de cautare
                { "youtube", panelYoutube },                // Se ocupa cu optiunile procedura pentru downloadare a melodiilor de pe youtube
                { "equalizer", panelEqualizer },            // Ofera posibilitatea de a modifica valorile din equalizer
                { "playlist", panelPlaylist }               // Afiseaza melodiile dintr-un playlist in momentul in care acesta este selectat
            };

            _sqlManager = SQLiteManager.SQLManager.GetInstance();

            _player = new MediaPlayer.MusicPlayer();

            _viewManager = new ViewManager(this, panelPlaylistsListResult);

            _currentSongDuration = 0;

            timerSong.Start();

            volumeValue = trackVolume.Value;

            Thread.Sleep(500);
            ShowAcasa();
            CheckEqualizer();
        }

        public TrackBar GetDurationSlider
        {
            get
            {
                return trackMediaProgress;
            }
        }

        public string SetCurrentPlaylist
        {
            set
            {
                _selectedPlaylist = value;
            }
        }

        public void RefreshRecentPlaylists(string playlist)
        {
            _sqlManager.InsertToRecent(playlist);
        }

        // Afisarea melodiilor dintr-un playlist in interfata grafica
        public void DisplayPlayList(string playList)
        {
            SetView(panelPlaylist);
            _viewManager.SetPanel = panelPlaylistSongs;
            if (playList != "" && playList != "Toate melodiile")
            {
                labelPlaylistSongsTitle.Text = playList;
                _viewManager.CreatePlaylists(_sqlManager.GetSongsFromPlayList(playList));
            }
            else
            {
                labelPlaylistSongsTitle.Text = "Toate melodiile";
                _viewManager.CreatePlaylists(_sqlManager.GetSongs());
            }
        }

        // Modifica si asigura ca singurul panou vizibil este cel pasat ca argument
        private void SetView(Panel panel = null)
        {
            foreach (KeyValuePair<string, Panel> item in _views)
            {
                if (item.Value != panel)
                    item.Value.Visible = false;
                else
                    item.Value.Visible = true;
            }

            if (panel != null && panel != panelPlaylistsList)
                _viewManager.SetPanel = panel;
        }

        // Se foloseste pentru a verificarea panoului vizibil
        public string CheckView()
        {
            foreach (KeyValuePair<string, Panel> item in _views)
            {
                if (item.Value.Visible)
                    return item.Key;
            }

            return "";
        }

        // Panoul acasa are in componenta sa doua panouri care au rolul de a afisa separat melodiile si playlist-urile recente
        // Restul layout-urilor au doar o zona de afisare a listelor deci se face de fiecare data dispose la inceputul crearii elementelor
        // Pentru panoul acasa trebuie luat in calcul acest lucru asa ca vom apela metoda de dispose doar o singura data la inceputul metodei
        public void ShowAcasa()
        {
            _viewManager.CleanupItems();
            SetView(panelAcasa);
            _viewManager.SetPanel = panelAcasaMelodii;
            _viewManager.CreateRoutine(_sqlManager.GetRecentSongs());
            _viewManager.SetPanel = panelAcasaPlaylisturi;
            _viewManager.CreateRoutine(_sqlManager.GetRecentPlaylists());
        }

        private void buttonAcasa_Click(object sender, EventArgs e)
        {
            ShowAcasa();
        }

        private void buttonPlaylisturi_Click(object sender, EventArgs e)
        {
            SetView(panelPlaylistsList);
            _viewManager.SetPanel = panelPlaylistsListResult;
            _viewManager.CreatePlaylists(_sqlManager.GetPlaylists());
        }

        private void buttonCautare_Click(object sender, EventArgs e)
        {
            SetView(panelSearch);
        }

        // Interogare baza de date dupa wildcard-ul din panoul de cautare
        private void buttonSearch_Click(object sender, EventArgs e)
        {
            _viewManager.SetPanel = panelSearchResults;
            _viewManager.CreatePlaylists(_sqlManager.SearchSongsByName(textBoxSearch.Text));
        }

        private void buttonYoutube_Click(object sender, EventArgs e)
        {
            SetView(panelYoutube);
        }

        private void buttonEqualizer_Click(object sender, EventArgs e)
        {
            SetView(panelEqualizer);
        }

        private void buttonOthers_Click(object sender, EventArgs e)
        {
            contextMenuStripOthers.Show(new Point(MousePosition.X, MousePosition.Y));
        }

        // Aceasta metoda este apelata in evenimentul de click-dreapta elementele corespondente melodiilor din layout
        // Afiseaza optiunile din meniul pentru melodii la coordonatele mouse-ului
        public void ShowSongsContext()
        {
            SongContextMenuStrip.Show(new Point(MousePosition.X, MousePosition.Y));
        }

        // Aceasta metoda este apelata in evenimentul de click-dreapta elementele corespondente playlist-urilor din layout
        // Afiseaza optiunile din meniul pentru playlist-uri la coordonatele mouse-ului
        public void ShowPlaylistContext()
        {
            if (_viewManager.GetSelectedPlaylist != "Toate melodiile" && _viewManager.GetSelectedPlaylist != "Favorite" && _viewManager.GetSelectedPlaylist != "Melodii Recente")

                PlaylistContextMenuStrip.Show(new Point(MousePosition.X, MousePosition.Y));
        }

        // Aplica modificarile din slider-urile din panoul equalizer
        private void EqSlider_ValueChanged(object sender, EventArgs e)
        {
            _player.ChangeValue((TrackBar)sender);
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string text = "\n\n" +
            "Proiect IP 2021\n" +
            "Numele proiectului: SoundCore\n\n" +
            "Realizatori:\n\tMacovei Ioan\n\tRotaru Vlad\n\tManole Stefan\n\n" +
            "Descriere: Program Proiect IP\n\n";

            MessageBox.Show(text, "Despre aplicatie");
        }

        private void pictureMediaBack_Click(object sender, EventArgs e)
        {
            if (labelArtistName.Text.IndexOf(" Duration") > 0)
                SetMedia(_sqlManager.GetPrevSong(_selectedPlaylist, labelArtistName.Text.Substring(0, labelArtistName.Text.IndexOf(" Duration")) + " - " + labelSongName.Text));
            else
                SetMedia(_sqlManager.GetPrevSong(_selectedPlaylist, labelSongName.Text));
        }

        private void pictureMediaForth_Click(object sender, EventArgs e)
        {
            if (labelArtistName.Text.IndexOf(" Duration") > 0)
                SetMedia(_sqlManager.GetNextSong(_selectedPlaylist, labelArtistName.Text.Substring(0, labelArtistName.Text.IndexOf(" Duration")) + " - " + labelSongName.Text));
            else
                SetMedia(_sqlManager.GetNextSong(_selectedPlaylist, labelSongName.Text));
        }
        private void pictureMediaPlay_Click(object sender, EventArgs e)
        {
            if(_player.Play_Pause_Click() )
                pictureMediaPlay.Image = IPAplicatie.Properties.Resources.pause;
            else
                pictureMediaPlay.Image = IPAplicatie.Properties.Resources.play;
        }

        private string AddSongToDatabase(string link)
        {
            string songTitle = "";

            if (link != "")
            {
                songTitle = _player.GetName(link);

                if(songTitle.IndexOf("-") != -1)
                    songTitle = songTitle.Substring(0, songTitle.IndexOf("-")).Trim(' ') + " - " + songTitle.Substring(songTitle.IndexOf("-") + 1).Trim(' ');

                if (!_sqlManager.CheckMelodie(link))
                    _sqlManager.AddMelodie(link, songTitle, _player.GetDuration(link));
            }

            return songTitle;
        }

        // Aceasta metoda este apelata de fiecare data cand se doreste redarea unei melodii
        // Trebuie tratata perioada in care se deruleaza procedura de download
        // Daca aceasta tratare nu este efectuata exista riscul sa accesam un fisier inexistent
        // Se evita aceasta accesare ilegala prin setarea flagului isReady din '_player' pe valoarea false
        public void SetMedia(string title)
        {
            if (title != "")
            {
                _player.Ready = false;

                _sqlManager.UpdateRecentPlaylist(title);

                if (CheckView() == "playlist" && labelPlaylistSongsTitle.Text == "Cele mai recente")
                {
                    SetView(panelPlaylist);
                    _viewManager.SetPanel = panelPlaylistSongs;
                    _viewManager.CreatePlaylists(_sqlManager.GetRecentSongs());
                }

                labelSongName.Text = title.Substring(title.IndexOf("-") + 1).Trim(' ');
                labelArtistName.Text = _sqlManager.GetSongStats(title);

                _currentSongDuration = _sqlManager.GetSongDuration(title);

                pictureMediaPlay.Image = IPAplicatie.Properties.Resources.pause;

                if (currentOperation != null && currentOperation.IsAlive)
                {
                    //MessageBox.Show(currentOperation.IsAlive.ToString());
                    currentOperation.Abort();
                }       
                
                currentOperation = new Thread(() => _player.DownloadProcedure(_sqlManager.GetSongURL(title), volumeValue));
                currentOperation.Start();
                timerThumbnail.Start();
            }
        }
        
        // Se apeleaza in momentul in care se doreste adaugarea unei melodii de pe youtube in lista de melodii
        private void buttonYouTubeAdd_Click(object sender, EventArgs e)
        {
            if (currentOperation != null && currentOperation.IsAlive)
                currentOperation.Abort();

            string temp = textBoxYoutubeURL.Text;

            currentOperation = new Thread(() => AddSongToDatabase(temp));

            currentOperation.Start();

            textBoxYoutubeURL.Text = "";
        }

        // Se apeleaza in momentul in care se doreste redarea unei melodii cu un link de pe youtube
        private void buttonYouTubePlay_Click(object sender, EventArgs e)
        {
            string temp = textBoxYoutubeURL.Text;

            SetMedia(AddSongToDatabase(temp));

            textBoxYoutubeURL.Text = "";
        }

        // Aceasta metoda este apelata in momentul in care se doreste crearea unui nou playlist
        private void buttonPlaylistsListCreate_Click(object sender, EventArgs e)
        {
            //
            //  Panou de popup
            //
            Form popup = new Form();
            //
            //  Panou de introducere a numelui playlist-ului
            //
            TextBox nameBox = new TextBox();
            //
            //  Buton de acceptare
            //
            Button buttonOK = new Button();
            //
            //  Buton de anulare
            //
            Button buttonCancel = new Button();
            popup.Width = 350;
            popup.Height = 120;
            popup.FormBorderStyle = FormBorderStyle.FixedSingle;
            popup.Text = "Introdu numele playlistului";
            popup.StartPosition = FormStartPosition.CenterParent;
            popup.MaximizeBox = false;
            popup.MinimizeBox = false;
            nameBox.Width = 290;
            nameBox.Height = 60;
            nameBox.Top = 20;
            nameBox.Left = 30;
            buttonOK.Text = "OK";
            buttonOK.Left = 50;
            buttonOK.Top = 50;
            // Daca se apasa butonul OK se updateaza baza de date si se actualizeaza interfata
            buttonOK.Click += new EventHandler((object s, EventArgs ev) => {
                if (nameBox.Text != "")
                {
                    _sqlManager.AddPlaylist(nameBox.Text);
                    SetView(panelPlaylistsList);
                    _viewManager.CreatePlaylists(_sqlManager.GetPlaylists());
                }
                popup.DialogResult =  DialogResult.OK;
                popup.Close(); 
            });
            buttonCancel.Text = "Cancel";
            buttonCancel.Left = 220;
            buttonCancel.Top = 50;
            buttonCancel.Click += new EventHandler((object s, EventArgs ev) => { popup.DialogResult = DialogResult.Abort; popup.Close(); });
            popup.Controls.Add(nameBox);
            popup.Controls.Add(buttonOK);
            popup.Controls.Add(buttonCancel);
            DialogResult show = popup.ShowDialog(this);
            string result = nameBox.Text;

            if (show == DialogResult.OK && result != "")
            {
                //creare playlist
                MessageBox.Show("Playlist cu numele: " + result);
            }
        }

        // Configurarea slider-urilor cu valorile din fisierul equalizer.data
        private void CheckEqualizer()
        {
            try
            {
                if (File.Exists(@EqualizerPath))
                {
                    string settings = File.ReadAllText(@EqualizerPath);
                    string[] values = settings.Split(' ');

                    for (int i = 0; i < 10; i++)
                    {
                        int value = Int32.Parse(values[i]);

                        if (value < 0 || value > 40)
                            throw new Exception("error");

                        ((TrackBar)(panelEqualizer.Controls.Find("trackBarEq" + (i + 1), false)[0])).Value = value;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Eroare la parsarea datelor de equalizer");
                ClearEqualizer();
                SaveEqualizer();
                return;
            } 
        }

        // Metoda pentru resetarea valorilor din panoul equalizer
        private void ClearEqualizer()
        {
            for (int i = 0; i < 10; i++)
            {
                ((TrackBar)(panelEqualizer.Controls.Find("trackBarEq" + (i + 1), false)[0])).Value = 5;
            }
        }

        // Metoda pentru salvarea configurarilor din panoul pentru equalizer
        private void SaveEqualizer()
        {
            try
            {
                string text = "";

                for (int i = 0; i < 10; i++)
                {
                    text += ((TrackBar)(panelEqualizer.Controls.Find("trackBarEq" + (i + 1), false)[0])).Value.ToString() + " ";
                }

                File.WriteAllText(@EqualizerPath, text);
            }
            catch 
            {
                MessageBox.Show("Eroare la scrierea datelor de equalizer");
                return;
            }
        }

        private void buttonEqualizerSave_Click(object sender, EventArgs e)
        {
            SaveEqualizer();
        }
        
        protected override void OnClosing(CancelEventArgs e)
        {
            _player.Stop();
            base.OnClosing(e);
        }

        private void DurationSlider_MouseUp(object sender, MouseEventArgs e)
        {
            _player.SetDuration = GetDurationSlider.Value;
        }

        private void trackVolume_Scroll(object sender, EventArgs e)
        {
            volumeValue = ((TrackBar)sender).Value;
            _player.ChangeVolume = (float)volumeValue;
        }

        // Aceasta metoda monitorizeaza progresul melodiei si actualizeaza bara de durata
        // Aceasta actualizare a duratei se face doar in conditiile in care procedeul de download a luat sfarsit si redarea melodiei a inceput
        private void timerSong_Tick(object sender, EventArgs e)
        {
            trackMediaProgress.Value = _player.monitorPosition();
            if (_currentSongDuration > 0)
                if (_currentSongDuration % 60 > 9)
                    labelCurrentTime.Text = _player.monitorTime(_currentSongDuration, trackMediaProgress.Value) + _currentSongDuration / 60 + ":" + _currentSongDuration % 60;
                else
                    labelCurrentTime.Text = _player.monitorTime(_currentSongDuration, trackMediaProgress.Value) + _currentSongDuration / 60 + ":0" + _currentSongDuration % 60;
            else
                labelCurrentTime.Text = "";
        }

        // Implementare pentru stergerea playlist-urilor
        private void deletePlaylistStripMenuItem_Click(object sender, EventArgs e)
        {
            _sqlManager.DeletePlaylist(_viewManager.GetSelectedPlaylist);
            SetView(panelPlaylistsList);
            _viewManager.SetPanel = panelPlaylistsListResult;
            _viewManager.CreatePlaylists(_sqlManager.GetPlaylists());
        }

        // Implementare pentru stergerea melodiilor sau stergerea melodiilor din playlist-uri
        private void deleteSongStripMenuItem_Click(object sender, EventArgs e)
        {
            if (labelPlaylistSongsTitle.Text == "Toate melodiile")
                _sqlManager.DeleteSong(_viewManager.GetSelectedSong);
            else
                _sqlManager.DeleteSongFromPlaylist(labelPlaylistSongsTitle.Text, _viewManager.GetSelectedSong);

            SetView(panelPlaylist);
            _viewManager.SetPanel = panelPlaylistSongs;
            _viewManager.CreatePlaylists(_sqlManager.GetSongsFromPlayList(labelPlaylistSongsTitle.Text));
        }

        // Implementare pentru adaugarea melodiilor in playlist-uri
        private void addToPlaylistStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            // Fereastra de popup
            //
            Form popup = new Form();
            //
            // Lista cu playlisturi disponibile
            //
            ListBox nameBox = new ListBox();
            foreach (string line in _sqlManager.GetPlaylists().Keys)
            {
                if(line != "Toate melodiile" && line != "Favorite" && line != "Melodii Recente")
                    nameBox.Items.Add(line);
            }
            //
            // Buton de acceptare
            //
            Button buttonOK = new Button();
            //
            // Buton de anulare
            //
            Button buttonCancel = new Button();
            popup.Width = 350;
            popup.Height = 200;
            popup.FormBorderStyle = FormBorderStyle.FixedSingle;
            popup.Text = "Introdu playlist-ul destinatie";
            popup.StartPosition = FormStartPosition.CenterParent;
            popup.MaximizeBox = false;
            popup.MinimizeBox = false;
            nameBox.Width = 290;
            nameBox.Height = 100;
            nameBox.Top = 20;
            nameBox.Left = 30;
            buttonOK.Text = "OK";
            buttonOK.Left = 50;
            buttonOK.Top = 130;
            // Daca este apasat butonul de OK se va adauga melodia in playlistul selectat in elementul listBox
            buttonOK.Click += new EventHandler((object s, EventArgs ev) => {
                _sqlManager.AddToPlaylist((string)nameBox.SelectedItem, _viewManager.GetSelectedSong);
                popup.DialogResult = DialogResult.OK;
                popup.Close();
            });
            buttonCancel.Text = "Cancel";
            buttonCancel.Left = 220;
            buttonCancel.Top = 130;
            buttonCancel.Click += new EventHandler((object s, EventArgs ev) => { popup.DialogResult = DialogResult.Abort; popup.Close(); });
            popup.Controls.Add(nameBox);
            popup.Controls.Add(buttonOK);
            popup.Controls.Add(buttonCancel);
            DialogResult show = popup.ShowDialog(this);
            string result = nameBox.Text;

            if (show == DialogResult.OK && result != "")
            {
                //Adaugare melodie in playlist
                MessageBox.Show("Melodia " + _viewManager.GetSelectedSong + " a fost introdus in playlist-ul " + result);
            }
        }
        private void addToFavoriteStripMenuItem_Click(object sender, EventArgs e)
        {
            _sqlManager.AddToPlaylist("Favorite", _viewManager.GetSelectedSong);
        }

        private void buttonEqualizerClear_Click(object sender, EventArgs e)
        {
            ClearEqualizer();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }


        // Acest eveniment este necesar deoarece trebuie sa inlocuim continutul din calea "Samples/" 
        // Iar pe parcursul inlocuirii nu se poate accesa continutul
        // Asa ca interogam aceasta cale pentru a observa momentul in care continutul se actualizeaza
        // Ea este apaelata la momentul schimbarii melodiei
        private void timerThumbnail_Tick(object sender, EventArgs e)
        {
            string path = "Samples/audio.jpg";

            try
            {
                if (File.Exists(@path) && _player.Ready)
                {
                    byte[] content = File.ReadAllBytes(@path);
                    Image thumb = Image.FromStream(new MemoryStream(content));
                    if (pictureBoxSong.Image != null)
                        pictureBoxSong.Image.Dispose();
                    pictureBoxSong.Image = new Bitmap(thumb, new Size(thumb.Width * pictureBoxSong.Height / thumb.Height, pictureBoxSong.Height));;
                    timerThumbnail.Stop();
                }
                else
                    throw new Exception("not found");
            }
            catch
            {
                pictureBoxSong.Image = Properties.Resources.music;
                pictureBoxSong.SizeMode = PictureBoxSizeMode.CenterImage;
            }
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, "fisier help.chm");
        }
    }
}