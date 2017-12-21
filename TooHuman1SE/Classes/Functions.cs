using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Isolib.STFSPackage;
using TooHuman1SE.SEStructure;

namespace TooHuman1SE.SEFunctions
{
    public static class Functions
    {

        public static Stfs confs;
        public static byte[] _buffer;

        public static void log(string logMessage)
        {
            try
            {
                if (!Directory.Exists("log")) Directory.CreateDirectory("log");
                using (StreamWriter w = File.AppendText("log/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log"))
                {
                    w.WriteLine("{0}: {1}", DateTime.Now.ToLongTimeString(), logMessage);
                }
            }
            catch { }

            MainWindow._MessagesPage.RichLog.AppendText(logMessage + Environment.NewLine);

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
