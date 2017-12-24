using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Windows.Documents;
using System.Windows.Media;
using Isolib.STFSPackage;
using TooHuman1SE.SEStructure;

namespace TooHuman1SE.SEFunctions
{
    public static class Functions
    {

        public static Stfs confs;
        public static byte[] _buffer;

        public const int LC_DEFAULT = 0;
        public const int LC_CRITICAL = 1;
        public const int LC_SUCCESS = 2;
        public const int LC_WARNING = 3;
        public const int LC_PRIMARY = 4;

        public static void log(string logMessage)
        {
            log(logMessage, LC_DEFAULT);
        }

        public static void log(string logMessage, int logColour)
        {
            System.Windows.Controls.RichTextBox rb = MainWindow._MessagesPage.RichLog;
            TextRange trf = new TextRange(rb.Document.ContentEnd, rb.Document.ContentEnd);
            TextRange trh = new TextRange(rb.Document.ContentEnd, rb.Document.ContentEnd);
            SolidColorBrush col;

            string tPrefix = "";
            string fullMessage = "";
            string highlightMessage = "";

            switch (logColour)
            {
                case LC_CRITICAL:
                    col = Brushes.PaleVioletRed;
                    tPrefix = "[x] ";
                    break;
                case LC_SUCCESS:
                    col = Brushes.LightGreen;
                    tPrefix = "[+] ";
                    break;
                case LC_WARNING:
                    col = Brushes.Yellow;
                    tPrefix = "[!] ";
                    break;
                case LC_PRIMARY:
                    col = Brushes.LightBlue;
                    tPrefix = "[>] ";
                    break;
                default:
                    col = Brushes.White;
                    break;
            }

            highlightMessage = tPrefix + logMessage + Environment.NewLine;
            fullMessage = DateTime.Now.ToLongTimeString() + ": ";

            try
            {
                if (!Directory.Exists("log")) Directory.CreateDirectory("log");
                using (StreamWriter w = File.AppendText("log/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log"))
                {
                    w.WriteLine(fullMessage + highlightMessage);
                }
            }
            catch { }

            trf.Text = fullMessage;
            trh.Text = highlightMessage;
            trh.ApplyPropertyValue(TextElement.BackgroundProperty, col);

        }

        public static void decompressSave(string filename)
        {
            string _savepath = @"saves\";
            string shortfilename = Path.GetFileNameWithoutExtension(filename);
            string _rawpath = _savepath + shortfilename + "-raw";
            byte[] crc;

            TH1SaveStructure newsave = new TH1SaveStructure();
            newsave.readSaveFile(filename);


            /*
            List<string> fileHeaders = new List<string>();
            Directory.CreateDirectory(_savepath);
            log("Extracting - " + confs.EmbeddedFiles[0].Name);
            _buffer = confs.Extract(0);
            if (_buffer != null)
            {
                // crc = AvalonIO.Security.XeAlgo.Md5(_buffer);
                File.WriteAllBytes(_rawpath, _buffer);

                newsave.readSaveFile(_rawpath);

                if (newsave.saveLoaded)
                {
                    log("Saved Extracted (" + newsave.sectorCount.ToString() + " sectors, " + newsave.dataSize.ToString() + " bytes)");
                }
                else { log("Save Failed To Load"); }
                
                // Debug Only
                newsave.writeAllSectors(false);

            }
            else log("Unable To Extract Save.");
            */

        }

    }
}
