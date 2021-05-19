﻿using System;
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

            AddPlaylist("Toate melodiile");

            AddPlaylist("Favorite");

            AddPlaylist("Melodii Recente");
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
                "Nume_Playlist TEXT NOT NULL," +
                "UNIQUE(Nume_Playlist)"+
                ")";

            cmd.ExecuteNonQuery();

            cmd.CommandText = "CREATE TABLE if NOT EXISTS Playlist (" +
                "playlistID INTEGER NOT NULL," +
                "melodieID INTEGER NOT NULL," +
                "FOREIGN KEY(melodieID) REFERENCES Melodie(melodieID), " +
                "FOREIGN KEY(playlistID) REFERENCES Playlist_uri(playlistID)" +
                ")";

            cmd.ExecuteNonQuery();

            cmd.CommandText = "CREATE TABLE if NOT EXISTS PlaylistRecente (" +
                "id INTEGER PRIMARY KEY AUTOINCREMENT," + 
                "playlistID INTEGER NOT NULL," +
                "FOREIGN KEY(playlistID) REFERENCES Playlist_uri(playlistID)" +
                ")";

            cmd.ExecuteNonQuery();
        }

        public void DeleteSongFromPlaylist(string playlistName, string songName)
        {
            int songID = -1;

            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            SQLiteDataReader reader;

            cmd.CommandText = "SELECT melodieID FROM Melodie WHERE Nume_Melodie=\'" + songName + "\'";

            reader = cmd.ExecuteReader();

            if (reader.Read())
                songID = reader.GetInt32(0);

            DeleteSongFromPlaylist(playlistName, songID);
        }

        public void DeleteSongFromPlaylist(string playlistName, int songID)
        {
            int playlistID = -1;

            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            SQLiteDataReader reader;

            cmd.CommandText = "SELECT playlistID FROM Playlist_uri WHERE Nume_Playlist=\'" + playlistName + "\'";

            reader = cmd.ExecuteReader();

            if (reader.Read())
                playlistID = reader.GetInt32(0);

            reader.Close();

            if (songID != -1 && playlistID != -1)
            {
                cmd.CommandText = "DELETE FROM Playlist WHERE playlistID=" + playlistID + " AND " + "melodieID=" + songID;

                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteSong(string songName)
        {
            int songID = -1;

            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            SQLiteDataReader reader;

            cmd.CommandText = "SELECT melodieID FROM Melodie WHERE Nume_Melodie=\'" + songName + "\'";

            reader = cmd.ExecuteReader();

            if (reader.Read())
                songID = reader.GetInt32(0);

            reader.Close();

            DeleteSong(songID);
        }

        public void DeleteSong(int songID)
        {
            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            cmd.CommandText = "DELETE FROM Melodie WHERE melodieID=" + songID;

            cmd.ExecuteNonQuery();

            cmd.CommandText = "DELETE FROM Playlist WHERE melodieID=" + songID;

            cmd.ExecuteNonQuery();
        }

        public void DeletePlaylist(string playlistName)
        {
            int playlistID = -1;

            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            SQLiteDataReader reader;

            cmd.CommandText = "SELECT playlistID FROM Playlist_uri WHERE Nume_Playlist=\'" + playlistName + "\'";

            reader = cmd.ExecuteReader();

            if (reader.Read())
                playlistID = reader.GetInt32(0);

            reader.Close();

            DeletePlaylist(playlistID);
        }

        public void DeletePlaylist(int playlistID)
        {
            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            cmd.CommandText = "DELETE FROM Playlist_uri WHERE playlistID=" + playlistID;

            cmd.ExecuteNonQuery();

            cmd.CommandText = "DELETE FROM Playlist WHERE playlistID=" + playlistID;

            cmd.ExecuteNonQuery();
        }

        public int GetSongDuration(string songName)
        {
            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            SQLiteDataReader reader;

            cmd.CommandText = "SELECT Duration FROM Melodie WHERE Nume_Melodie=\'" + songName + "\'";

            reader = cmd.ExecuteReader();

            int duration = 0;

            if (reader.Read())
                duration = reader.GetInt32(0);

            return duration;
        }
        
        public void UpdateRecentPlaylist(string song)
        {
            int songID = -1, playlistID = -1;

            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            SQLiteDataReader reader;

            cmd.CommandText = "SELECT melodieID FROM Melodie WHERE Nume_Melodie=\'" + song + "\'";

            reader = cmd.ExecuteReader();

            if (reader.Read())
                songID = reader.GetInt32(0);

            reader.Close();

            cmd.CommandText = "SELECT playlistID FROM Playlist_uri WHERE Nume_Playlist=\'Melodii Recente\'";

            reader = cmd.ExecuteReader();

            if (reader.Read())
                playlistID = reader.GetInt32(0);

            reader.Close();

            List<int> songList = new List<int>();

            cmd.CommandText = "SELECT melodieID FROM Playlist WHERE playlistID=" + playlistID;

            reader = cmd.ExecuteReader();

            while (reader.Read())
                songList.Add(reader.GetInt32(0));

            reader.Close();

            if (songList.Count > 0)
            {
                if (songList.Contains(songID))
                {
                    DeleteSongFromPlaylist("Melodii Recente", songID);

                    AddToPlaylist(playlistID, songID);
                }
                else
                {
                    while (songList.Count >= 5)
                    {
                        DeleteSongFromPlaylist("Melodii Recente", songList.ElementAt(0));

                        songList.RemoveAt(0);
                    }

                    AddToPlaylist(playlistID, songID);
                }
            }
            else
                AddToPlaylist(playlistID, songID);
            
        }

        public Dictionary<string, string> GetRecentSongs()
        {
            Dictionary<string, string> rez = new Dictionary<string, string>();

            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            SQLiteDataReader reader;

            List<string> songs = GetListSongs("Melodii Recente");

            for (int i = songs.Count - 1; i >= 0; --i)
                rez[songs.ElementAt(i)] = GetSongStats(songs.ElementAt(i));

            return rez;
        }

        public void AddMelodie(string url, string name, int duration)
        {
            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            try
            {
                cmd.CommandText = "INSERT INTO Melodie " +
                    "(Sursa_Video, Nume_Melodie, Duration) VALUES " +
                    "(\'" + url + "\', \'" + name + "\', " + duration + ")";

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

            cmd.CommandText = "SELECT melodieID FROM Melodie WHERE Sursa_Video=\'" + url + "\'";

            return cmd.ExecuteReader().Read();
        }

        public bool CheckPlaylist(string name)
        {
            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            cmd.CommandText = "SELECT playlistID FROM Playlist_uri WHERE Nume_Playlist=\'" + name + "\'";

            return cmd.ExecuteReader().Read();
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

            string rez = "";
            
            if (reader.Read())
                rez = reader.GetString(0);

            return rez;
        }

        public void InsertToRecent(string playlistName)
        {
            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            SQLiteDataReader reader;

            cmd.CommandText = "SELECT count(id) FROM PlaylistRecente";

            int count = 0;

            reader = cmd.ExecuteReader();

            if (reader.Read())
                count = reader.GetInt32(0);

            reader.Close();

            cmd.CommandText = "SELECT melodieID FROM Playlist_uri WHERE Nume_Playlist=\'" + playlistName + "\'";

            reader = cmd.ExecuteReader();

            int id = -1;

            if (reader.Read())
                id = reader.GetInt32(0);

            reader.Close();

            if (count > 0)
            {
                if (id != -1)
                {
                    cmd.CommandText = "SELECT id FROM Playlist_uri WHERE playlistID=" + id;

                    reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        reader.Close();
                        try
                        {
                            cmd.CommandText = "DELETE FROM Playlist_uri WHERE playlistID=" + id;

                            cmd.ExecuteNonQuery();
                        }
                        catch
                        {
                            MessageBox.Show("Stergerea nu a putut fi efectuata");
                            return;
                        }

                        try
                        {
                            cmd.CommandText = "INSERT INTO Playlist_uri (playlistID) VALUES (" + id + ")";

                            cmd.ExecuteNonQuery();
                        }
                        catch
                        {
                            MessageBox.Show("Inserarea nu s-a putut efectua");
                            return;
                        }
                    }
                    else
                    {
                        cmd.CommandText = "DELETE FROM Playlist_uri WHERE id=(SELECT MIN(id) FROM Playlist_uri)";

                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "INSERT INTO Playlist_uri (playlistID) VALUES (" + id + ")";

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                cmd.CommandText = "INSERT INTO Playlist_uri (playlistID) VALUES (" + id + ")";

                cmd.ExecuteNonQuery();
            }
        }

        public Dictionary<string, string> GetRecentPlaylists()
        {
            Dictionary<string, string> rez = new Dictionary<string, string>();

            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            SQLiteDataReader reader;

            List<int> idList = new List<int>();

            cmd.CommandText = "SELECT playlistID FROM PlaylistRecente";

            reader = cmd.ExecuteReader();

            while (reader.Read())
                idList.Add(reader.GetInt32(0));

            reader.Close();

            string temp;

            for (int i = idList.Count - 1; i >= 0; --i)
            {
                cmd.CommandText = "SELECT Nume_Playlist FROM Playlist_uri WHERE playlistID=" + idList.ElementAt(i);

                reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    temp = reader.GetString(0);
                    
                    rez[temp] = GetPlayListStats(temp);
                }

                reader.Close();
            }

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

            if (listID != -1)

                cmd.CommandText = "SELECT melodieID FROM Playlist WHERE playlistID=" + listID;

            else

                cmd.CommandText = "SELECT melodieID FROM Melodie";

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
            if (listName == "Toate melodiile" || listName == "")
                return GetListSongs(-1);

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

            if (playList == "Toate melodiile")
                rez = "Number of songs: " + length;
            else 
            if (duration % 60 > 9)
                rez = "Duration: " + (duration / 60) + ":" + (duration % 60) + " * " + "Number of songs: " + length;
            else
                rez = "Duration: " + (duration / 60) + ":0" + (duration % 60) + " * " + "Number of songs: " + length;

            if (playList == "Melodii Recente")
                rez = rez.Substring(0, rez.IndexOf(" *"));

            return rez;
        }

        public Dictionary<string, string> GetSongs()
        {
            Dictionary<string, string> rez = new Dictionary<string, string>();

            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            cmd.CommandText = "SELECT Nume_Melodie FROM Melodie";

            SQLiteDataReader reader = cmd.ExecuteReader();
            
            string song;

            while (reader.Read())
            {
                song = reader.GetString(0);
                rez[song] = GetSongStats(song);
            }

            return rez;
        }

        public string GetPrevSong(string playlist, string song)
        {
            Dictionary<string, string> list;

            if (playlist != "" && playlist != "Toate melodiile")
            {
                list = GetSongsFromPlayList(playlist);
            }
            else
            {
                list = GetSongs();
            }

            if (list.Keys.ElementAt(0) == song)
                return "";

            for (int i = 0; i < list.Keys.Count - 1; ++i)
            {
                if (list.Keys.ElementAt(i + 1) == song)
                    return list.Keys.ElementAt(i);
            }

            return "";
        }

        public string GetNextSong(string playlist, string song)
        {
            Dictionary<string, string> list;

            if (playlist != "" && playlist != "Toate melodiile")
            {
                list = GetSongsFromPlayList(playlist);
            }
            else
            {
                list = GetSongs();
            }

            int length = list.Keys.Count;

            if (list.Keys.ElementAt(length - 1) == song)
                return "";

            for (int i = length - 1; i > 0; --i)
            {
                if (list.Keys.ElementAt(i - 1) == song)
                    return list.Keys.ElementAt(i);
            }
            
            return "";
        }

        public string GetSongStats(string song)
        {
            string rez = "";

            SQLiteCommand cmd = _sqlConnection.CreateCommand();

            cmd.CommandText = "SELECT Duration FROM Melodie WHERE Nume_Melodie=\'" + song + "\'";

            SQLiteDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                int duration = reader.GetInt32(0);

                if (song.IndexOf("-") != -1)
                {
                    rez = song.Substring(0, song.IndexOf("-")).Trim(' ') + " ";
                }

                if (duration % 60 > 9)
                    rez += "Duration: " + duration / 60 + ":" + duration % 60;
                else
                    rez += "Duration: " + duration / 60 + ":0" + duration % 60; 
            }
            
            return rez;
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
