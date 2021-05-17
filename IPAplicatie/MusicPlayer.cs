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


namespace IPAplicatie
{
    class MusicPlayer
    {
        private Equalizer _eq;
        private ISoundOut _soundOut;

        public MusicPlayer()
        {
        }

        private void PlayFunc(string fileName)
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

            /*Thread t = new Thread(monitorPosition);

            t.Start();*/
        }

        /*private void monitorPosition()
        {
            while (_eq.Position / _eq.Length < 1.0f)
            {
                trackBar12.Value = (int)(100.0f * _eq.Position / _eq.Length);

                Thread.Sleep(1000);
            }
        }*/

        private void ChangeSong_Click()
        {
            var ofn = new OpenFileDialog();

            ofn.Filter = CodecFactory.SupportedFilesFilterEn;

            if (ofn.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PlayFunc(ofn.FileName);
            }
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
                _soundOut.Volume = value;
            }
        }
        

        public bool Play_Pause_Click()
        {
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

        public void DownloadProcedure(string link)
        {
            if (_soundOut != null && _soundOut.PlaybackState != PlaybackState.Stopped)
                Stop();

            ExecCommand("cmd.exe", "/c mkdir \"Samples\\audio.wav*\"");
            ExecCommand("cmd.exe", " /c del \"Samples\\audio.wav*\"");
            ExecCommand("youtube-dl.exe", "-f best " + ParseLink(link) + " -x --audio-format \"wav\" -o \"Samples\\audio.wav\" --write-thumbnail");
            foreach (string file in Directory.GetFiles("Samples"))
            {
                if (file.Contains(".webp"))
                {
                    ExecCommand("ffmpeg.exe", " -i \"" + file + "\" \"" + file.Replace(".webp", ".jpg") + "\"");
                }
            }
            ExecCommand("cmd.exe", " /c del \"Samples\\*.webp\"");
            PlayFunc("Samples\\audio.wav");
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
                _eq.Position = _eq.Length / value;
            }
        }
    }
}

