using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SQLiteManager;
using IPAplicatie;
using System.Windows.Forms;
using MediaPlayer;
using Layout;
using System.Drawing;

namespace AplicatieTestUnit
{
    [TestClass]
    public class UnitTestClass
    {
        private static SQLManager _sqlManager;
        private static MainForm _mainForm;
        private static MusicPlayer _musicPlayer;

        public static readonly string YoutubeLink = "https://www.youtube.com/watch?v=EqkCBpeLT1Q";

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _sqlManager = SQLManager.GetInstance();
            _mainForm = new MainForm();
            _musicPlayer = new MusicPlayer();
        }

        [TestMethod]
        public void SQLManager_TestToateMelodiile()
        {
            Assert.AreEqual(true, _sqlManager.CheckPlaylist("Toate melodiile"));
        }

        [TestMethod]
        public void SQLManager_TestMelodiiNumeAlteator()
        {
            Assert.AreEqual(false, _sqlManager.CheckPlaylist("NumeAleatorPeCareUnUtilizatorNuOSaIlAleagaNiciodata"));
        }

        [TestMethod]
        public void SQLManager_TestMelodiiRecente()
        {
            Assert.AreEqual(true, _sqlManager.CheckPlaylist("Melodii Recente"));
        }

        [TestMethod]
        public void MainForm_TestSetView_Existent()
        {
            Assert.AreEqual(true, _mainForm.SetView(_mainForm.Views["acasa"]));
        }

        [TestMethod]
        public void MainForm_TestSetView_Inexistent()
        {
            Assert.AreEqual(false, _mainForm.SetView(new Panel()));
        }

        [TestMethod]
        public void MainForm_TestCheckEqualizer_Existent()
        {
            Assert.AreEqual(true, _mainForm.CheckEqualizer(MainForm.EqualizerPath));
        }

        [TestMethod]
        public void MainForm_TestCheckEqualizer_Inexistent()
        {
            Assert.AreEqual(false, _mainForm.CheckEqualizer("nodata.data"));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void MainForm_TestProcessEqualizer_Exception()
        {
            _mainForm.ProcessEqualizer("0 -10 0 0 0 0 0 0 0 0");
        }

        [TestMethod]
        public void MainForm_TestProcessEqualizer_Correct()
        {
            Assert.AreEqual(true, _mainForm.ProcessEqualizer("5 5 5 5 5 5 5 5 5 5"));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void MainForm_TestAddSongToDatabase_Exception()
        {
            _mainForm.AddSongToDatabase("http://google.ro");
        }

        [TestMethod]
        public void MainForm_TestAddSongToDatabase_SpongeBobVideo()
        {
            Assert.AreEqual("SpongeBob Production Music Twelfth Street Rag", _mainForm.AddSongToDatabase(YoutubeLink));
        }

        [TestMethod]
        [ExpectedException(typeof(UriFormatException))]
        public void MusicPlayer_TestDownloadThumbnail_UriException()
        {
            _musicPlayer.DownloadThumbnail("=", "testurl");
        }

        [TestMethod]
        public void MusicPlayer_TestDownloadThumbnail_DownloadStarts()
        {
            Assert.AreEqual(true, _musicPlayer.DownloadThumbnail("=ceva", "http://smuwn.com"));
        }

        [TestMethod]
        public void MusicPlayer_TestDownloadThumbnail_DownloadDoesntStart()
        {
            Assert.AreEqual(false, _musicPlayer.DownloadThumbnail("", "http://smuwn.com"));
        }

        [TestMethod]
        public void MusicPlayer_TestParseLink_PathWithList()
        {
            string linkName = "&list=cevaanume";
            Assert.AreEqual(-1, _musicPlayer.ParseLink(YoutubeLink + linkName).IndexOf(linkName));
        }

        [TestMethod]
        public void MusicPlayer_TestParseLink_PathWithoutList()
        {
            Assert.AreEqual(YoutubeLink, _musicPlayer.ParseLink(YoutubeLink));
        }

        [TestMethod]
        public void MusicPlayer_TestGetDuration_2_07_Video()
        {
            Assert.AreEqual(127, _musicPlayer.GetDuration("https://www.youtube.com/watch?v=EqkCBpeLT1Q"));
        }

        [TestMethod]
        public void MusicPlayer_TestGetDuration_InvalidLink()
        {
            Assert.AreEqual(0, _musicPlayer.GetDuration("https://www.youtube.com/"));
        }

        [TestMethod]
        public void LayoutItem_TestGetSizeList_1_Px()
        {
            int height = 1;
            LayoutItem layout = new ListItem(10, height, 1, "Name", "Secondary", null);
            Assert.AreEqual((int)(0.8 * height), layout.GetPictureSize().Height);
        }

        [TestMethod]
        public void LayoutItem_TestGetSizeGrid_2Billion_Px()
        {
            int width = 2000000000;
            LayoutItem layout = new Layout.GridItem(width, 10, 1, "Name", "Secondary", null);
            Assert.AreEqual(0.7 * width, layout.GetPictureSize().Width);
        }
    }
}
