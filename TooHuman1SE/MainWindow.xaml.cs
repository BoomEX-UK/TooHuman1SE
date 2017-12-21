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
using System.IO;
using TooHuman1SE.SEFunctions;

namespace TooHuman1SE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        internal static MessagesPage _MessagesPage = new MessagesPage();

        public MainWindow()
        {
            InitializeComponent();
            CreateMessagesFrame();
        }

        private void CreateMessagesFrame()
        {

            MainFrame.NavigationService.Navigate(_MessagesPage);
        }

        private void btnOpenDir_Click(object sender, RoutedEventArgs e)
        {
            // Functions funcs = new Functions();
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    Functions.log("Scanning (" + fbd.SelectedPath + ")");
                    string[] files = Directory.GetFiles(fbd.SelectedPath,"TH1_SaveGame*");
                    if (files.Length > 0)
                    {
                        Functions.log("Loading " + files.Length.ToString() + " Files");
                        loadFilesIntoList(files);
                    } else {
                        System.Windows.Forms.MessageBox.Show("No Files Found!");
                        Functions.log("No Files Found");
                    }
                }
            }
        }

        #region FileOpening

        private void loadFilesIntoList( string[] files)
        {
            foreach( string filename in files)
            {
                Functions.decompressSave(filename);
                /*
                Functions.confs = new Stfs(filename);
                if (Functions.confs.HeaderData.SignatureHeaderType != SignatureType.Con ||
                    !Functions.confs.HeaderData.TitleID.Contains("4D5307DE")
                )
                {
                    // If the file type is bad, or it's the wrong game type --
                    Functions.confs.Close();
                }
                else
                {
                    Functions.log("Starting Save Extract (" + filename + ")");
                    Functions.decompressSave( filename );
                }
                Functions.confs.Close();
                */
            }

        }

        #endregion FileOpening
    }
}
