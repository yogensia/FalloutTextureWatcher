#region License Information (GPL v3)

/*

    Fallout 4 Texture Watcher - Keeps an eye on your TGA source files
    and converts them to DDS when they are modified.

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program. If not, see <http://www.gnu.org/licenses/>.

*/

#endregion License Information (GPL v3)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Hardcodet.Wpf.TaskbarNotification;
using IniParser;
using IniParser.Model;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace FalloutTextureWatcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region MainWindow
        
        static FileSystemWatcher watcher;

        public MainWindow()
        {
            InitializeComponent();
            LoadINIFile();
            WindowSetup();
            SetupTrayIcon();
            SetupOutput();
            ConsoleWelcome();
            StartWatcher();
        }

        // Set window title, version, etc.
        private void WindowSetup()
        {
            // Increment version.
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            string displayableVersion = $"{version}";

            // Set window title.
            Title = "Fallout 4 Texture Watcher v" + displayableVersion;

            // Start in bottom right corner of desktop.
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;
        }

        public void SetupOutput()
        {
            using (var controlWriter = new ControlWriter(ConsoleBlock, ConsoleOutputScroll))
            {
                Console.SetOut(controlWriter);
            }
        }

        private static void ConsoleWelcome()
        {
            // Wait for the user to quit the program.
            Console.WriteLine("This tool is intended for testing materials " +
                "quickly. It's not an Elrich replacement. Elrich is still " +
                "recommended for converting final textures.");
            Console.WriteLine();
        }

        // Set FileSystemWatcher.
        // TODO Dispose of watcher and restart it when
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static void StartWatcher()
        {
            // Create a new FileSystemWatcher and set its properties.
            watcher = new FileSystemWatcher();

            // Check path is valid.
            if (!Directory.Exists(settingsData["General"]["SourcePath"]))
            {
                Console.WriteLine("Source directory not found, please select a valid directory!");
                TrayIconError();
            }
            else {
                watcher.Path = settingsData["General"]["SourcePath"];
            }

            // Watch for changes in LastAccess and LastWrite times, and
            // the renaming of files or directories.
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            // Only watch these files.
            watcher.Filter = "*.tga";

            // Watch subdirectories.
            watcher.IncludeSubdirectories = true;

            // Add event handlers.
            // Remark: Usually when a tga file is saved in Photoshop and other
            // tools, it's not changed but deleted, a new temp file is created,
            // then renamed, etc. This messes with the FileSystemWatcher events,
            // so when a tga is updated the deleted event may fire instead.
            // If a tga is really deleted this code will still send it to texconv,
            // which will close with a missing file error, no harm done there.
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Begin watching.
            if (Directory.Exists(settingsData["General"]["SourcePath"]))
                watcher.EnableRaisingEvents = true;
        }

        // Specify what is done when a file is changed, created, or deleted.
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            Thread.Sleep(200);

            string path = System.IO.Path.GetFileNameWithoutExtension(e.Name);

            // Only work on files ending with a suffix (_d, _s, _n...). 
            if (!Regex.Match(path, @"_.$").Success)
            {
                return;
            }

            // Set Tray icon to busy color.
            TrayIconBusy();

            // Use BC5 format on spec/gloss/normal maps.
            bool useBC5 = false;
            useBC5 = path.EndsWith("_s") || path.EndsWith("_n");

            string textureFormat = "BC1_UNORM";
            if (useBC5)
            {
                textureFormat = "BC5_UNORM";
            }

            // Add quotes to avoid issues with spaces.
            string fullPathQuotes = '"' + e.FullPath + '"';

            // Get output path.
            string outputPath = settingsData["General"]["DataPath"];
            outputPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(outputPath, e.Name));

            // Make sure the directory exists.
            string outputDir = System.IO.Path.GetDirectoryName(outputPath);
            string outputDirQuotes = '"' + outputDir + '"';
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // Build texconv.exe arguments.
            // TODO: check image size and set mipmaps accordingly with: -m 11
            string arguments = "-f " + textureFormat + " -y -o " + outputDirQuotes
                + " " + fullPathQuotes;

            // Create and run process.
            var process = new Process();
            process.StartInfo.FileName = "texconv.exe";
            process.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(e.FullPath);
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = false;

            // Start process.
            process.Start();

            // Don't wait forever!
            process.WaitForExit(500);

            Console.WriteLine(DateTime.Now.ToString("[h:mm:ss] tt") + e.Name +
                " >>> DDS " + textureFormat);

            // Set Tray icon to free color.
            TrayIconReady();
        }

        // Specify what is done when a file is renamed.
        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            // Ignored for now.
        }

        // Set Source path button.
        private void SetSourcePath(object sender, RoutedEventArgs e)
        {
            using (var dlg = new CommonOpenFileDialog
            {
                Title = "Set Source/Textures Directory",
                IsFolderPicker = true,
                InitialDirectory = SourceTextBox.Text,
                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                DefaultDirectory = SourceTextBox.Text,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            })
            {
                // TODO: Reorganize.
                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    SourceTextBox.Text = dlg.FileName;
                    settingsData["General"]["SourcePath"] = dlg.FileName;
                    SaveSettings();
                    TrayIconReady();
                    watcher.Path = settingsData["General"]["SourcePath"];
                    watcher.EnableRaisingEvents = true;
                    Console.WriteLine("Source path changed to: '" + dlg.FileName + "'");
                }
            }
        }

        // Set Data path button.
        private void SetDataPath(object sender, RoutedEventArgs e)
        {
            using (var dlg = new CommonOpenFileDialog
            {
                Title = "Set Data/Textures Directory",
                IsFolderPicker = true,
                InitialDirectory = DataTextBox.Text,
                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                DefaultDirectory = DataTextBox.Text,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            })
            {
                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    DataTextBox.Text = dlg.FileName;
                    settingsData["General"]["DataPath"] = dlg.FileName;
                    SaveSettings();
                    Console.WriteLine("Data path changed to: '" + dlg.FileName + "'");
                }
            }
        }

        #endregion MainWindow

        #region Settings

        static IniData settingsData;

        private void LoadINIFile()
        {
            var parser = new FileIniDataParser();

            if (File.Exists("Settings.ini"))
            {
                settingsData = parser.ReadFile("Settings.ini");
            }
            else
            {
                // Create INI file if not found.
                byte[] array = Encoding.ASCII.GetBytes(Properties.Resources.Settings);
                File.WriteAllBytes(@"Settings.ini", array);

                settingsData = parser.ReadFile("Settings.ini");
            }

            SourceTextBox.Text = settingsData["General"]["SourcePath"];
            DataTextBox.Text = settingsData["General"]["DataPath"];
        }

        private static void SaveSettings()
        {
            var parser = new FileIniDataParser();

            parser.WriteFile("Settings.ini", settingsData);
        }

        // Make sure Source directory exists.
        // TODO: Deprecated.
        private static bool CheckSourcePath(string path)
        {
            if (!Directory.Exists(path))
            {
                Console.WriteLine("Source directory could not be found!");
                Console.WriteLine();
                return false;
            }
            else
            {
                return true;
            }
        }

        // Make sure Data directory exists.
        // TODO: Deprecated.
        private static bool CheckDataPath(string path)
        {
            if (!Directory.Exists(path))
            {
                Console.WriteLine("Data directory could not be found!");
                Console.WriteLine();
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion Settings

        #region TrayIcon

        static TaskbarIcon tbi;

        private void SetupTrayIcon()
        {
            tbi = (TaskbarIcon)FindResource("TrayIcon");
            //tbi.TrayLeftMouseUp += new RoutedEventHandler(TrayIconCmdShowHide);
            tbi.TrayMouseDoubleClick += new RoutedEventHandler(TrayIconCmdShowHide);
        }

        private static void TrayIconReady()
        {
            tbi.Icon = Properties.Resources.Binoculars;
            tbi.ToolTipText = "F4TW - Ready...";
        }

        private static void TrayIconBusy()
        {
            tbi.Icon = Properties.Resources.BinocularsOrange;
            tbi.ToolTipText = "F4TW - Working!";
        }

        private static void TrayIconError()
        {
            tbi.Icon = Properties.Resources.BinocularsGrey;
            tbi.ToolTipText = "F4TW - Check Settings!";
        }

        private void TrayIconCmdShowHide(object source, RoutedEventArgs e)
        {
            if (ShowInTaskbar)
            {
                ShowInTaskbar = false;
                Visibility = Visibility.Collapsed;
            }
            else
            {
                ShowInTaskbar = true;
                Visibility = Visibility.Visible;
            }
        }

        #endregion TrayIcon
    }
}
