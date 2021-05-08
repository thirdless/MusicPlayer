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
        public MainForm()
        { 
            InitializeComponent();
            
        }

        private void buttonAcasa_Click(object sender, EventArgs e)
        {
            panelMedia.Visible = false;
        }

        private void buttonPlaylisturi_Click(object sender, EventArgs e)
        {
            
        }

        private void buttonCautare_Click(object sender, EventArgs e)
        {
            
        }

        private void buttonYoutube_Click(object sender, EventArgs e)
        {

        }

        private void buttonOthers_Click(object sender, EventArgs e)
        {
            contextMenuStrip1.Show(new Point(MousePosition.X, MousePosition.Y));
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

        }

        private void pictureMediaPlay_Click(object sender, EventArgs e)
        {
            if (DateTimeOffset.Now.ToUnixTimeSeconds() % 2 == 0)
                pictureMediaPlay.Image = IPAplicatie.Properties.Resources.play;
            else
                pictureMediaPlay.Image = IPAplicatie.Properties.Resources.pause;
        }
    }
}
