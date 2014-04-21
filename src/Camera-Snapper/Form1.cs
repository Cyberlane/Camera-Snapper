using AForge.Video.DirectShow;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
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
                if (_saveFrequency == 0)
                {
                    settings.SaveFrequency = 60;
                    _saveFrequency = 60;
                }
                if (!Directory.Exists(_snapshotsPath))
                {
                    Directory.CreateDirectory(_snapshotsPath);
                }
            }
            UpdateSettings(settings);
            UpdateFormSettings(settings);
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

        private void UpdateFormSettings(ConfigSettings config)
        {
            txtSavePath.Text = config.SnapshotPath;
            txtSaveFrequency.Text = config.SaveFrequency.ToString();
            if (config.SaveFrequency == 0) return;
            timer1.Interval = config.SaveFrequency * 1000;
        }

        private void UpdateSettingsFromForm()
        {
            _snapshotsPath = txtSavePath.Text;
            _saveFrequency = int.Parse(txtSaveFrequency.Text);
            var config = new ConfigSettings
            {
                SaveFrequency = _saveFrequency,
                SnapshotPath = _snapshotsPath
            };
            UpdateSettings(config);
            timer1.Interval = config.SaveFrequency * 1000;
        }

        private void Stream()
        {
            var devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            var list = devices.Cast<FilterInfo>().ToArray();
            cmbDeviceList.DisplayMember = "Name";
            cmbDeviceList.ValueMember = "MonikerString";
            cmbDeviceList.Items.AddRange(list);
            if (devices.Count > 0)
            {
                SetCaptureDevice(devices[0].MonikerString);
                cmbDeviceList.SelectedIndex = 0;
            }
            if (devices.Count == 0)
            {
                cmbDeviceList.Text = "No Devices Found";
            }
            if (devices.Count > 1)
            {
                cmbDeviceList.Enabled = false;
            }
        }

        private void SetCaptureDevice(string moniker)
        {
            if (_captureDevice != null)
            {
                _captureDevice.Stop();
            }
            _captureDevice = new VideoCaptureDevice(moniker);
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


            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    lblLoading.Visible = false;
                    pictureBox1.Image = _currentSnapshot;
                });
            }
            catch (ObjectDisposedException) { }

            GC.Collect();

            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    pictureBox1.Refresh();
                });
            }
            catch (ObjectDisposedException) { }
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
            UpdateSettingsFromForm();
        }

        private void txtSaveFrequency_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) &&
                !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void cmbDeviceList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var info = ((ComboBox)sender).SelectedItem as FilterInfo;
            SetCaptureDevice(info.MonikerString);
        }

        private void btnTimerStatus_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            if (btn.Text == "Start")
            {
                timer1.Start();
                btn.Text = "Stop";
            }
            else
            {
                timer1.Stop();
                btn.Text = "Start";
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_currentSnapshot != null)
            {
                if (!Directory.Exists(_snapshotsPath))
                {
                    Directory.CreateDirectory(_snapshotsPath);
                }
                var filename = string.Format("{0:yyyy-MM-dd_HH-mm-ss}.png", DateTime.Now);
                var savePath = Path.Combine(_snapshotsPath, filename);
                _currentSnapshot.Save(savePath, ImageFormat.Png);
            }
        }
    }
}
