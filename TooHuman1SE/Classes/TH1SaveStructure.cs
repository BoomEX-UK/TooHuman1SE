using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using Isolib.IOPackage;
using Isolib.STFSPackage;
using MessageBox = System.Windows.MessageBox;
using TooHuman1SE.SEFunctions;

namespace TooHuman1SE.SEStructure
{
    public class TH1SaveStructure
    {

        #region Constants

        // Sector Constants
        private const int TH1_SECTOR_HASH = 0; // Contains file header and hash
        private const int TH1_SECTOR_PLAYER = 1;
        private const int TH1_SECTOR_UNKOWN01 = 2; // Mostly Null?
        private const int TH1_SECTOR_LOCATION = 3; // Area & Co-Ordinates?
        private const int TH1_SECTOR_QUEST = 4; // Quest - 2x Only, NOID = No Rune
        private const int TH1_SECTOR_RUNE = 5; // Runes?
        private const int TH1_SECTOR_UNKOWN02 = 6; // 0x0000 (or 0x002 Quest?)
        private const int TH1_SECTOR_UNKOWN03 = 7; // 
        private const int TH1_SECTOR_UNKOWN04 = 8; // 
        private const int TH1_SECTOR_UNKOWN05 = 9; // 
        private const int TH1_SECTOR_UNKOWN06 = 10; // 
        private const int TH1_SECTOR_UNKOWN07 = 11; // 
        private const int TH1_SECTOR_UNKOWN08 = 12; // 
        private const int TH1_SECTOR_UNKOWN09 = 13; // 

        // Save Types
        private const int TH1_SAVETYPE_NONE = 00;
        private const int TH1_SAVETYPE_CON = 01;
        private const int TH1_SAVETYPE_RAW = 02;

        // Fixed Offsets (well, ya body's no good without a skeleton!)
        private const long TH1_OFFSET_HASH = 0x10;
        private const long TH1_OFFSET_SIZE = 0x24;
        private const long TH1_OFFSET_MINSIZE = 0x0798;

        // Moi
        private static readonly byte[] PUBLIC_FOOTER = new byte[]{
            0x43, 0x72, 0x65, 0x61, 0x74, 0x65, 0x64, 0x20,
            0x42, 0x79, 0x20, 0x4A, 0x61, 0x6D, 0x65, 0x73
        };

        //  Bauldur's Secret
        private static readonly byte[] PRIVATE_KEY = new byte[]
        {
            0x05, 0x73, 0x30, 0xD3, 0xED, 0x76, 0x6C, 0x7E,
            0x93, 0x83, 0x0F, 0x50, 0x60, 0xAF, 0xB6, 0x78
        };

        // Valid Save Header
        private static readonly byte[] PUBLIC_HEADER = new byte[]{
            0x12, 0x34, 0x56, 0x78, 0x00, 0x00, 0x00, 0x30,
            0x54, 0x48, 0x31, 0x00, 0x00, 0x00, 0x00, 0x42
        };

        //  Is this the chicken or the egg?
        private static readonly byte[] PLACEHOLDER_HASH = new byte[]
        {
            0x2A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        #endregion Constants

        #region Variables

        // General Variables
        public string filePath;
        public int saveType;
        public byte[] rawData;
        public Boolean saveLoaded = false;
        public byte[] hash;
        public long dataSize;

        // Raw Data Variables
        public long sectorCount;
        public List<UInt32> sectorCode;
        public List<UInt32> sectorSize;
        public List<byte[]> sectorData;

        // Gamesave Data Loading
        public long runeCount;
        public List<string> runeNames;
        public List<List<UInt32>> runeValues;

        #endregion Variables

        #region Init (Reset)
        private void Init()
        {
            filePath = "";
            this.saveType = TH1_SAVETYPE_NONE;
            this.rawData = null;
            this.saveLoaded = false;
            this.dataSize = 0;

            this.sectorCount = 0;
            this.sectorCode = new List<UInt32>();
            this.sectorSize = new List<UInt32>();
            this.sectorData = new List<byte[]>();

            this.runeCount = 0;
            this.runeNames = new List<string>();
            this.runeValues = new List<List<UInt32>>();
        }

        #endregion Init (Reset)

        #region Public

        public void readSaveFile( string filePath )
        {
            // Try The File
            Init();
            this.filePath = filePath;
            Functions.log("File In [" + Path.GetFileNameWithoutExtension(this.filePath) + "]");
            readTH1GameSave();

            // If Sucessful
            if (this.saveLoaded)
            {
                Functions.log("Verification Passed", Functions.LC_SUCCESS);
                this.sectorCount = this.sectorCode.Count;
                foreach (byte[] barr in this.sectorData)
                {
                    this.dataSize += barr.Length + 8;
                }

                // Data Parsing
                // loadRunes();
            } else Functions.log("Verification Failed", Functions.LC_CRITICAL);

        }

        public void writeAllSectors(bool incHeader)
        {
            Directory.CreateDirectory("tmp");
            string fname = Path.GetFileNameWithoutExtension(this.filePath);
            for (int i = 0; i < this.sectorCount; i++)
            {
                writeSectorToFile("tmp\\" + fname + i.ToString() + ".tmp", i, incHeader);                    
            }
        }

        #endregion Public

        #region IO Functions

        private void readTH1GameSave()
        {
            if (File.Exists(this.filePath))
            {
                // Detect File format
                saveType = readSaveType(this.filePath);

                // Do Something With It
                switch( this.saveType)
                {
                    case TH1_SAVETYPE_CON:
                        loadConFromFile();
                        break;
                    case TH1_SAVETYPE_RAW:
                        loadRawFromFile();
                        break;
                    default:
                        MsgBox("Invalid Gamesave: " + this.filePath);
                        break;
                }

                // Do Some Further Basic Corruption Checks
                verifyRawData();

            }
            // return rawsave;
        }

        // Simple - RawData Loader
        private void loadRawFromFile()
        {
            Functions.log("Loading Raw File");
            RWStream reader = new RWStream(filePath, true);
            try
            {
                this.rawData = reader.ReadAllBytes();
            }
            catch (Exception ex) { MsgBox(ex.ToString()); }
            finally { reader.Close(false); }
        }

        // Simple (with the right library) - CON Loader
        private void loadConFromFile()
        {
            Stfs confs;
            Functions.log("Loading file from CONtainer");
            confs = new Stfs(this.filePath);
            if (confs.HeaderData.SignatureHeaderType != SignatureType.Con ||
            !confs.HeaderData.TitleID.Contains("4D5307DE"))
            {
                confs.Close();
            } else
            {
                this.rawData = confs.Extract(0);
            }
            confs.Close();
        }

        private int readSaveType( string filepath)
        {
            int tmpres = TH1_SAVETYPE_NONE;
            int peekSize = 4;
            byte[] bytesbuff;
            var byteCon = new byte[] { 0x43, 0x4F, 0x4E, 0x20 };
            var byteRaw = new byte[peekSize];

            Functions.log("Distinguishing Save Type");
            Array.Copy(PUBLIC_HEADER, byteRaw, peekSize);
            RWStream reader = new RWStream(filePath, true);
            try
            {
                bytesbuff = reader.PeekBytes(peekSize);
                if( bytesbuff.SequenceEqual(byteCon)) tmpres = TH1_SAVETYPE_CON;
                if( bytesbuff.SequenceEqual(byteRaw)) tmpres = TH1_SAVETYPE_RAW;
            }
            catch (Exception ex) { MsgBox(ex.ToString()); }
            finally { reader.Close(false); }

            return tmpres;
        }

        private byte[] abra(byte[] header)
        {
            // Abra used Teleport
            var tmpres = new byte[header.Length];
            byte[] output;

            Array.Copy(header, tmpres, header.Length);
            for (int i = 0; i < tmpres.Length; i++ ){
                tmpres[i] ^= PRIVATE_KEY[i];
                tmpres[i] ^= PUBLIC_FOOTER[i];
            }
            output = Encoding.ASCII.GetBytes(BitConverter.ToString(tmpres).Replace("-",""));
            return output;
        }

        private void writeSectorToFile( string filename, int sectorID, bool incHeader )
        {
            if (saveLoaded && (sectorID <= this.sectorCount) && (sectorID >= 0 ) )
            {
                RWStream writer = new RWStream(File.Open(filename, FileMode.Create), true);
                if (incHeader)
                {
                    writer.WriteUInt32(this.sectorCode[sectorID]);
                }
                writer.WriteBytes(this.sectorData[sectorID]);
                writer.Flush();
                writer.Close(false);
            }
        }

        #endregion IO Functions

        #region General Functions

        private void verifyRawData()
        {
            bool tmpres = this.rawData != null;
            int peekSize = 4;
            var shortHead = new byte[peekSize];
            var shortPeek = new byte[peekSize];
            byte[] newhash;

            if( tmpres )
            {
                RWStream readerW = new RWStream(this.rawData, true, true);
                try
                {
                    tmpres = readerW.Length >= TH1_OFFSET_SIZE; // - Check #1 (Minimum Length)

                    if (tmpres) // Check #2 (Size Matters)
                    {
                        // grab the true data size (no padding / overflow)
                        readerW.Position = TH1_OFFSET_SIZE;
                        this.dataSize = readerW.ReadInt32();
                        tmpres = readerW.Length >= this.dataSize;
                    }
                    else Functions.log("Fail - File Size Below " + TH1_OFFSET_SIZE.ToString() + "b", Functions.LC_CRITICAL);

                    if(tmpres) { // OK - enough checks, let's prime the data
                        // trim the fat
                        var swap = new byte[this.dataSize];
                        Array.Copy(this.rawData, swap, swap.Length);
                        this.rawData = swap;

                        // flush out the hash
                        readerW.Position = TH1_OFFSET_HASH;
                        this.hash = readerW.ReadBytes(PLACEHOLDER_HASH.Length);
                        readerW.Position = TH1_OFFSET_HASH;
                        readerW.WriteBytes(PLACEHOLDER_HASH);
                        readerW.Flush();
                    }
                    else Functions.log("Fail - Corrupt Gamesave Length", Functions.LC_CRITICAL);

                }
                catch (Exception ex) { MsgBox(ex.ToString()); }
                finally { this.rawData = readerW.ReadAllBytes(); readerW.Close(true); }
            }

            if (tmpres) // if we made it this far, lets hash it all up
            {
                newhash = getHash();
                if (!this.hash.SequenceEqual(newhash)) Functions.log("Warning: Original file contains invalid hash.", Functions.LC_WARNING); // FYI
            }

            this.saveLoaded = tmpres;
        }

        private void loadRunes()
        {
            byte[] thisSector = this.sectorData[TH1_SECTOR_RUNE];
            UInt32 runeNameLength;

            RWStream reader = new RWStream(thisSector, true);
            try
            {
                this.runeCount = reader.ReadUInt32();
                for (int i = 0; i < this.runeCount; i++)
                {

                }
            }
            catch (Exception ex) { MsgBox(ex.ToString()); }
            finally { reader.Close(false); }
        }

        private byte[] getHash()
        {
            // Prepare Buffers
            var tmpres = new byte[PLACEHOLDER_HASH.Length];
            var checkBuff = new byte[(PUBLIC_HEADER.Length*2) + this.dataSize];

            // Load
            Array.Copy(abra(PUBLIC_HEADER),0,checkBuff,0, (PUBLIC_HEADER.Length * 2));
            Array.Copy(this.rawData, 0, checkBuff, (PUBLIC_HEADER.Length * 2), this.dataSize);

            // Calculate
            SHA1 sha = new SHA1CryptoServiceProvider();
            try
            {
                tmpres = sha.ComputeHash(checkBuff);
            } finally{ sha.Clear(); };

            // Free
            Array.Clear(checkBuff, 0, checkBuff.Length);

            return tmpres;
        }

        private void MsgBox( string message)
        {
            MessageBox.Show(message);
            Functions.log("MsgBox(); " + message);
        }

        #endregion General Functions

    }
}
