using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.Sql;
using System.Data.SQLite;
using System.Windows.Forms;

namespace IPAplicatie
{
    class SQLManager
    {
        private SQLiteConnection _sqlConnection;
        public SQLManager()
        {
            if (!File.Exists("ProjectDatabase.db"))
                _sqlConnection = new SQLiteConnection("Data Source=ProjectDatabase.db;Version=3;New=True;Compress=True;");
            else
                _sqlConnection = new SQLiteConnection("Data Source=ProjectDatabase.db;Version=3;Compess=True;");

            try
            {
                _sqlConnection.Open();
            }
            catch
            {
                MessageBox.Show("Unable to open database");
            }

            CreateTables();
        }

        public void DisposeDatabase()
        {
            _sqlConnection.Close();
        }
        public void CreateTables()
        {
            SQLiteCommand cmd;

            cmd = _sqlConnection.CreateCommand();

            cmd.CommandText = "CREATE TABLE if NOT EXISTS Melodie (" +
                "melodieID INTEGER PRIMARY KEY AUTOINCREMENT," +
                "Sursa_Video TEXT NOT NULL," +
                "Nume_Melodie TEXT NOT NULL," +
                "Duration INTEGER NOT NULL," +
                "UNIQUE(Sursa_Video)" +
                ")";

            cmd.ExecuteNonQuery();

            cmd.CommandText = "CREATE TABLE if NOT EXISTS Playlist_uri (" +
                "playlistID INTEGER PRIMARY KEY AUTOINCREMENT," +
                "Nume_Playlist TEXT NOT NULL" +
                ")";

            cmd.ExecuteNonQuery();

            cmd.CommandText = "CREATE TABLE if NOT EXISTS Playlist (" +
                "playlistID INTEGER NOT NULL," +
                "melodieID INTEGER NOT NULL," +
                "FOREIGN KEY(melodieID) REFERENCES Melodie(melodieID), " +
                "FOREIGN KEY(playlistID) REFERENCES Playlist_uri(playlistID)" +
                ")";

            cmd.ExecuteNonQuery();
        }

        public void AddMelodie(string url, string name, int duration)
        {
            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            try
            {
                cmd.CommandText = "INSERT INTO Melodie " +
                    "(Sursa_Video, Nume_Melodie, Duration) VALUES" +
                    "(\'" + url + "\', \'" + name + "\' , " + duration + ")";

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Operatia de adaugare a melodiei nu a putut fi efectuata");
            }
        }

        public bool CheckMelodie(string url)
        {
            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            cmd.CommandText = "SELECT count(melodieID) FROM Melodie WHERE Sursa_Video = " + url;

            int rez = Convert.ToInt32(cmd.ExecuteScalar());

            return rez > 0;
        }

        public bool CheckPlaylist(string name)
        {
            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            cmd.CommandText = "SELECT count(playlistID) FROM Playlist_uri WHERE Nume_Playlist=\'" + name + "\'";

            int rez = Convert.ToInt32(cmd.ExecuteScalar());

            return rez > 0;
        }

        public void AddToPlaylist(int playListID, int melodieID)
        {
            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            try
            {
                cmd.CommandText = "INSERT INTO Playlist " +
                    "(playlistID, melodieID) VALUES " +
                    "(" + playListID + ", " + melodieID + ")";

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Operatia de adaugare in playlist a esuat");
            }
        }

        public void AddToPlaylist(string playlistName, string melodie)
        {
            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            cmd.CommandText = "SELECT playlistID FROM Playlist_uri WHERE Nume_Playlist=\'" + playlistName + "\'";

            int listID = -1, songID = -1;

            SQLiteDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
                listID = reader.GetInt32(0);

            reader.Close();

            cmd.CommandText = "SELECT melodieID FROM Melodie WHERE Nume_Melodie=\'" + melodie + "\'";

            reader = cmd.ExecuteReader();

            if (reader.Read())
                songID = reader.GetInt32(0);

            if (listID != -1 && songID != -1)
                AddToPlaylist(listID, songID);
        }

        public string GetSongURL(string songName)
        {
            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            SQLiteDataReader reader;

            cmd.CommandText = "SELECT Sursa_Video FROM Melodie WHERE Nume_Melodie=\'" + songName + "\'";

            reader = cmd.ExecuteReader();

            string rez = reader.GetString(0);

            return rez;
        }

        public Dictionary<string, string> SearchSongsByName(string wildcard)
        {
            Dictionary<string, string> rez = new Dictionary<string, string>();

            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            SQLiteDataReader reader;

            cmd.CommandText = "SELECT Nume_Melodie FROM Melodie WHERE Nume_Melodie LIKE \'%" + wildcard + "%\'";

            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                string temp = reader.GetString(0);

                rez[temp] = GetSongStats(temp);
            }

            return rez;
        }

        public List<string> GetListSongs(int listID)
        {
            List<string> songs = new List<string>();

            SQLiteDataReader reader;

            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            cmd.CommandText = "SELECT melodieID FROM Playlist WHERE playlistID=" + listID;

            reader = cmd.ExecuteReader();

            List<int> aux = new List<int>();

            while (reader.Read())
            {
                aux.Add(reader.GetInt32(0));
            }

            reader.Close();

            foreach (int elem in aux)
            {
                cmd.CommandText = "SELECT Nume_Melodie FROM Melodie WHERE melodieID=" + elem;
                reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    songs.Add(reader.GetString(0));
                }
                reader.Close();
            }

            return songs;
        }

        public List<string> GetListSongs(string listName)
        {
            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            cmd.CommandText = "SELECT playlistID FROM Playlist_uri WHERE Nume_Playlist=\'" + listName + "\'";

            SQLiteDataReader reader = cmd.ExecuteReader();

            int id = -1;

            if (reader.Read())
                id = reader.GetInt32(0);

            if (id > -1)
                return GetListSongs(id);
            return null;
        }

        public Dictionary<string, string> GetSongsFromPlayList(string listName)
        {
            Dictionary<string, string> rez = new Dictionary<string, string>();

            foreach (string song in (List<string>)GetListSongs(listName))
            {
                rez[song] = GetSongStats(song);
            }

            return rez;
        }
    
        public Dictionary<string, string> GetPlaylists()
        {
            Dictionary<string, string> rez = new Dictionary<string, string>();

            List<string> playLists = new List<string>();

            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            cmd.CommandText = "SELECT Nume_Playlist FROM Playlist_uri";

            SQLiteDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                playLists.Add(reader.GetString(0));
            }

            reader.Close();

            foreach (string playList in playLists)
            {
                rez[playList] = GetPlayListStats(playList);
            }

            return rez;
        }

        public string GetPlayListStats(string playList)
        {
            int duration = 0;

            int length = 0;

            List<string> songs = GetListSongs(playList);

            SQLiteCommand cmd = _sqlConnection.CreateCommand();
            
            SQLiteDataReader reader;

            foreach (string song in songs)
            {
                cmd.CommandText = "SELECT Duration FROM Melodie WHERE Nume_Melodie=\'" + song + "\'";
                
                reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    duration += reader.GetInt32(0);

                    ++length;
                }

                reader.Close();
            }

            string rez;

            if (duration % 60 > 9)
                rez = "Duration: " + (duration / 60) + ":" + (duration % 60) + " * " + "Number of songs: " + length;
            else
                rez = "Duration: " + (duration / 60) + ":0" + (duration % 60) + " * " + "Number of songs: " + length;

            return rez;
        }

        public Dictionary<string, string> GetSongs()
        {
            Dictionary<string, string> rez = new Dictionary<string, string>();

            List<string> songList = new List<string>();

            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            cmd.CommandText = "SELECT Nume_Melodie, Sursa_Video FROM Melodie";

            SQLiteDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                songList.Add(reader.GetString(0) + " " + reader.GetString(1));
            }

            foreach (string song in songList)
            {
                rez[song] = GetSongStats(song);
            }

            return rez;
        }

        public string GetSongStats(string song)
        {
            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            cmd.CommandText = "SELECT Duration FROM Melodie WHERE Nume_Melodie=\'" + song + "\'";

            SQLiteDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
                return "Duration: " + reader.GetInt32(0);
            
            return "";
        }

        public void AddPlaylist(string newList)
        {
            if (!CheckPlaylist(newList))
            {
                SQLiteCommand cmd = _sqlConnection.CreateCommand();

                try
                {
                    cmd.CommandText = "INSERT INTO Playlist_uri " +
                          "(Nume_Playlist) VALUES " +
                          "(\'" + newList + "\')";

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Operatia de adaugare a noului playlist a esuat");
                }
            }
        }
    }
}
