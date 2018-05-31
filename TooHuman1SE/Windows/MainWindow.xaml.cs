using Isolib.Functions;
using Isolib.IOPackage;
using Isolib.STFSPackage;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Diagnostics;
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
        // All Collections (For Pre-Loading)
        public TH1Collections db;

        // Let's Go
        internal static CharactersUC _CharactersUC = new CharactersUC();
        internal static LogUC _LogUC = new LogUC();
        internal static LogWindow _LogWindow = new LogWindow();

        public MainWindow()
        {
            InitializeComponent();
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = string.Format(" v{0}.{1}.{2}", fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart);
            this.Title = this.Title + version;
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
        }

        private void LoadLogWindow()
        {
            _LogWindow.MainCC.Content = _LogUC;
        }

        public void enableInteractions()
        {
            mnuFile.IsEnabled = true;
            mnuRebuild.IsEnabled = true;
        }

        private void sortCharacterList( int sortType, ObservableCollection<CharListImages> listToSort)
        {
            if (listToSort == null) return;

            int i = 0;
            bool breakout = false;

            while( i < listToSort.Count && !breakout)
            {
                if ( i > 0)
                {
                    CharListImages thisItem = listToSort[i];
                    CharListImages prevItem = listToSort[i-1];

                    bool switchout = false;
                    switch( sortType)
                    {
                        case 0:
                            DateTime thisPlayed = DateTime.Parse(thisItem.lastplayed);
                            DateTime prevPlayed = DateTime.Parse(prevItem.lastplayed);
                            switchout = (prevPlayed < thisPlayed);
                            break;
                        case 1:
                            long thisExp = long.Parse(thisItem.exp, NumberStyles.AllowThousands);
                            long prevExp = long.Parse(prevItem.exp, NumberStyles.AllowThousands);
                            switchout = (prevExp < thisExp);
                            break;
                        case 2:
                            switchout = (string.Compare(thisItem.name, prevItem.name) == -1);
                            break;
                    }

                    if(switchout)
                    {
                        listToSort.Move(i, i - 1);
                        breakout = true;
                        sortCharacterList(sortType, listToSort);
                    }
                }
                i++;
            }

        }

        #region FileOpening

        private void loadFilesIntoList( string[] files)
        {
            foreach( string filename in files)
            {
                TH1SaveStructure newsave = new TH1SaveStructure();
                Functions.log("Reading " + filename, Functions.LC_PRIMARY);

                newsave.db = this.db;
                newsave.readSaveFile(filename);

                if( newsave.lastError == 0 )
                {
                    if (!newsave.hashVerified) { Functions.log("Save file contains an invalid hash", Functions.LC_WARNING);  }
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
            dlg.Filter = "TH1 Xbox360 Save|TH1_Save*|TH1 Decompressed Save|*.txt|All Files|*";

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
            string saveDir = "saves";
            string fileExt = ".th1";

            Functions.log("Rebuilding Character List", Functions.LC_PRIMARY);

            if (File.Exists(Functions.charPath)) File.Delete(Functions.charPath);
            if (_CharactersUC.lstCharacters.ItemsSource != null)
            {
                ObservableCollection<CharListImages> list = (ObservableCollection<CharListImages>)_CharactersUC.lstCharacters.ItemsSource;
                list.Clear();
            }


            if ( Directory.Exists(saveDir))
            {
                var files = Directory.EnumerateFiles(saveDir, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(fileExt, StringComparison.OrdinalIgnoreCase));

                loadFilesIntoList(files.ToArray());
            }

        }

        private void mnu_webGithub(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/xJam-es/TooHuman1SE");
        }

        private void mnu_webWiki(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/xJam-es/TooHuman1SE/wiki"); 
        }

        private void mnuSortBy_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem mnu = sender as System.Windows.Controls.MenuItem;

            if (mnu == null) return;
            int sortType = int.Parse((string)mnu.Tag);

            sortCharacterList(sortType, (ObservableCollection<CharListImages>)_CharactersUC.lstCharacters.ItemsSource);
        }

        private void mnu_question(object sender, RoutedEventArgs e)
        {
            string rn = "\r\n";
            string sep = "\r\n+------------------------------------+\r\n";
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            MessageBox.Show(
                string.Format("Too Human 1 Save Editor{0}aka \"9 Years Too Late\"{1}Version: {2}{0}Created By: xJam.es{1}Credits To:{0}Jappi88 & Pureis (Isolib){0}Newtonsoft (Json){0}Fandom (Too Human Wiki){0}Silicon Knights (R.I.P)", rn,sep, version),
                "Too Human 1 Save Editor",
                MessageBoxButton.OK,
                MessageBoxImage.Information
                );
        }

        #endregion Menu Items
    }
}
