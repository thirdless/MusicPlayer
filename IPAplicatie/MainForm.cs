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

namespace IPAplicatie
{
    public partial class MainForm : Form
    {
        private readonly Dictionary<string, Panel> _views;

        PlaylistsListView _playlistsListView;

        SQLManager _sqlManager;

        MusicPlayer _player;

        string _selectedPlaylist = "";

        public Thread currentOperation;

        public int volumeValue;

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

            _sqlManager = new SQLManager();

            _player = new MusicPlayer();

            _playlistsListView = new PlaylistsListView(this, panelPlaylistsListResult);

            timer1.Start();
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
            _playlistsListView.SetPanel = panelPlaylistSongs;
            if (playList != "" && playList != "Toate melodiile")
            {
                labelPlaylistSongsTitle.Text = playList;
                _playlistsListView.CreatePlaylists(_sqlManager.GetSongsFromPlayList(playList));
            }
            else
            {
                labelPlaylistSongsTitle.Text = "Toate melodiile";
                _playlistsListView.CreatePlaylists(_sqlManager.GetSongs());
            }
        }

        private void SetView(Panel panel = null)
        {
            foreach (KeyValuePair<string, Panel> item in _views)
            {
                item.Value.Visible = false;
            }

            if (panel != null)
            {
                if (panel != panelPlaylistsList)
                {
                    _playlistsListView.SetPanel = panel;
                }
                panel.Visible = true;
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

        private void buttonAcasa_Click(object sender, EventArgs e)
        {
            SetView(panelPlaylist);
            labelPlaylistSongsTitle.Text = "Cele mai recente";
            //_playlistsListView.CreatePlaylists();

        }

        private void buttonPlaylisturi_Click(object sender, EventArgs e)
        {
            SetView(panelPlaylistsList);
            _playlistsListView.SetPanel = panelPlaylistsListResult;
            _playlistsListView.CreatePlaylists(_sqlManager.GetPlaylists());
        }

        private void buttonCautare_Click(object sender, EventArgs e)
        {
            SetView(panelSearch);
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            _playlistsListView.SetPanel = panelSearchResults;
            _playlistsListView.CreatePlaylists(_sqlManager.SearchSongsByName(textBoxSearch.Text));
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
            "Realizatori:\n\tMacovei Ioan\n\tRotaru Vlad\n\tManole Stefan\n\tHretu Cristian\n\n" +
            "Descriere: Program Proiect IP\n\n";

            MessageBox.Show(text, "Despre aplicatie");
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void pictureMediaBack_Click(object sender, EventArgs e)
        {
            SetMedia(_sqlManager.GetPrevSong(_selectedPlaylist, labelSongName.Text));
        }

        private void pictureMediaForth_Click(object sender, EventArgs e)
        {
            SetMedia(_sqlManager.GetNextSong(_selectedPlaylist, labelSongName.Text));
        }
        private void pictureMediaPlay_Click(object sender, EventArgs e)
        {
            if(_player.Play_Pause_Click() )
                pictureMediaPlay.Image = IPAplicatie.Properties.Resources.pause;
            else
                pictureMediaPlay.Image = IPAplicatie.Properties.Resources.play;

            //if (DateTimeOffset.Now.ToUnixTimeSeconds() % 2 == 0)

        }

        private string AddSongToDatabase(string link)
        {
            string songTitle = "";

            if (link != "")
            {
                songTitle = _player.GetName(link);

                int duration = _player.GetDuration(link);

                if (!_sqlManager.CheckMelodie(link))
                    _sqlManager.AddMelodie(link, songTitle, duration);
            }

            return songTitle;
        }

        private void buttonYouTubeAdd_Click(object sender, EventArgs e)
        {
            AddSongToDatabase(textBoxYoutubeURL.Text);

            textBoxYoutubeURL.Text = "";
        }

        public void SetMedia(string title)
        {
            if (title != "")
            {
                labelSongName.Text = title.Substring(title.IndexOf("-") + 1).Trim(' ');
                labelArtistName.Text = _sqlManager.GetSongStats(title);

                pictureMediaPlay.Image = IPAplicatie.Properties.Resources.pause;

                if (currentOperation != null && currentOperation.IsAlive)
                        currentOperation.Interrupt();
                
                currentOperation = new Thread(() => _player.DownloadProcedure(_sqlManager.GetSongURL(title), volumeValue));
                currentOperation.Start();
            }
        }

        private void buttonYouTubePlay_Click(object sender, EventArgs e)
        {
            SetMedia(AddSongToDatabase(textBoxYoutubeURL.Text));

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
                    _playlistsListView.CreatePlaylists(_sqlManager.GetPlaylists());
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

        private void buttonEqualizerSave_Click(object sender, EventArgs e)
        {

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
            //volumeValue = ((TrackBar)sender).Value;
            _player.ChangeVolume = (float)((TrackBar)sender).Value / 100.0f;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            trackMediaProgress.Value = _player.monitorPosition();
        }
    }
}