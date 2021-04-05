using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Input;
using MVVM_ClassLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.WindowsAPICodePack.Dialogs;


namespace AmongUsToggler
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _selectedPath;
        private string _enabledString;

        public ICommand StartCommand
        {
            get => _startCommand;
            set
            {
                _startCommand = value;
                OnPropertyChanged(nameof(StartCommand));
            }
        }

        public ICommand ToggleModsCommand
        {
            get => _toggleModsCommand;
            set
            {
                _toggleModsCommand = value;
                OnPropertyChanged(nameof(ToggleModsCommand));
            }
        }

        public ICommand ChangeAmongUsDirectoryCommand
        {
            get => _changeAmongUsDirectoryCommand;
            set
            {
                _changeAmongUsDirectoryCommand = value;
                OnPropertyChanged(nameof(ChangeAmongUsDirectoryCommand));
            }
        }

        public string SelectedPath
        {
            get => _selectedPath;
            set
            {
                _selectedPath = value;
                OnPropertyChanged(nameof(SelectedPath));
            }
        }

        public string EnabledString
        {
            get => _enabledString;
            set
            {
                _enabledString = value;
                OnPropertyChanged(nameof(EnabledString));
            }
        }

        public bool AreModsEnabled
        {
            get => _areModsEnabled;
            set
            {
                _areModsEnabled = value;
                EnabledString = value ? "Aktiv" : "Aus";
            }
        }

        private const string DefaultJson = "{\n\t\"enabled\": true,\n\t\"BepinExPath\": \"\"\n}";

        private readonly string _dataFileLocation =
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
            @"\BrainyQDev\AmongUsToggler\Settings.json";

        private bool _areModsEnabled;
        private ICommand _changeAmongUsDirectoryCommand;
        private ICommand _toggleModsCommand;
        private ICommand _startCommand;

        public MainWindowViewModel()
        {
            StartCommand = new RelayCommand(StartGame);
            ToggleModsCommand = new RelayCommand(ToggleMods);
            ChangeAmongUsDirectoryCommand = new RelayCommand(ChangeAmongUsDirectory);

            if (!File.Exists(_dataFileLocation))
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                          @"\BrainyQDev\AmongUsToggler");
                var file = File.Create(_dataFileLocation);
                file.Close();
                File.WriteAllText(_dataFileLocation, DefaultJson);
            }

            var json = File.ReadAllText(_dataFileLocation);
            var saved = JsonConvert.DeserializeObject<JsonSaveClass>(json);
            AreModsEnabled = saved.enabled;
            SelectedPath = saved.BepinExPath;
            var path = SelectedPath + "\\BepInEx_\\";
            if (Directory.Exists(path))
            {
                AreModsEnabled = false;
            }
            path = SelectedPath + "\\BepInEx\\";
            if (Directory.Exists(path))
            {
                AreModsEnabled = true;
            }
        }

        private void StartGame()
        {
            var path = SelectedPath + "\\Among us.exe";
            Process.Start(path);
        }

        private void ToggleMods()
        {
            AreModsEnabled = !AreModsEnabled;
            SaveCurrent();
            if (AreModsEnabled)
            {
                var path = SelectedPath + "\\BepInEx_\\";

                if (!Directory.Exists(path))
                {
                    MessageBox.Show("Mods nicht installiert");
                    return;
                }

                Directory.Move(path, path.Remove(path.Length - 2));
            }
            else
            {
                var path = SelectedPath + "\\BepInEx\\";

                if (!Directory.Exists(path))
                {
                    MessageBox.Show("Mods nicht installiert");
                    return;
                }

                Directory.Move(path, path.Remove(path.Length - 1) + "_");
            }
        }

        private void ChangeAmongUsDirectory()
        {
            using (var folderSelector = new OpenFileDialog())
            {
                folderSelector.ValidateNames = false;
                folderSelector.CheckFileExists = false;
                folderSelector.CheckPathExists = false;
                folderSelector.FileName = "Among Us";

                var result = folderSelector.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrEmpty(folderSelector.FileName))
                {
                    SelectedPath = Path.GetDirectoryName(folderSelector.FileName);
                }
            }

            SaveCurrent();
        }

        private void SaveCurrent()
        {
            var toSave = new JsonSaveClass
            {
                enabled = AreModsEnabled,
                BepinExPath = SelectedPath
            };
            var json = JsonConvert.SerializeObject(toSave);
            File.WriteAllText(_dataFileLocation, json);
            
            
        }
    }
}