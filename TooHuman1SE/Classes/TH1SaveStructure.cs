using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using Isolib.IOPackage;
using Isolib.STFSPackage;
using MessageBox = System.Windows.MessageBox;

namespace TooHuman1SE.SEStructure
{

    /* Error Codes
         -1  : Initial (No Save Loaded)
          0  : OK (No Error)
          1  : No Save Data Was Loaded
          2  : File Size Is Below Minimum
          3  : Data Size value is larger than the actual file-size
          4  : IO Error Caught {exception appended}
          5  : Failed to load Gamesave sectors
          6  : Save Type Not Recognised
          7  : Unable To Write Gamesave
          8  : Unable To Write New Hash
          9  : Failed To Write Character Name To Buffer
         10  : Unable To Write Character Stats To Save Data
         11  : Unable To Write New Filesize
         12  : Unable To Parse Skills Tree
         13  : Unable To Write Skills Tree
         14  : Unable To Parse Runes
         15  : Unable To Write Runes
    */

    public class TH1Sector
    {
        public long id = 0;
        public long offset = 0;
        public long size = 0;
    }

    public class TH1Character
    {
        // Offsets
        public long OFFSET_HEADER = 0;
        public long OFFSET_SAVESLOT = 4;
        public long OFFSET_NAME_U = 20;
        public long OFFSET_CLASS = 52;
        public long OFFSET_ALIGNMENT = 0; // unknown
        public long OFFSET_LEVELA = 56;
        public long OFFSET_LEVELB = 1748;
        public long OFFSET_SKILLPOINTS = 128;
        public long OFFSET_EXP = 1756;
        public long OFFSET_BOUNTY = 1764;
        public long OFFSET_NAME_A_LENGTH = 1800;
        public long OFFSET_NAME_A = 1804;
        public long OFFSET_CURR_LEVEL_EXP = 1752;
        public long OFFSET_DATA_PAIRSA = 956;
        public long OFFSET_DATA_PAIRSB = 1652;
        public long OFFSET_ENEMIES_KILLED = 124;

        // Limits
        public int LIMIT_NAME_LENGTH = 15;
        public int LIMIT_DATA_PAIRSA = 87;
        public int LIMIT_DATA_PARISB = 7;

        // Variables
        public string name;
        public long alignment;
        public long charClass;
        public long level;
        public long exp;
        public long bounty;
        public long skillPoints;
        public string playTime;
        public long saveSlot;

        // Data Pairs
        public Dictionary<string, uint> dataPairsA = new Dictionary<string, uint>();
        public Dictionary<string, uint> dataPairsB = new Dictionary<string, uint>();

        // Data Pair Names
        public Dictionary<uint, string> dataPairNamesA = new Dictionary<uint, string> {
            [0x01] = "goblins_killed",
            [0x02] = "trolls_killed",
            [0x03] = "dark_elves_killed",
            [0x04] = "undead_killed",
            [0x10] = "air_kills",
            [0x11] = "ruiner_kills",
            [0x2D] = "wells_activated",
            [0x2E] = "rifle_shots_fired", // Primary or Secondary
            [0x52] = "rune_pickups",
            [0x5B] = "hall_of_heros_01",
            [0x5C] = "hall_of_heros_02",
            [0x5D] = "hall_of_heros_03",
            [0x5E] = "hall_of_heros_spare",
            [0x5F] = "ice_forest_01",
            [0x60] = "ice_forest_02",
            [0x61] = "ice_forest_03",
            [0x62] = "ice_forest_04",
            [0x63] = "world_serpent_01",
            [0x64] = "world_serpent_02",
            [0x65] = "world_serpent_03",
            [0x66] = "world_serpent_04",
            [0x67] = "world_serpent_05",
            [0x68] = "helheim_01",
            [0x69] = "helheim_02",
            [0x6A] = "helheim_03",
            [0x6B] = "helheim_04",
            [0x6C] = "helheim_05",
            [0x7A] = "highest_combo",
            [0x7B] = "item_pickups"
        };

        public Dictionary<uint, string> dataPairNamesB = new Dictionary<uint, string>
        {

        };

    }

    // 2x Value For Skills Tree
    public class TH1SkillsTreePair
    {
        public long first = 0;
        public long second = 0;
    }

    public class TH1SkillsTree
    {
        // Constants
        private long ST_VALUES_DEFAULT = 65;
        private long ST_VALUES_CHAMPION = 67;

        // Limits
        public long LIMIT_SKILL_MAX = 99999;

        // Variables
        private long cClass = 0;

        // Skill Names
        private string[] NAMES_0 = new string[] { // Berserker
            "Spiritual Runier",
            "A Capacity for Rage",
            "The Bear's Bolling Blood",
            "Onslaught of Claws",
            "Brutality",
            "Loki's Kiss",
            "Ankle Biter",
            "Sleep-Storm of Steel",
            "Swift of Claw",
            "Engulfing Rage",
            "Shield Biter",
            "Unrelenting Blades",
            "Weapon Recovery",
            "Warrior of the Twinned-Claw",
            "Spirit of Fenrir"
        };
        private string[] NAMES_1 = new string[] { // Champion
            "Raven Call",
            "Unerring Strike",
            "Immolating Blade",
            "Asgard's Fury",
            "Kinship of Gungnir",
            "Lament for the Battle-Slain",
            "Thermal Induction Mine",
            "Feeder of Ravens",
            "Tree of Raining-Iron",
            "One Will Rise Above",
            "Valiant's Might",
            "Storm of Mortal Wounds",
            "Warrior of the Blood-Eel",
            "Ascent to Valhalla",
            "Stopping Power",
            "Spirit of Fenrir"
        };
        private string[] NAMES_2 = new string[] { // Defender
            "Valiant's Unstable Hand",
            "Defender’s Resilience",
            "Enthalpy Reduction Attack",
            "Grim Resolve",
            "The Berserker's Grief",
            "Enthalpy Reduction Mines",
            "Ward of the NORNs",
            "Tree of Scorching Light",
            "Fimbulwinter's Numbing Touch",
            "Reversal of Wyrds",
            "Egil's Blessing",
            "Adept of the Light-Spear",
            "Tyr's Best Work",
            "Warrior of the Iron Fist",
            "Spirit of Fenrir "
        };
        private string[] NAMES_5 = new string[] { // Commando
            "Wrecker of Mead Halls",
            "Pinning Shot",
            "Rain of Iron",
            "Adept of the Burning Spear",
            "Bullet-Tree",
            "Cluster Munitions",
            "Tree of Shrieking-Flame",
            "Smoothbore",
            "Lightning Cascade",
            "Cut to the Bone",
            "Ballistic Telemetry Feedback",
            "Delayed Fragmentation Warheads",
            "Gift of Gungnir",
            "Spirit of Fenrir",
            "Helm Reddener"
        };
        private string[] NAMES_6 = new string[] { // Bio-Engineer
            "Valkyrie's Blessing",
            "Idunn's Touch",
            "Skuld's Embrace",
            "Warrior of the Battle-Oar",
            "Warrior of Tyr's Way",
            "Wrack of Lightning Mine",
            "Ward of the NORNs",
            "Gifts of Idunn",
            "Idunn's Boon",
            "Idunn's Favor",
            "Idunn's Wish",
            "Ascent to Valhalla",
            "Cellular Rebonding",
            "Electrified Blade",
            "Spirit of Fenrir"
        };

        // Skills Tree Values
        public List<TH1SkillsTreePair> pairs = new List<TH1SkillsTreePair>();

        public TH1SkillsTree( long setClass )
        {
            cClass = setClass;
        }

        public string[] getSkillNames()
        {
            string[] tmpout = new string[15];
            switch (cClass)
            {
                case 0: tmpout = NAMES_0; break;
                case 1: tmpout = NAMES_1; break;
                case 2: tmpout = NAMES_2; break;
                case 5: tmpout = NAMES_5; break;
                case 6: tmpout = NAMES_6; break;
                default: for (int n = 0; n < tmpout.Length; n++) tmpout[n] = "Unknown"; break;
            }
            return tmpout;
        }

        public long getValueCount()
        {
            if (cClass == 1) return ST_VALUES_CHAMPION;
            else return ST_VALUES_DEFAULT;
        }
    }

    class TH1CharClassAlign
    {
        public string[] classNames = { "Berserker", "Champion", "Defender", "Heavy Gunner", "Gunslinger", "Commando", "Bio-Engineer", "Dragon", "Rune Master" };
        public string[] alignmentNames = { "None", "Human", "Cybernitics" };
    }

    class TH1ExpToNextLevel
    {
        public long[] expUp = {
            450, 690, 970, 1290, 1650,
            2050, 2710, 3670, 4710, 5805,
            7515, 9339, 11277, 13329, 15495,
            17775, 20169, 22677, 25229
        };
        public long[] baseEXP;

        public TH1ExpToNextLevel()
        {
            baseEXP = new long[expUp.Length+1];
            for( int baseoffset = 1; baseoffset < expUp.Length+1; baseoffset++)
            {
                baseEXP[baseoffset] = baseEXP[baseoffset - 1] + expUp[baseoffset - 1];
            }
        }

        public int calcLevel( long exp)
        {
            int tmpres = 0;
            while ((tmpres < baseEXP.Length) && (exp >= baseEXP[tmpres])) tmpres++;
            return tmpres;
        }

        public long expToNext(long exp)
        {
            long tmpres = 0;
            if (exp > 0)
            {
                int lvl = calcLevel(exp);
                if (lvl < baseEXP.Length) tmpres = expUp[lvl - 1];
            }
            return tmpres;
        }

        public long expProgressToNext(long exp)
        {
            long tmpres = 0;
            int lvl = calcLevel(exp);
            long toNext = expToNext(exp);
            if (toNext > 0) tmpres = expUp[lvl-1]-(baseEXP[lvl]-exp);
            return tmpres;
        }

    }

    public class TH1Rune
    {
        //- Private
        private string[] colourNames = new string[] { "Unknown", "Grey", "Green", "Blue", "Purple", "Orange", "Red" };
        private Double[] runeValues = new Double[] { 0, 0.0025, 0.0125, 0.02, 0, 0, 25 };
        private bool _purchased = false;
        private char _runeMid = 'M'; // default
        //- Public
        public uint purchased {
            get
            {
                return (uint)(_purchased ? 1 : 0);
            }
            set
            {
                _purchased = (value == 1);
            }
         }
        public uint baseValue { get; set; }
        public uint dataB { get; set; }
        public uint dataD { get; set; }
        public uint paintID { get; set; }
        public int runeColour { get; set; }
        public int runeID { get; set; }
        public char runeMid {
            get
            {
                return _runeMid;
            }

            set
            {
                _runeMid = value;
            }
        }
        public string[] getColourNames
        {
            get
            {
                string[] tmpstring = new string[colourNames.Length - 1];
                for( int i=1; i<colourNames.Length; i++)
                {
                    tmpstring[i - 1] = colourNames[i];
                }
                return tmpstring;
            }
        }
        public int LIMIT_MAX_RUNES
        {
            get
            {
                return 60;
            }
        }
        public string runeColourName {
            get
            {
                return colourNames[runeColour];
            }
        }
        public void nameToRune(byte[] name )
        {
            string tmpname = "";
            for( int nameloop = 0; nameloop < name.Length-1; nameloop++)
            {
                tmpname += (char)name[nameloop];
            }
            string[] parts = tmpname.Split('_');
            switch( parts[0])
            {
                case "G":
                    runeColour = 1;
                    break;
                case "E":
                    runeColour = 2;
                    break;
                case "B":
                    runeColour = 3;
                    break;
                case "P":
                    runeColour = 4;
                    break;
                case "O":
                    runeColour = 5;
                    break;
                case "R":
                    runeColour = 6;
                    break;
            }
            runeMid = parts[1][0];
            runeID = int.Parse(parts[2]);
        }
        public byte[] runeToName
        {
            get
            {
                string tmpString = "";
                switch (runeColour)
                {
                    case 2:
                        tmpString += "E";
                        break;
                    case 3:
                        tmpString += "B";
                        break;
                    case 4:
                        tmpString += "P";
                        break;
                    case 5:
                        tmpString += "O";
                        break;
                    case 6:
                        tmpString += "R";
                        break;
                    default:
                    case 1:
                        tmpString += "G";
                        break;
                }
                tmpString += "_" + runeMid + "_" + runeID.ToString();
                byte[] tmpByte = new byte[tmpString.Length + 1];
                Array.Copy(Encoding.ASCII.GetBytes(tmpString), tmpByte, tmpString.Length);
                return tmpByte;
            }
        }
        public int getByteSize
        {
            get
            {
                return 4 + runeToName.Length + (5*4);
            }
        }
        public long getValue
        {
            get
            {
                return Convert.ToUInt32(baseValue * runeValues[runeColour]);
            }
        }
        public byte[] runeToArray
        {
            get
            {
                byte[] tmpRune = new byte[getByteSize];
                RWStream writer = new RWStream(tmpRune, true, true);
                try
                {
                    byte[] tmpName = runeToName;
                    writer.WriteUInt32((uint)tmpName.Length);
                    writer.WriteBytes(tmpName);
                    writer.WriteUInt32(purchased);
                    writer.WriteUInt32(dataB);
                    writer.WriteUInt32(baseValue);
                    writer.WriteUInt32(dataD);
                    writer.WriteUInt32(paintID);
                } catch { }
                finally { writer.Flush(); tmpRune = writer.ReadAllBytes(); writer.Close(true); }
                return tmpRune;
            }
        }
        public void setColourByName( string _name)
        {
            for( int i = 0; i < colourNames.Length; i++)
            {
                if (_name == colourNames[i])
                {
                    runeColour = i;
                }
            }
        }
    }

    public class TH1SaveStructure
    {

        #region Constants

        // Sector Constants
        public const int TH1_SECTOR_HEADER = 0; // Header & Hash
        public const int TH1_SECTOR_CHARACTER = 1; // Character Data
        public const int TH1_SECTOR_SKILLTREE = 2; // Skill/Class Tree?
        public const int TH1_SECTOR_LOCATION = 3; // Area & Co-Ordinates?
        public const int TH1_SECTOR_QUEST01 = 4; // Active Charms? - 2x Only, NOID = No Rune
        public const int TH1_SECTOR_RUNE = 5; // Runes?
        public const int TH1_SECTOR_QUEST02 = 6; // Charms Store? -- uint #of quests, string size, name, uint 0x00, 0x123456, data length 0x4C, back to string size 
        public const int TH1_SECTOR_WEAPONS = 7; // Weapons Begin? -- uint #of weapons, string size, name, uint 0x00, 0x123456, data length 0x14, back to string size
        public const int TH1_SECTOR_UNKNOWN02 = 8; // Weapons Begin Here

        // Save Types
        private const int TH1_SAVETYPE_NONE = 00;
        private const int TH1_SAVETYPE_CON = 01;
        private const int TH1_SAVETYPE_RAW = 02;

        // Fixed Offsets (well, ya body's no good without a skeleton!)
        private const long TH1_OFFSET_HASH = 0x10;
        private const long TH1_OFFSET_SIZE = 0x24;

        // Limits (for control)
        private const long TH1_LIMIT_MINSIZE = 0x0798;

        // Moi
        private static readonly byte[] PUBLIC_FOOTER = new byte[]{
            0x78, 0x4A, 0x61, 0x6D, 0x2E, 0x65, 0x73, 0x2F,
            0x54, 0x6F, 0x6F, 0x48, 0x75, 0x6D, 0x61, 0x6E,
            0x20, 0x2D, 0x20, 0x54, 0x6F, 0x6F, 0x20, 0x48,
            0x75, 0x6D, 0x61, 0x6E, 0x20, 0x31, 0x20, 0x53,
            0x61, 0x76, 0x65, 0x20, 0x45, 0x64, 0x69, 0x74,
            0x6F, 0x72
        };

        //  Bauldur's Secret
        private static readonly byte[] PRIVATE_KEY = new byte[]
        {
            0x3E, 0x4B, 0x34, 0xDF, 0xB7, 0x76, 0x7B, 0x71,
            0x85, 0x95, 0x40, 0x52, 0x74, 0xAF, 0xB2, 0x65
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
        public byte[] hash;
        public Boolean hashVerified;
        public long dataSize;

        // Gamesave Parsing
        public List<TH1Sector> sectors;
        public TH1Character character;
        public List<TH1Rune> runes;
        public TH1SkillsTree skills;

        // Problems
        // public Boolean saveLoaded = false;
        public long lastError;       // current 0
        public string lastErrorMsg;

        #endregion Variables

        #region Init (Reset)
        private void Init()
        {
            // General
            this.filePath = "";
            this.saveType = TH1_SAVETYPE_NONE;
            this.rawData = null;
            this.hash = PLACEHOLDER_HASH;
            this.hashVerified = false;
            this.dataSize = 0;

            // Parsed
            this.sectors = new List<TH1Sector>();
            this.character = new TH1Character();
            this.runes = new List<TH1Rune>();
            this.skills = new TH1SkillsTree(0);

            // Checks
            setError(-1, "Save Not Loaded.");
        }

        #endregion Init (Reset)

        #region Public

        public void readSaveFile(string filePath)
        {
            // Try The File
            Init();
            this.filePath = filePath;
            readTH1Gamesave();

            // If Sucessful
            if (this.lastError == 0)
            {
                loadGamesaveSectors();
                dataToCharacter();
                dataToSkillsTree();
                dataToRunes();

                this.dataSize = this.rawData.Length;
            }

        }

        public void writeSaveFile(string outFilePath)
        {
            byte[] newhash;
            byte[] saveOut;

            // Save Settings
            characterToData();
            skillsTreeToData();
            runesToData();

            // Create the Save Buffer in Memory
            saveOut = new byte[this.dataSize];
            Array.Copy(this.rawData, 0, saveOut, 0, saveOut.Length);
            Array.Resize(ref saveOut, (int)(this.dataSize + PUBLIC_FOOTER.Length));
            Array.Copy(PUBLIC_FOOTER, 0, saveOut, this.dataSize, PUBLIC_FOOTER.Length);

            // Generate Hash
            newhash = getHash(saveOut);

            // Overwrite the Placeholder Hash
            RWStream writer = new RWStream(saveOut, true, true);
            try
            {
                writer.Position = TH1_OFFSET_HASH;
                writer.WriteBytes(newhash);
                writer.Flush();
            }
            catch (Exception ex) {
                setError(8, "Unable To Write New Hash: " + ex.ToString());
            }
            finally { saveOut = writer.ReadAllBytes(); writer.Close(true); }

            try
            {
                using (var fs = new FileStream(outFilePath, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(saveOut, 0, saveOut.Length);
                }
            }
            catch (Exception ex)
            {
                setError(7, "Unable To Write Gamesave: " + ex.ToString());
            }

        }

        // Drop Them all
        public void writeAllSectors(bool incHeader)
        {
            string tmphash = BitConverter.ToString(this.hash).Replace("-", "");
            Directory.CreateDirectory("tmp");
            Directory.CreateDirectory(@"tmp\" + tmphash);
            for (int i = 0; i < this.sectors.Count; i++)
            {
                writeSectorToFile(@"tmp\" + tmphash + @"\sector" + i.ToString("000") + ".tmp", i, incHeader);
            }
        }

        #endregion Public

        #region IO Functions

        private void readTH1Gamesave()
        {
            if (File.Exists(this.filePath))
            {
                // Detect File format
                saveType = readSaveType(this.filePath);

                // Do Something With It
                switch (this.saveType)
                {
                    case TH1_SAVETYPE_CON:
                        loadConFromFile();
                        break;
                    case TH1_SAVETYPE_RAW:
                        loadRawFromFile();
                        break;
                    default:
                        setError(6, "Save Type Not Recognised");
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

        private int readSaveType(string filepath)
        {
            int tmpres = TH1_SAVETYPE_NONE;
            int peekSize = 4;
            byte[] bytesbuff;
            var byteCon = new byte[] { 0x43, 0x4F, 0x4E, 0x20 };
            var byteRaw = new byte[peekSize];

            Array.Copy(PUBLIC_HEADER, byteRaw, peekSize);
            try
            {
                RWStream reader = new RWStream(filePath, true);
                try
                {
                    bytesbuff = reader.PeekBytes(peekSize);
                    if (bytesbuff.SequenceEqual(byteCon)) tmpres = TH1_SAVETYPE_CON;
                    if (bytesbuff.SequenceEqual(byteRaw)) tmpres = TH1_SAVETYPE_RAW;
                }
                catch (Exception ex) { MsgBox(ex.ToString()); }
                finally { reader.Close(false); }
            }
            catch { }

            return tmpres;
        }

        private byte[] abra(byte[] header)
        {
            // Abra used Teleport
            var tmpres = new byte[header.Length];
            byte[] output;

            Array.Copy(header, tmpres, header.Length);
            for (int i = 0; i < tmpres.Length; i++) {
                tmpres[i] ^= PRIVATE_KEY[i];
                tmpres[i] ^= PUBLIC_FOOTER[i];
            }
            output = Encoding.ASCII.GetBytes(BitConverter.ToString(tmpres).Replace("-", ""));
            return output;
        }

        // Dumping Sectors..
        private void writeSectorToFile(string filename, int sectorID, bool incHeader)
        {
            // Sector Code
            byte[] sectorcode = new byte[4];
            Array.Copy(PUBLIC_HEADER, sectorcode, sectorcode.Length);

            // Write All Sectors
            if ((this.lastError == 0) && (sectorID <= this.sectors.Count) && (sectorID >= 0))
            {
                // Grab Data
                byte[] sectordata = new byte[this.sectors[sectorID].size];
                Array.Copy(this.rawData, this.sectors[sectorID].offset, sectordata, 0, sectordata.Length);

                // Write It
                RWStream writer = new RWStream(File.Open(filename, FileMode.Create), true);
                if (incHeader)
                {
                    writer.WriteBytes(sectorcode);
                }
                writer.WriteBytes(sectordata);
                writer.Flush();
                writer.Close(false);
            }
        }

        // For Those Constantly Changing Sectors ..
        private void replaceSector(int sectorID, byte[] newData)
        {

            byte[] reconstructed;
            long pos = 0;
            long relSize = 0;
            reconstructed = new byte[this.rawData.Length - this.sectors[sectorID].size + newData.Length];

            // Beginning
            Array.Copy(this.rawData, 0, reconstructed, pos, this.sectors[sectorID].offset);
            pos += this.sectors[sectorID].offset;

            // Middle
            Array.Copy(newData, 0, reconstructed, pos, newData.Length);
            pos += newData.Length;

            // End
            long secondHalf = this.sectors[sectorID].offset + this.sectors[sectorID].size;
            Array.Copy(this.rawData, secondHalf, reconstructed, pos, this.rawData.Length - secondHalf);

            // Shuffle Sector Offsets
            relSize = newData.Length - this.sectors[sectorID].size;
            this.sectors[sectorID].size = newData.Length;
            for (int sectorI = sectorID + 1; sectorI < this.sectors.Count; sectorI++)
            {
                this.sectors[sectorI].offset += relSize;
            }

            // Output
            this.rawData = reconstructed;
            this.dataSize = this.rawData.Length;
            rewriteFileSize();
        }

        private void rewriteFileSize()
        {
            long tmpOffset = TH1_OFFSET_SIZE;
            RWStream writer = new RWStream(this.rawData, true, true);
            try
            {
                writer.Position = tmpOffset;
                writer.WriteUInt32((uint)this.dataSize);
            }
            catch (Exception ex) { setError(11, "Unable To Write New Filesize: " + ex.ToString()); }
            finally { this.rawData = writer.ReadAllBytes(); writer.Close(true); }
        }

        #endregion IO Functions

        #region Character IO
        private void dataToCharacter()
        {
            // Supports
            // - Name
            // - Class
            // - Level
            // - Exp
            // - Bounty
            // - Skillpoints
            // - SaveSlot
            // - Data Pairs A
            // - Data Pairs B

            // temp load the character data
            byte[] charData = new byte[this.sectors[TH1_SECTOR_CHARACTER].size];
            Array.Copy(this.rawData, this.sectors[TH1_SECTOR_CHARACTER].offset, charData, 0, charData.Length);
            TH1Character tmpChar = this.character;

            RWStream reader = new RWStream(charData, true);
            try
            {

                // Character Name
                reader.Position = tmpChar.OFFSET_NAME_A_LENGTH;
                int namelength = (int)reader.PeekUInt32();
                reader.Position = tmpChar.OFFSET_NAME_A;
                tmpChar.name = reader.ReadString(StringType.Ascii, namelength - 1);

                //tmpChar.alignment;

                reader.Position = tmpChar.OFFSET_CLASS;
                tmpChar.charClass = reader.ReadUInt32();

                reader.Position = tmpChar.OFFSET_LEVELA;
                tmpChar.level = reader.ReadUInt32();

                reader.Position = tmpChar.OFFSET_EXP;
                tmpChar.exp = reader.ReadUInt32();

                reader.Position = tmpChar.OFFSET_BOUNTY;
                tmpChar.bounty = reader.ReadUInt32();

                reader.Position = tmpChar.OFFSET_SKILLPOINTS;
                tmpChar.skillPoints = reader.ReadUInt32();

                reader.Position = tmpChar.OFFSET_SAVESLOT;
                tmpChar.saveSlot = reader.ReadUInt32();

                reader.Position = tmpChar.OFFSET_DATA_PAIRSA;
                for (int i = 0; i < tmpChar.LIMIT_DATA_PAIRSA; i++)
                {
                    tmpChar.dataPairsA.Add(lookupDataPairName(reader.ReadUInt32(), 1), reader.ReadUInt32());
                }

                reader.Position = tmpChar.OFFSET_DATA_PAIRSB;
                for (int i = 0; i < tmpChar.LIMIT_DATA_PARISB; i++)
                {
                    tmpChar.dataPairsB.Add(lookupDataPairName(reader.ReadUInt32(), 2), reader.ReadUInt32());
                }

                //tmpChar.playtime;

            }
            catch (Exception ex) { MsgBox(ex.ToString()); }
            finally { reader.Close(false); this.character = tmpChar; }
        }

        private void characterToData()
        {

            // Supports
            // - Name (Unicode & ASCII)
            // - Class
            // - Level (x2 Offsets)
            // - Exp
            // - Bounty
            // - Skillpoints
            // - SaveSlot
            // - Data Pairs A
            // - Data Pairs B

            // temp load the character data
            byte[] charData = new byte[this.sectors[TH1_SECTOR_CHARACTER].size];
            byte[] charDataTemp = new byte[1];

            Array.Copy(this.rawData, this.sectors[TH1_SECTOR_CHARACTER].offset, charData, 0, charData.Length);
            TH1Character tmpChar = this.character;

            // No Overflows
            tmpChar.name = tmpChar.name.Substring(0, Math.Min(tmpChar.name.Length, tmpChar.LIMIT_NAME_LENGTH));

            RWStream namewriter = new RWStream(charData, true, true);
            try
            {
                // Character Name
                namewriter.Position = tmpChar.OFFSET_NAME_A_LENGTH;
                uint oldNameLength = namewriter.ReadUInt32() - 1;
                uint newNameLength = (uint)tmpChar.name.Length;

                namewriter.Position = tmpChar.OFFSET_NAME_A_LENGTH;
                namewriter.WriteUInt32(newNameLength + 1);

                charDataTemp = new byte[charData.Length - oldNameLength + newNameLength];

                // i think ?!
                Array.Copy(charData, charDataTemp, tmpChar.OFFSET_NAME_A);
                Array.Copy(Encoding.ASCII.GetBytes(tmpChar.name), 0, charDataTemp, tmpChar.OFFSET_NAME_A, tmpChar.name.Length);
                Array.Copy(charData, tmpChar.OFFSET_NAME_A + oldNameLength, charDataTemp, tmpChar.OFFSET_NAME_A + newNameLength, charData.Length - tmpChar.OFFSET_NAME_A - oldNameLength);
            }
            catch (Exception ex) { setError(9, "Failed To Write Character Name To Buffer: " + ex.ToString()); return; }

            charData = charDataTemp;

            RWStream writer = new RWStream(charData, true, true);
            try
            {
                TH1ExpToNextLevel _expCalc = new TH1ExpToNextLevel();

                // Write Unicode Name
                writer.Position = tmpChar.OFFSET_NAME_U;
                writer.WriteString(tmpChar.name, StringType.Unicode, tmpChar.name.Length);
                for (int uniz = tmpChar.name.Length; uniz < tmpChar.LIMIT_NAME_LENGTH; uniz++) writer.WriteBytes(new byte[] { 0x00, 0x00 });

                //tmpChar.alignment;

                // Write Class
                writer.Position = tmpChar.OFFSET_CLASS;
                writer.WriteUInt32((uint)tmpChar.charClass);

                // Write Level
                writer.Position = tmpChar.OFFSET_LEVELA;
                writer.WriteUInt32((uint)tmpChar.level);

                writer.Position = tmpChar.OFFSET_LEVELB;
                writer.WriteUInt32((uint)tmpChar.level);

                // Write EXP
                writer.Position = tmpChar.OFFSET_EXP;
                writer.WriteUInt32((uint)tmpChar.exp);

                writer.Position = tmpChar.OFFSET_CURR_LEVEL_EXP;
                writer.WriteUInt32((uint)_expCalc.expProgressToNext(tmpChar.exp));

                // Write Bounty
                writer.Position = tmpChar.OFFSET_BOUNTY;
                writer.WriteUInt32((uint)tmpChar.bounty);

                // Write Skillpoints
                writer.Position = tmpChar.OFFSET_SKILLPOINTS;
                writer.WriteUInt32((uint)tmpChar.skillPoints);

                // Write Save Slot
                writer.Position = tmpChar.OFFSET_SAVESLOT;
                writer.WriteUInt32((uint)tmpChar.saveSlot);

                writer.Position = tmpChar.OFFSET_DATA_PAIRSA;
                foreach (KeyValuePair<string, uint> dp in tmpChar.dataPairsA)
                {
                    writer.WriteUInt32(lookupDataPairValue(dp.Key, 1));
                    writer.WriteUInt32(dp.Value);
                }

                writer.Position = tmpChar.OFFSET_DATA_PAIRSB;
                foreach (KeyValuePair<string, uint> dp in tmpChar.dataPairsB)
                {
                    writer.WriteUInt32(lookupDataPairValue(dp.Key, 2));
                    writer.WriteUInt32(dp.Value);
                }

                //tmpChar.playtime;

            }
            catch (Exception ex) { setError(10, "Unable To Write Character Stats To Save Data: " + ex.ToString()); return; }
            finally { writer.Flush(); charData = writer.ReadAllBytes(); writer.Close(false); }

            replaceSector(TH1_SECTOR_CHARACTER, charData);

        }

        #endregion Character IO

        #region Skills Tree IO

        private void dataToSkillsTree()
        {
            // Reset With Default Class
            TH1SkillsTree tmpSkills = new TH1SkillsTree(0);

            // Buffer It
            byte[] skillsData = new byte[this.sectors[TH1_SECTOR_SKILLTREE].size];
            Array.Copy(this.rawData, this.sectors[TH1_SECTOR_SKILLTREE].offset, skillsData, 0, skillsData.Length);

            RWStream reader = new RWStream(skillsData, true);
            try
            {
                // Read In Character Class
                tmpSkills = new TH1SkillsTree(reader.ReadUInt32()); // New With Alignment

                while ( reader.Position < reader.Length)
                {
                    TH1SkillsTreePair stp = new TH1SkillsTreePair();
                    stp.first = reader.ReadUInt32();
                    stp.second = reader.ReadUInt32();
                    tmpSkills.pairs.Add(stp);
                }

            }
            catch (Exception ex) { setError( 12, "Unable To Parse Skills Tree: " + ex.ToString()); }
            finally { reader.Close(false); this.skills = tmpSkills; }
        }

        private void skillsTreeToData()
        {
            // Buffer It
            byte[] skillsData = new byte[(this.skills.pairs.Count*8)+4];

            RWStream writer = new RWStream(skillsData, true, true);
            try
            {
                writer.WriteUInt32((uint)this.character.charClass);

                for( int num=0; num < this.skills.pairs.Count; num++)
                {
                    writer.WriteUInt32((uint)this.skills.pairs[num].first);
                    writer.WriteUInt32((uint)this.skills.pairs[num].second);
                }
            }
            catch (Exception ex) { setError(13, "Unable To Write Skills Tree: " + ex.ToString()); }
            finally { writer.Flush(); skillsData = writer.ReadAllBytes(); writer.Close(false); }

            replaceSector(TH1_SECTOR_SKILLTREE, skillsData);
        }

        #endregion Skills Tree IO

        #region Runes IO

        private void dataToRunes()
        {
            List<TH1Rune> tmpRunes = new List<TH1Rune>();

            // Buffer It
            byte[] runesData = new byte[this.sectors[TH1_SECTOR_RUNE].size];
            Array.Copy(this.rawData, this.sectors[TH1_SECTOR_RUNE].offset, runesData, 0, runesData.Length);

            RWStream reader = new RWStream(runesData, true);
            try
            {
                int runeCount = (int)reader.ReadUInt32();

                for (int runeloop = 0; runeloop < runeCount; runeloop++)
                {
                    int nameLength = (int)reader.ReadUInt32();
                    TH1Rune tmpRune = new TH1Rune();

                    tmpRune.nameToRune(reader.ReadBytes(nameLength));
                    tmpRune.purchased = reader.ReadUInt32();
                    tmpRune.dataB = reader.ReadUInt32();
                    tmpRune.baseValue = reader.ReadUInt32();
                    tmpRune.dataD = reader.ReadUInt32();
                    tmpRune.paintID = reader.ReadUInt32();

                    tmpRunes.Add(tmpRune);
                }

            }
            catch (Exception ex) { setError(14, "Unable To Parse Runes: " + ex.ToString()); }
            finally { reader.Close(false); this.runes = tmpRunes; }

        }

        private void runesToData()
        {
            long bytesize = 0;
            foreach( TH1Rune tmpRune in this.runes) bytesize += tmpRune.getByteSize; 
            byte[] tmpRunes = new byte[4+bytesize];

            RWStream writer = new RWStream(tmpRunes, true, true);
            try
            {
                writer.WriteUInt32((uint)this.runes.Count);
                foreach (TH1Rune runeLoop in this.runes) writer.WriteBytes(runeLoop.runeToArray);
            } catch( Exception ex) { setError(15, "Unable To Write Runes: " + ex.ToString()); }
            finally { writer.Flush(); tmpRunes = writer.ReadAllBytes(); writer.Close(true); }

            replaceSector(TH1_SECTOR_RUNE, tmpRunes);
        }

        #endregion Runes IO

        #region General Functions

        private void verifyRawData()
        {
            int peekSize = 4;
            var shortHead = new byte[peekSize];
            var shortPeek = new byte[peekSize];
            byte[] newhash;

            setError(0, "OK");

            if(this.rawData == null)
            {
                setError(1, "No Save Data Was Loaded");
                return;
            }

            RWStream readerW = new RWStream(this.rawData, true, true);
            try
            {
                if (!(readerW.Length >= TH1_LIMIT_MINSIZE)) // - Check #1 (Minimum Length)
                {
                    setError(2, "File Size Is Below Minimum");
                    return;
                }

                // grab the true data size (no padding / overflow)
                readerW.Position = TH1_OFFSET_SIZE;
                this.dataSize = readerW.ReadInt32();
                if (!(readerW.Length >= this.dataSize))
                {
                    setError(3, "Data Size value is larger than the actual file-size");
                    return;
                }

                // trim the fat
                readerW.WriterBaseStream.SetLength(this.dataSize);

                // flush out the hash
                readerW.Position = TH1_OFFSET_HASH;
                this.hash = readerW.ReadBytes(PLACEHOLDER_HASH.Length);
                readerW.Position = TH1_OFFSET_HASH;
                readerW.WriteBytes(PLACEHOLDER_HASH);
                readerW.Flush();

            }
            catch (Exception ex) {
                setError(4,"IO Error Caught: " + ex.ToString());
                return;
            }
            finally {
                this.rawData = readerW.ReadAllBytes();
                readerW.Close(true);
            }

            newhash = getHash();
            this.hashVerified = this.hash.SequenceEqual(newhash);

        }

        private void loadGamesaveSectors()
        {
            byte[] deliminator = new byte[4];
            UInt32 thisSize = 0;
            long[] sectors;

            // Load
            Array.Copy(PUBLIC_HEADER, deliminator, deliminator.Length);

            RWStream reader = new RWStream(this.rawData, true);
            try
            {
                sectors = reader.SearchHexString(BitConverter.ToString(deliminator).Replace("-",""), false);
                if (sectors.Length > 6)
                {
                    for (int cursec = 0; cursec < sectors.Length; cursec++)
                    {

                        // Checking for Next Sector Available.
                        if (cursec < (sectors.Length - 1)) thisSize = (UInt32)(sectors[cursec + 1] - sectors[cursec]);
                        else thisSize = (UInt32)(this.dataSize - sectors[cursec]);

                        // Load In The Sector
                        thisSize -= (UInt32)deliminator.Length; // we don't need the header every time..

                        // Add the important references to the list
                        TH1Sector tmpsector = new TH1Sector();
                        tmpsector.id = cursec;
                        tmpsector.offset = sectors[cursec] + deliminator.Length;
                        tmpsector.size = thisSize;
                        this.sectors.Add(tmpsector);

                    }
                }
                else {
                    setError(5, "Failed to load Gamesave sectors");
                }
            }
            catch { }
            finally { reader.Close(false);  }
        }

        private byte[] getHash( byte[] saveData )
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

        // Overload
        private byte[] getHash()
        {
            return getHash(this.rawData);
        }

        private void setError( long errno, string errmsg)
        {
            this.lastError = errno;
            this.lastErrorMsg = errmsg;
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
        private void MsgBox( string message)
        {
            MessageBox.Show(message);
        }

        private string lookupDataPairName( uint dkey, int dpair)
        {
            switch (dpair)
            {
                case 1:
                    if (this.character.dataPairNamesA.ContainsKey(dkey)) return this.character.dataPairNamesA[dkey];
                    break;
                case 2:
                    if (this.character.dataPairNamesB.ContainsKey(dkey)) return this.character.dataPairNamesB[dkey];
                    break;
            }
            
            return dkey.ToString("X2");
        }

        private uint lookupDataPairValue(string dkey, int dpair)
        {
            switch (dpair)
            {
                case 1:
                    var keysA = this.character.dataPairNamesA.Where(kvp => kvp.Value == dkey).Select(kvp => kvp.Key).Take(1).ToList();
                    if (keysA.Count > 0) return keysA[0];
                    break;
                case 2:
                    var keysB = this.character.dataPairNamesB.Where(kvp => kvp.Value == dkey).Select(kvp => kvp.Key).Take(1).ToList();
                    if (keysB.Count > 0) return keysB[0];
                    break;
            }
            return uint.Parse(dkey, System.Globalization.NumberStyles.HexNumber);
        }

        #endregion General Functions

    }
}
