using Isolib.Functions;
using Isolib.IOPackage;
using Isolib.STFSPackage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.IO;
using TooHuman1SE.SEStructure;
using TooHuman1SE.SEFunctions;
using TooHuman1SE.User_Controls;
using TooHuman1SE.Windows;
using MessageBox = System.Windows.MessageBox;

namespace TooHuman1SE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public static class MyDirectory
    {   // Regex version
        public static IEnumerable<string> GetFiles(string path,
                            string searchPatternExpression = "",
                            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            Regex reSearchPattern = new Regex(searchPatternExpression, RegexOptions.IgnoreCase);
            return Directory.EnumerateFiles(path, "*", searchOption)
                            .Where(file =>
                                     reSearchPattern.IsMatch(System.IO.Path.GetExtension(file)));
        }

        // Takes same patterns, and executes in parallel
        public static IEnumerable<string> GetFiles(string path,
                            string[] searchPatterns,
                            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return searchPatterns.AsParallel()
                   .SelectMany(searchPattern =>
                          Directory.EnumerateFiles(path, searchPattern, searchOption));
        }
    }

    public partial class MainWindow : Window
    {
        public TH1RuneMCollection runeCollection = new TH1RuneMCollection();

        internal static CharactersUC _CharactersUC = new CharactersUC();
        internal static LogUC _LogUC = new LogUC();
        internal static LogWindow _LogWindow = new LogWindow();

        public MainWindow()
        {
            InitializeComponent();
            SetContentControl();
            LoadLogWindow();
        }

        private void SetContentControl()
        {
            SwitchCC(_CharactersUC);
        }

        private void SwitchCC( System.Windows.Controls.UserControl CC)
        {
            this.MainCC.Content = CC;
            // this.lblUC.Content = CC.ToolTip + ":";
        }

        private void LoadLogWindow()
        {
            _LogWindow.MainCC.Content = _LogUC;
        }

        #region FileOpening

        private void loadFilesIntoList( string[] files)
        {
            foreach( string filename in files)
            {
                TH1SaveStructure newsave = new TH1SaveStructure();
                Functions.log("Reading " + filename, Functions.LC_PRIMARY);
                newsave.readSaveFile(filename);
                if( newsave.lastError == 0 )
                {
                    if (!newsave.hashVerified) { Functions.log("Save file contains an invalid hash", Functions.LC_WARNING);  }
                    // newsave.writeAllSectors(false); // Debugging Type activity
                    Functions.log("File Read OK! Importing Character..");
                    Functions.importCharSave(newsave);
                    Functions.log("Character Imported.", Functions.LC_SUCCESS);
                } else {
                    Functions.log("Error " + newsave.lastError.ToString() + ": " + newsave.lastErrorMsg, Functions.LC_CRITICAL); // FYI
                }
            }
        }

        private void ChooseFile()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            // dlg.DefaultExt = ".png";
            dlg.Filter = "TH1 Xbox360 Save (TH1_Save*)|TH1_Save*|TH1 Decompressed Save (savegame.txt)|*.txt|All Files (*)|*";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string[] filename = new string[1];
                filename[0] = dlg.FileName;
                loadFilesIntoList(filename);
            }
        }


        #endregion FileOpening

        #region Menu Items

        private void mnu_importSave(object sender, RoutedEventArgs e)
        {
            ChooseFile();
        }

        private void mnu_showLog(object sender, RoutedEventArgs e)
        {
            _LogWindow.Show();
            _LogWindow.WindowState = WindowState.Normal;
            _LogWindow.Activate();
         }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void mnu_exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void mnu_importDir(object sender, RoutedEventArgs e)
        {
            // Functions funcs = new Functions();
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    Functions.log("Scanning (" + fbd.SelectedPath + ")");
                    IEnumerable<string> files = MyDirectory.GetFiles(fbd.SelectedPath, new string[] { "TH1_SaveGame*", "*.txt" });
                    if (files.Count() > 0)
                    {
                        Functions.log("Loading " + files.Count().ToString() + " Files");
                        loadFilesIntoList(files.ToArray());
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("No Files Found!");
                        Functions.log("No Files Found");
                    }
                }
            }
        }

        private void mnu_RebuildList(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not Yet Built");
        }

        private void mnu_webGithub(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/xJam-es/TooHuman1SE");
        }

        private void mnu_webBlog(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://toohuman1se.xjam.es/"); 
        }

        #endregion Menu Items
    }
}
