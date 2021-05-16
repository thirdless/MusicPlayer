using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IPAplicatie
{
    public partial class MainForm : Form
    {

        private readonly Dictionary<string, Panel> _views;

        PlaylistsListView _playlistsListView;

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

            _playlistsListView = new PlaylistsListView(this, panelPlaylistsListResult);
        }

        private void SetView(Panel panel = null)
        {
            foreach (KeyValuePair<string, Panel> item in _views)
            {
                item.Value.Visible = false;
            }

            if (panel != null)
                panel.Visible = true;
        }

        private void SetSongName(string name)
        {
            labelSongName.Text = name;
        }

        private void SetArtistName(string name)
        {
            labelArtistName.Text = name;
        }

        private void SetPicture(Image picture)
        {
            pictureBoxSong.Image = picture;
        }

        private void buttonAcasa_Click(object sender, EventArgs e)
        {
            SetView(panelPlaylist);
        }

        private void buttonPlaylisturi_Click(object sender, EventArgs e)
        {
            SetView(panelPlaylistsList);
            _playlistsListView.CreatePlaylists();
        }

        private void buttonCautare_Click(object sender, EventArgs e)
        {
            SetView(panelSearch);
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
            panelMedia.Controls.Find("labelSongNam*", false)[0].Text = "ceva";
        }

        private void pictureMediaPlay_Click(object sender, EventArgs e)
        {
            if (DateTimeOffset.Now.ToUnixTimeSeconds() % 2 == 0)
                pictureMediaPlay.Image = IPAplicatie.Properties.Resources.play;
            else
                pictureMediaPlay.Image = IPAplicatie.Properties.Resources.pause;
        }

        private void buttonPlaylistsListCreate_Click(object sender, EventArgs e)
        {
            Form popup = new Form();
            TextBox nameBox = new TextBox();
            Button buttonOK = new Button();
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
            buttonOK.Left = 130;
            buttonOK.Top = 50;
            buttonOK.Click += new EventHandler((object s, EventArgs ev) => { popup.DialogResult = DialogResult.OK; popup.Close(); });
            popup.Controls.Add(nameBox);
            popup.Controls.Add(buttonOK);
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
    }
}
