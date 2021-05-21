using System;
using System.IO;
using System.Windows.Forms;
using CSCore;
using CSCore.Codecs;
using CSCore.SoundOut;
using CSCore.Streams;
using CSCore.Streams.Effects;
using System.Diagnostics;
using System.Net;

namespace MusicPlayer
{
    public class MusicPlayer
    {
        private Equalizer _eq; // prin intermediul acestei variabile vom modifica progresul melodiei si vom genera si modifica canalele pentru equalizer
        private ISoundOut _soundOut; // prin intermediul acestei variabile vom reda, vom opri si vom modifica volumul fisierul audio
        private bool _isReady; // acest flag determina daca aplicatia se afla sau nu in procesul de inlocuire a fisierelor din directorul de media

        public MusicPlayer()
        {
        }

        // Se va obtine handler-ul pentru redarea melodiei
        // se vor genera 10 canale pentru equalizer
        // Se va seta volumul cu valoarea de la slider-ul de volum din interfata grafica
        private void PlayFunc(string fileName, int volume)
        {
            Stop();

            if (WasapiOut.IsSupportedOnCurrentPlatform)
            {
                _soundOut = new WasapiOut();
            }
            else
            {
                _soundOut = new DirectSoundOut();
            }

            var source = CodecFactory.Instance.GetCodec(fileName);

            source = new LoopStream(source) { EnableLoop = false };
            ((LoopStream)source).StreamFinished += (s, args) => Stop();

            _eq = Equalizer.Create10BandEqualizer(source.ToSampleSource());

            _soundOut.Initialize(_eq.ToWaveSource(16));
            _soundOut.Play();

            ChangeVolume = (float)volume;

            _isReady = true;
        }

        // Proprietate folosita in MainForm pentru interogarea instantei de tip MusicPlayer in legatura cu starea in care se afla aplicatia
        // Aceast flag devine fals in momentul in care se modifica o melodie
        // Si va deveni true cand se incheie procesul de download si redare a noii melodii
        public bool Ready
        {
            set
            {
                _isReady = value;
            }
            get
            {
                return _isReady;
            }
        }

        // Este necesar pentru a afisa cu acuratete progresul melodiei in interfata grafica
        // Va fi apelata de evenimentul tick al elementului TimerSong
        public string monitorTime(int duration, int percentage)
        {
            if (_eq != null && _isReady)
            {
                int currentTime = duration * percentage / 100;

                if (currentTime % 60 > 9)

                    return currentTime / 60 + ":" + currentTime % 60 + "/";

                else

                    return currentTime / 60 + ":0" + currentTime % 60 + "/";
            }

            return "";
        }

        // Va returna in procente progresul melodiei
        public int monitorPosition()
        {
            if (_eq != null && _isReady)
                return (int)(100.0f * _eq.Position / _eq.Length);
            return 0;
        }

        // Procedura de oprire si eliminare a handler-lor audio
        public void Stop()
        {
            if (_soundOut != null)
            {
                try
                {
                    if (_soundOut != null)
                    {
                        _soundOut.Stop();
                        _soundOut.Dispose();
                    }
                    if (_eq != null)
                        _eq.Dispose();
                    _soundOut = null;
                    _eq = null;
                }
                catch (Exception err)
                {
                    Console.WriteLine(err);
                }
            }
        }

        // Aceasta metoda este apelata de fiecare data cand un slider din panoul equalizer este modificat
        // Numarul canalului va fi memorat in proprietatea tag al elementului de tip TrackBar
        // Astfel cele 10 slidere din panoul equalizer au la rubrica Tag cate o valoare de la 0-9
        public void ChangeValue(TrackBar trackBar)
        {
            if (_eq != null)
            {
                const double MaxDB = 40;

                double perc = ((double)(trackBar.Value - 5) / (double)trackBar.Maximum);

                float value = (float)(perc * MaxDB);

                int filterIndex = Int32.Parse((string)trackBar.Tag);
                EqualizerFilter filter = _eq.SampleFilters[filterIndex];
                filter.AverageGainDB = value;
            }
        }

        // Se foloseste la schimbarea efectiva volumului prin modificarea handler-ului audio
        // Este necesar in acelasi timp sa se verifice daca melodia este in procesul de redare si daca handler-ului este nul
        public float ChangeVolume
        {
            set
            {
                if (_soundOut != null && _isReady)
                    _soundOut.Volume = value / 100.0f;
            }
        }

        // Se va apela in momentul in care se apasa buton de play/pause din interfata grafica
        public bool Play_Pause_Click()
        {
            if (_soundOut != null)
                if (_soundOut.PlaybackState == PlaybackState.Playing)
                {
                    _soundOut.Pause();
                }
                else if (_soundOut.PlaybackState == PlaybackState.Paused)
                {
                    _soundOut.Play();
                    return true;
                }

            return false;
        }

        // Pentru utilizarea utilitarului youtube-dl vom apela functii de sistem
        // Din acest motiv am creat aceasta metoda ce va simplifica acest proces prin intermediul argumentelor
        private void ExecCommand(string command, string args)
        {
            var processInfo = new ProcessStartInfo(command, args);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            Process process;

            try
            {
                process = Process.Start(processInfo);
            }
            catch
            {
                MessageBox.Show("Lipseste una din executabilele dependenta.");
                return;
            }


            process.OutputDataReceived += (object send, DataReceivedEventArgs ev) =>
            Console.WriteLine("output>>" + ev.Data);
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (object send, DataReceivedEventArgs ev) =>
            Console.WriteLine("error>>" + ev.Data);
            process.BeginErrorReadLine();

            process.WaitForExit();

            Console.WriteLine("ExitCode: {0}", process.ExitCode);
            process.Close();
        }

        // Metoda de a obtine titlul unui videoclip
        public string GetName(string url)
        {
            string line = "";
            var processInfo = new ProcessStartInfo("youtube-dl.exe", "-e " + ParseLink(url));
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);

            while (!process.StandardOutput.EndOfStream)
            {
                line = process.StandardOutput.ReadLine();
            }

            process.WaitForExit();

            Console.WriteLine("ExitCode: {0}", process.ExitCode);
            process.Close();

            return line;
        }

        // Metoda de a obtine durata unui videoclip
        public int GetDuration(string url)
        {
            string line = "";
            var processInfo = new ProcessStartInfo("youtube-dl.exe", "--get-duration " + ParseLink(url));
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);

            while (!process.StandardOutput.EndOfStream)
            {
                line = process.StandardOutput.ReadLine();
            }

            process.WaitForExit();

            Console.WriteLine("ExitCode: {0}", process.ExitCode);
            process.Close();

            if (line != "")
            {
                int duration = 0;

                string[] time = line.Split(':');

                for (int i = time.Length - 1; i >= 0; --i)
                    duration += (time.Length - i) * 60 + Convert.ToInt32(time[i]);

                return duration;
            }
            else
                return 0;
        }

        // Metoda creata pentru a simplifica procedeul de downloadare a unei melodii
        // Melodia va fi in format audio
        // In plus in acelasi director vom salva si thumbnail-ul corespunzator melodiei
        public void DownloadProcedure(string link, int volume)
        {
            // Oprim si eliminam melodia din prezent pentru a evita accesarea fisierului audio dupa ce acesta este sters in pasurile urmatoare
            if (_soundOut != null && _soundOut.PlaybackState != PlaybackState.Stopped)
                Stop();

            ExecCommand("cmd.exe", "/c mkdir Samples");
            ExecCommand("cmd.exe", " /c del /Q \"Samples\\*\"");
            ExecCommand("youtube-dl.exe", "-f best " + ParseLink(link) + " -x --audio-format \"wav\" -o \"Samples\\audio.wav\"");

            DownloadThumbnail(ParseLink(link));

            try
            {
                PlayFunc("Samples\\audio.wav", volume);
            }
            catch
            {
                MessageBox.Show("Redarea melodiei nu s-a facut cu succes. Incearca din nou");
                return;
            }
        }

        // Metoda de obtinere a thumbnail-ului
        private void DownloadThumbnail(string vid_id)
        {
            string path = "Samples/audio.jpg";
            try
            {
                string id = vid_id.Split('=')[1];

                if (File.Exists(@path))
                    File.Delete(@path);

                WebClient client = new WebClient();
                client.DownloadFileAsync(new Uri("https://i.ytimg.com/vi/" + id + "/mqdefault.jpg"), @path);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare la descarcarea thumbnailului.\n" + ex.Message);
                return;
            }
        }

        // Este utila in cazul in care utilizatorul introduce o melodie dintr-un mix/playlist de pe youtube
        // Pentru a evita acest lucru vom parsa link-ul in asa fel incat sa nu accesam videoclipul din lista
        private string ParseLink(string url)
        {
            string temp = url;
            if (temp.Contains("&list="))
                temp = temp.Substring(0, temp.IndexOf("&list="));
            return temp;
        }

        // Prin acest mod vom seta progresul melodiei
        // Se va apela in momentul in care se modifica slider-ul de durata din interfata grafica
        public int SetDuration
        {
            set
            {
                if (this.Ready)
                    _eq.Position = _eq.Length / 100 * value;
            }
        }
    }
}

