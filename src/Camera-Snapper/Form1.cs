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

        public Form1()
        {
            InitializeComponent();
            InitialiseConfigFile();
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
                Directory.CreateDirectory(_snapshotsPath);
                settings = new ConfigSettings
                {
                    SnapshotPath = _snapshotsPath
                };
            }
            else
            {
                settings = LoadSettings();
                _snapshotsPath = settings.SnapshotPath;
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
    }
}
