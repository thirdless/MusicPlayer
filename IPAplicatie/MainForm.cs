using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace IPAplicatie
{
    public partial class MainForm : Form
    {
        private readonly Dictionary<string, Panel> _views;

        ViewManager _viewManager;

        SQLManager _sqlManager;

        MusicPlayer _player;

        private int _currentSongDuration;

        string _selectedPlaylist = "";

        public Thread currentOperation;

        public int volumeValue;

        public readonly static string EqualizerPath = "equalizer.data";

        public MainForm()
        {
            InitializeComponent();

            _views = new Dictionary<string, Panel>()
            {
                { "acasa", panelAcasa },
                { "playlistsList", panelPlaylistsList },
                { "search", panelSearch },
                { "youtube", panelYoutube },
                { "equalizer", panelEqualizer },
                { "playlist", panelPlaylist }
            };

            _sqlManager = SQLManager.GetInstance();

            _player = new MusicPlayer();

            _viewManager = new ViewManager(this, panelPlaylistsListResult);

            _currentSongDuration = 0;

            timerSong.Start();

            volumeValue = trackVolume.Value;

            // afisare acasa
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

        public void DisplayPlayList(string playList)
        {
            SetView(panelPlaylist);
            _viewManager.SetPanel = panelPlaylistSongs;
            _sqlManager.InsertToRecent(playList);
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
            {
                _viewManager.SetPanel = panel;
            }
        }

        public string CheckView()
        {
            foreach (KeyValuePair<string, Panel> item in _views)
            {
                if (item.Value.Visible)
                    return item.Key;
            }

            return "";
        }

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

        public void ShowSongsContext()
        {
            SongContextMenuStrip.Show(new Point(MousePosition.X, MousePosition.Y));
        }
        public void ShowPlaylistContext()
        {
            if (_viewManager.GetSelectedPlaylist != "Toate melodiile" && _viewManager.GetSelectedPlaylist != "Favorite" && _viewManager.GetSelectedPlaylist != "Melodii Recente")

                PlaylistContextMenuStrip.Show(new Point(MousePosition.X, MousePosition.Y));
        }

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

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {

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

        private void buttonYouTubeAdd_Click(object sender, EventArgs e)
        {
            if (currentOperation != null && currentOperation.IsAlive)
                currentOperation.Abort();

            string temp = textBoxYoutubeURL.Text;

            currentOperation = new Thread(() => AddSongToDatabase(temp));

            currentOperation.Start();

            textBoxYoutubeURL.Text = "";
        }

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

        private void buttonYouTubePlay_Click(object sender, EventArgs e)
        {
            string temp = textBoxYoutubeURL.Text;

            SetMedia(AddSongToDatabase(temp));

            textBoxYoutubeURL.Text = "";
        }

        private void buttonPlaylistsListCreate_Click(object sender, EventArgs e)
        {
            Form popup = new Form();
            TextBox nameBox = new TextBox();
            Button buttonOK = new Button();
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

        private void ClearEqualizer()
        {
            for (int i = 0; i < 10; i++)
            {
                ((TrackBar)(panelEqualizer.Controls.Find("trackBarEq" + (i + 1), false)[0])).Value = 5;
            }
        }

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

        private void deletePlaylistStripMenuItem_Click(object sender, EventArgs e)
        {
            _sqlManager.DeletePlaylist(_viewManager.GetSelectedPlaylist);
            SetView(panelPlaylistsList);
            _viewManager.SetPanel = panelPlaylistsListResult;
            _viewManager.CreatePlaylists(_sqlManager.GetPlaylists());
        }
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
        private void addToPlaylistStripMenuItem_Click(object sender, EventArgs e)
        {
            Form popup = new Form();
            ListBox nameBox = new ListBox();
            foreach (string line in _sqlManager.GetPlaylists().Keys)
            {
                if(line != "Toate melodiile" && line != "Favorite" && line != "Melodii Recente")
                    nameBox.Items.Add(line);
            }
            Button buttonOK = new Button();
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
    }
}