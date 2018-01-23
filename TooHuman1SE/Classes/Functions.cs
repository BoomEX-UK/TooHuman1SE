using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Forms;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml.Serialization;
using System.Windows.Media.Imaging;
using Isolib.STFSPackage;
using TooHuman1SE.SEStructure;
using TooHuman1SE.Windows;
using System.Collections.ObjectModel;

namespace TooHuman1SE.SEFunctions
{
    public class CharList
    {
        public string path;
        public string name { get; set; }
        public string exp { get; set; }
        public long level { get; set; }
        public string hash;
        public string importedon { get; set; }
        public string cclass { get; set; }
        public string bounty { get; set; }
        public string calign { get; set; }
    }

    public class CharListImages
    {
        public string path;
        public string name { get; set; }
        public string exp { get; set; }
        public long level { get; set; }
        public string hash;
        public string importedon { get; set; }
        public string cclass { get; set; }
        public string bounty { get; set; }
        public string calign { get; set; }
        public BitmapImage image { get; set; }
    }

    public static class Functions
    {
        public const string appName = "Too Human 1 Save Editor";
        public const string creatorName = "Created By James";

        private const string charPath = "caracters.xml";

        public static Stfs confs;
        public static byte[] _buffer;

        public const int LC_DEFAULT = 0;
        public const int LC_CRITICAL = 1;
        public const int LC_SUCCESS = 2;
        public const int LC_WARNING = 3;
        public const int LC_PRIMARY = 4;

        #region Error Logging

        public static void log(string logMessage)
        {
            log(logMessage, LC_DEFAULT);
        }

        public static void log(string logMessage, int logColour)
        {
            // Used Controls
            System.Windows.Controls.RichTextBox rb = MainWindow._LogUC.RichLog;

            // Text Ranges
            TextRange trDateTime = new TextRange(rb.Document.ContentEnd, rb.Document.ContentEnd);
            TextRange trNotify = new TextRange(rb.Document.ContentEnd, rb.Document.ContentEnd);
            TextRange trMessage = new TextRange(rb.Document.ContentEnd, rb.Document.ContentEnd);

            // Colours
            SolidColorBrush notifyColour;

            // Strings
            string textDateTime = "";
            string textNotify = "";
            string textMessage = "";

            switch (logColour)
            {
                case LC_CRITICAL:
                    notifyColour = Brushes.PaleVioletRed;
                    textNotify = "[x]";
                    break;
                case LC_SUCCESS:
                    notifyColour = Brushes.LightGreen;
                    textNotify = "[+]";
                    break;
                case LC_WARNING:
                    notifyColour = Brushes.Yellow;
                    textNotify = "[!]";
                    break;
                case LC_PRIMARY:
                    notifyColour = Brushes.LightBlue;
                    textNotify = "[>]";
                    break;
                default:
                    notifyColour = Brushes.White;
                    break;
            }

            
            textDateTime = DateTime.Now.ToLongTimeString() + ": ";
            textMessage = " " + logMessage;

            // Try To Write Log
            try
            {
                if (!Directory.Exists("log")) Directory.CreateDirectory("log");
                using (StreamWriter w = File.AppendText("log/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log"))
                {
                    w.WriteLine(textDateTime + textNotify + textMessage);
                }
            }
            catch { }

            // Write To Rich Edit
            trDateTime.Text = textDateTime;
            trNotify.Text = textNotify;
            trMessage.Text = textMessage + Environment.NewLine;

            trNotify.ApplyPropertyValue(TextElement.BackgroundProperty, notifyColour);
            trMessage.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.White);

        }

        #endregion Error Logging

        #region Character Archive

        public static List<CharList> loadCharList()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<CharList>));
            List<CharList> list = new List<CharList>();
            try
            {
                using (FileStream stream = File.OpenRead(charPath))
                {
                    list = (List<CharList>)serializer.Deserialize(stream);
                }
            }
            catch
            {
                log("Unable to read " + charPath, LC_WARNING);
            }
            return list;
        }

        private static void saveCharListView( System.Windows.Controls.ListView lstv )
        {
            ObservableCollection<CharListImages> obsicol = (ObservableCollection<CharListImages>)lstv.ItemsSource;
            List<CharList> lstc = new List<CharList>();
            foreach(CharListImages cli in obsicol)
            {
                CharList chl = new CharList();
                chl.name = cli.name;
                chl.level = cli.level;
                chl.importedon = cli.importedon;
                chl.exp = cli.exp;
                chl.hash = cli.hash;
                chl.path = cli.path;
                chl.cclass = cli.cclass;
                chl.bounty = cli.bounty;
                chl.calign = cli.calign;
                lstc.Add(chl);
            }
            saveCharList(lstc);
        }

        public static void saveCharList( List<CharList> list)
        {   
            XmlSerializer serializer = new XmlSerializer(typeof(List<CharList>));
            try
            {
                FileStream stream = File.Create(charPath);
                try
                {
                    serializer.Serialize(stream, list);
                }
                catch { }
                finally { stream.Flush(); stream.Dispose(); }
            }
            catch
            {
                log("Unable to write to " + charPath, LC_CRITICAL);
            }
        }

        public static void initLoadCharList()
        {
            log("Refreshing Character List View");
            List<CharList> lst = loadCharList();
            List<CharListImages> lsti = new List<CharListImages>();
            TH1CharClassAlign ca = new TH1CharClassAlign();
            lst.Sort(delegate (CharList c1, CharList c2) { return c1.importedon.CompareTo(c2.importedon); });
            lst.Reverse();
            foreach(CharList cl in lst)
            {
                CharListImages il = new CharListImages();
                il.name = cl.name;
                il.level = cl.level;
                il.importedon = cl.importedon;
                il.exp = cl.exp;
                il.hash = cl.hash;
                il.path = cl.path;
                il.cclass = cl.cclass;
                il.bounty = cl.bounty;
                il.image = classToImage(Array.IndexOf(ca.classNames,cl.cclass));
                il.calign = cl.calign;
                lsti.Add(il);
            }

            ObservableCollection<CharListImages> obsicol = new ObservableCollection<CharListImages>(lsti);
            obsicol.CollectionChanged += lstCharacter_OnChange;
            MainWindow._CharactersUC.lstCharacters.ItemsSource = obsicol;
        }

        private static void lstCharacter_OnChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            saveCharListView(MainWindow._CharactersUC.lstCharacters);
        }

        public static void importCharSave( TH1SaveStructure save)
        {
            bool alreadylisted = false;
            Directory.CreateDirectory("saves");

            if (save.lastError == 0)
            {
                // Generate list
                ObservableCollection<CharListImages> list = (ObservableCollection<CharListImages>)MainWindow._CharactersUC.lstCharacters.ItemsSource;
                CharListImages item = new CharListImages();
                TH1CharClassAlign ca = new TH1CharClassAlign();

                // Generate item
                item.hash = BitConverter.ToString(save.hash).Replace("-","");
                item.name = save.character.name;
                item.exp = save.character.exp.ToString("N0");
                item.level = save.character.level;
                item.importedon = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                item.bounty = save.character.bounty.ToString("N0");
                item.cclass = ca.classNames[save.character.charClass];
                item.calign = ca.alignmentNames[save.character.alignment];
                item.image = classToImage(save.character.charClass);

                // Backup save
                item.path = @"saves\" + item.hash + ".th1";
                save.writeSaveFile(item.path);

                // Check if already in the list
                for( int i = 0; i < list.Count; i++) { 
                    if( list[i].hash == item.hash ) {
                        alreadylisted = true;
                        list[i] = item;
                    }
                }

                // Save list
                if ( !alreadylisted ){ list.Insert(0,item); }
            }

        }

        private static BitmapImage classToImage( long cclass)
        {
            BitmapImage tmpres = new BitmapImage();

            switch (cclass)
            {
                case 0: // Berserker
                    tmpres = new BitmapImage(new Uri("pack://application:,,,/ClassImages/berserker.png"));
                    break;
                case 1: // Champion
                    tmpres = new BitmapImage(new Uri("pack://application:,,,/ClassImages/champion.png"));
                    break;
                case 2: // Defender
                    tmpres = new BitmapImage(new Uri("pack://application:,,,/ClassImages/defender.png"));
                    break;
                case 5: // Commando
                    tmpres = new BitmapImage(new Uri("pack://application:,,,/ClassImages/commando.png"));
                break;
                case 6: // Bio-Engineer
                    tmpres = new BitmapImage(new Uri("pack://application:,,,/ClassImages/bio-engineer.png"));
                break;
                default:
                    tmpres = new BitmapImage(new Uri("pack://application:,,,/ClassImages/default.png"));
                break;
            }

            return tmpres;
        }

        public static void loadIntoEditorWindow( string savepath)
        {
            TH1SaveStructure loadingSave = new TH1SaveStructure();
            log("Loading Into Editor \"" + savepath + "\"", LC_PRIMARY);

            loadingSave.readSaveFile(savepath);

            if (loadingSave.lastError == 0)
            {
                EditorWindow eWin = new EditorWindow();
                eWin._save = loadingSave;
                eWin.Show();
                log("File Loaded Successfully", LC_SUCCESS);
            } else
            {
                MessageBox.Show("Error " + loadingSave.lastError.ToString() + ": " + loadingSave.lastErrorMsg);
                log("Error " + loadingSave.lastError.ToString() + ": " + loadingSave.lastErrorMsg);
            }
        }

        #endregion Character Archive

        }
}
