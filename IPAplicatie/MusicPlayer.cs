using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using CSCore;
using CSCore.Codecs;
using CSCore.SoundOut;
using CSCore.Streams;
using CSCore.Streams.Effects;
using System.Diagnostics;
using System.Threading;

namespace IPAplicatie
{
    class MusicPlayer
    {
        private Equalizer _eq;
        private ISoundOut _soundOut;
        private bool _isReady;

        public MusicPlayer()
        {
        }

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
            (source as LoopStream).StreamFinished += (s, args) => Stop();

            _eq = Equalizer.Create10BandEqualizer(source.ToSampleSource());

            _soundOut.Initialize(_eq.ToWaveSource(16));
            _soundOut.Play();

            ChangeVolume = (float)volume;

            _isReady = true;
        }

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

        public int monitorPosition()
        {
            if(_eq != null && _isReady)
                return (int)(100.0f * _eq.Position / _eq.Length);
            return 0;
        }

        public void Stop()
        {
            if (_soundOut != null)
            {
                try
                {
                    _soundOut.Stop();
                    _soundOut.Dispose();
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

        public void ChangeValue(TrackBar trackBar)
        {
            if (_eq != null)
            {
                const double MaxDB = 20;

                double perc = ((double)(trackBar.Value - 5) / (double)trackBar.Maximum);

                float value = (float)(perc * MaxDB);

                int filterIndex = Int32.Parse((string)trackBar.Tag);
                EqualizerFilter filter = _eq.SampleFilters[filterIndex];
                filter.AverageGainDB = value;
            }
        }

        public float ChangeVolume
        {
            set
            {
                if(_soundOut != null && _isReady)
                    _soundOut.Volume = value / 100.0f;
            }
        }
        

        public bool Play_Pause_Click()
        {
            if(_soundOut != null)
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
        private void ExecCommand(string command, string args)
        {
            var processInfo = new ProcessStartInfo(command, args);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);

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

        //youtube-dl -f best "URL" -x --audio-format "wav" -o "audio2.wav" --write-thumbnail
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

        public void DownloadProcedure(string link, int volume)
        {
            if (_soundOut != null && _soundOut.PlaybackState != PlaybackState.Stopped)
                Stop();

            ExecCommand("cmd.exe", "/c mkdir Samples");
            ExecCommand("cmd.exe", " /c del /Q \"Samples\\*\"");
            ExecCommand("youtube-dl.exe", "-f best " + ParseLink(link) + " -x --audio-format \"wav\" -o \"Samples\\audio.wav\" --write-thumbnail");
            foreach (string file in Directory.GetFiles("Samples"))
            {
                if (file.Contains(".webp"))
                {
                    ExecCommand("ffmpeg.exe", " -i \"" + file + "\" \"" + file.Replace(".webp", ".jpg") + "\"");
                }
            }
            ExecCommand("cmd.exe", " /c del \"Samples\\*.webp\"");

            PlayFunc("Samples\\audio.wav", volume);
        }

        private string ParseLink(string url)
        {
            string temp = url;
            if (temp.Contains("&list="))
                temp = temp.Substring(0, temp.IndexOf("&list="));
            return temp;
        }

        public int SetDuration
        {
            set
            {
                _eq.Position = _eq.Length / 100 * value;
            }
        }
    }
}

