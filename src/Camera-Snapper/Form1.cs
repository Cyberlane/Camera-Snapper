using AForge.Video.DirectShow;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Camera_Snapper
{
    public partial class Form1 : Form
    {
        private string _settingsPath;
        private string _snapshotsPath;
        private const string FolderName = "Camera-Snapper";
        private int _saveFrequency;
        private VideoCaptureDevice _captureDevice;
        private Bitmap _currentSnapshot;

        public Form1()
        {
            InitializeComponent();
            InitialiseConfigFile();
            Stream();
        }

        private void InitialiseConfigFile()
        {
            _snapshotsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), FolderName);
            if (!Directory.Exists(_snapshotsPath))
            {
                Directory.CreateDirectory(_snapshotsPath);
            }
            ConfigSettings settings;
            _settingsPath = Path.Combine(_snapshotsPath, "setting.json");
            if (!File.Exists(_settingsPath))
            {
                _snapshotsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), FolderName);
                _saveFrequency = 60;
                Directory.CreateDirectory(_snapshotsPath);
                settings = new ConfigSettings
                {
                    SnapshotPath = _snapshotsPath,
                    SaveFrequency = _saveFrequency
                };
            }
            else
            {
                settings = LoadSettings();
                _snapshotsPath = settings.SnapshotPath;
                _saveFrequency = settings.SaveFrequency;
                if (!Directory.Exists(_snapshotsPath))
                {
                    Directory.CreateDirectory(_snapshotsPath);
                }
            }
            UpdateSettings(settings);
        }

        private ConfigSettings LoadSettings()
        {
            var raw = File.ReadAllText(_settingsPath);
            return JsonConvert.DeserializeObject<ConfigSettings>(raw);
        }

        private void UpdateSettings(ConfigSettings config)
        {
            var raw = JsonConvert.SerializeObject(config);
            File.WriteAllText(_settingsPath, raw);
        }

        private void Stream()
        {
            var devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (devices.Count == 0) return;
            if (devices.Count == 1)
            {
                _captureDevice = new VideoCaptureDevice(devices[0].MonikerString);
            }
            else
            {
                //TODO: Show a list of all cameras
            }
            _captureDevice.NewFrame -= source_NewFrame;
            _captureDevice.NewFrame += source_NewFrame;
            _captureDevice.Start();
        }

        void source_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            try
            {
                if (_currentSnapshot != null)
                    _currentSnapshot.Dispose();
                _currentSnapshot = new Bitmap(eventArgs.Frame);
            }
            catch { }

            this.Invoke((MethodInvoker)delegate
            {
                pictureBox1.Image = _currentSnapshot;
            });

            GC.Collect();

            this.Invoke((MethodInvoker)delegate
            {
                pictureBox1.Refresh();
            });
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            _captureDevice.SignalToStop();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            //TODO: Save form settings to config and local variables
        }
    }
}
