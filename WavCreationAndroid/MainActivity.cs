using Android.App;
using Android.Widget;
using Android.OS;
using Android.Media;
using System;
using System.Threading.Tasks;
using WavCreator;
using System.IO;
using System.Linq;

namespace WavCreationAndroid
{
    [Activity(Label = "WavCreationAndroid", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        private AudioRecord audRecorder;
        private MediaPlayer player;
        private Button btnStart;
        private Button btnStop;
        private byte[] audioBuffer;
        private bool isRecording;

        private const string path = "/sdcard/MySound3.wav";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            btnStart = FindViewById<Button>(Resource.Id.buttonStart);
            btnStop = FindViewById<Button>(Resource.Id.buttonStop);

            InitializeMic();

            btnStart.Click += (o, i) =>
            {
                btnStart.Enabled = !btnStart.Enabled;
                btnStop.Enabled = !btnStop.Enabled;
                isRecording = true;

                RecordAudio();
            };

            btnStop.Click += (o, i) =>
            {
                btnStart.Enabled = !btnStart.Enabled;
                btnStop.Enabled = !btnStop.Enabled;

                // stop recording
                audRecorder.Stop();
                audRecorder.Release();
                isRecording = false;

                // try to read and play the file
                player.SetDataSource(path);
                player.Prepare();
                player.Start();
            };

        }

        private void InitializeMic()
        {
            audioBuffer = new byte[200000];
            audRecorder = new AudioRecord(
              // Hardware source of recording.
              AudioSource.Mic,
              // Frequency
              44100,
              // Mono or stereo
              ChannelIn.Mono,
              // Audio encoding
              Encoding.Pcm16bit,
              // Length of the audio clip.
              audioBuffer.Length
            );
        }

        private void ReadAudioAsync()
        {
            while (isRecording)
            {
                try
                {
                    // Keep reading the buffer while there is audio input.
                    audRecorder.Read(audioBuffer, 0, audioBuffer.Length);
                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex.Message);
                    break;
                }
            }

            byte[] realData = audioBuffer;
            Console.WriteLine(realData.Length);
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                WaveHeaderWriter.WriteHeader(fs, realData.Length, 1, 44100);
                fs.Write(realData, 0, realData.Length);
                fs.Close();
            }
        }

        private async void RecordAudio()
        {
            audRecorder.StartRecording();
            await Task.Run(() => ReadAudioAsync());
        }

        protected override void OnResume()
        {
            base.OnResume();

            player = new MediaPlayer();

            player.Completion += (sender, e) => {
                player.Reset();
                btnStart.Enabled = !btnStart.Enabled;
            };
        }

        protected override void OnPause()
        {
            base.OnPause();

            player?.Release();
            player?.Dispose();
            player = null;
            audRecorder?.Stop();
            audRecorder?.Release();
            audRecorder = null;
        }
    }
}

