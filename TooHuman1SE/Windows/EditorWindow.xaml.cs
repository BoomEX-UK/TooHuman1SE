using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TooHuman1SE.SEFunctions;
using TooHuman1SE.SEStructure;
using TooHuman1SE.Editor_Tabs;

namespace TooHuman1SE.Windows
{
    /// <summary>
    /// Interaction logic for EditorWindow.xaml
    /// </summary>
    /// 

    public partial class EditorWindow : Window
    {
        public TH1SaveStructure _save;
        TabCharacterUC tc = new TabCharacterUC();
        TabDataPairsUC dp = new TabDataPairsUC();
        TabSkillsTreeUC st = new TabSkillsTreeUC();
        TabRunesUC ru = new TabRunesUC();


        public EditorWindow()
        {
            InitializeComponent();
            attachTabs();
            tc.txtCExp.TextChanged += txtExp_TextChanged;
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            // On Show Event Stuff Goes Below..

            LoadAll();
            setWindowTitle();

        }

        #region Tab Loading

        private void LoadAll()
        {
            loadEditorWindow();
            loadCharacterTab();
            loadDataPairsTab();
            loadSkillTreeTab();
            loadRunesTab();
        }

        private void loadEditorWindow()
        {
            txtName.MaxLength = _save.character.LIMIT_NAME_LENGTH;
            txtName.Text = _save.character.name;
            txtSlot.Text = _save.character.saveSlot.ToString();
        }

        private void loadCharacterTab()
        {
            tc.txtCBounty.Text = _save.character.bounty.ToString();
            tc.txtCExp.Text = _save.character.exp.ToString();
            tc.txtCSkillPoints.Text = _save.character.skillPoints.ToString();
        }

        private void loadDataPairsTab()
        {
            dp.dataPairsA.ItemsSource = _save.character.dataPairsA;
            dp.dataPairsB.ItemsSource = _save.character.dataPairsB;
        }

        private void loadSkillTreeTab()
        {
            string[] stnames = _save.skills.getSkillNames();
            List<Label> labelList = GetLogicalChildCollection<Label>(st);
            List<TextBox> textList = GetLogicalChildCollection<TextBox>(st);

            foreach ( Label la in labelList)
            {
                int tagno;
                try
                {
                    tagno = int.Parse((string)la.Tag);
                }
                catch { tagno = -1; }

                if( (tagno > -1) && (tagno < stnames.Length-1))
                {
                    la.Content = stnames[tagno];
                }

                la.ToolTip = la.Content;
            }

            foreach (TextBox tb in textList)
            {
                int tagno;
                try
                {
                    tagno = int.Parse((string)tb.Tag);
                }
                catch { tagno = -1; }

                if ((tagno > -1) && (tagno < _save.skills.pairs.Count - 1))
                {
                    tb.Text = _save.skills.pairs[tagno].first.ToString();
                    tb.ToolTip = _save.skills.pairs[tagno].second.ToString();
                }

            }
        }

        private void loadRunesTab()
        {
            ru.gridRunes.ItemsSource = _save.runes;
            ru.recountRunes();
            ru.buildContextMenu();
        }

        #endregion Tab Loading

        #region Tab Saving

        private void saveAll()
        {
            saveEditorWindow();
            saveCharacterTab();
            saveDataPairsTab();
            saveSkillTreeTab();
        }

        private void saveEditorWindow()
        {
            _save.character.name = txtName.Text;
            _save.character.saveSlot = int.Parse(txtSlot.Text);
        }

        private void saveCharacterTab()
        {
            TH1ExpToNextLevel ex = new TH1ExpToNextLevel();
            _save.character.bounty = long.Parse(tc.txtCBounty.Text);
            _save.character.exp = long.Parse(tc.txtCExp.Text);
            _save.character.level = ex.calcLevel(_save.character.exp);
            _save.character.skillPoints = long.Parse(tc.txtCSkillPoints.Text);
        }

        private void saveDataPairsTab()
        {

        }

        private void saveSkillTreeTab()
        {
            List<Label> labelList = GetLogicalChildCollection<Label>(st);
            List<TextBox> textList = GetLogicalChildCollection<TextBox>(st);

            foreach (TextBox tb in textList)
            {
                int tagno;
                try
                {
                    tagno = int.Parse((string)tb.Tag);
                }
                catch { tagno = -1; }

                if ((tagno > -1) && (tagno < _save.skills.pairs.Count - 1))
                {
                    try
                    {
                        _save.skills.pairs[tagno].first = Math.Min(int.Parse(tb.Text), _save.skills.LIMIT_SKILL_MAX);
                    } catch
                    {
                        _save.skills.pairs[tagno].first = 0;
                    }
                }

            }
        }

        #endregion Tab Saving

        #region Functions

        private void setWindowTitle()
        {
            TH1CharClassAlign ca = new TH1CharClassAlign();
            this.Title = _save.character.name + ": L" + _save.character.level.ToString("N0") + " " + ca.alignmentNames[_save.character.alignment] + ", " + ca.classNames[_save.character.charClass];
        }

        private static bool validSaveSlot(string text)
        {
            Regex regex = new Regex("[^0-5.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        // Validate Entire Form
        private bool validateInputs()
        {
            bool tmpres = true;
            // in

            // out
            return tmpres;
        }

        private void attachTabs()
        {
            foreach( TabItem ti in tabEditor.Items)
            {
                string head = ti.Header.ToString();
                if (head == "Character") ti.Content = tc;
                if (head == "Stats") ti.Content = dp;
                if (head == "Skill Tree") ti.Content = st;
                if (head == "Runes") ti.Content = ru;
            }
        }

        private void refreshLevelLabel()
        {
            TH1ExpToNextLevel expStruc = new TH1ExpToNextLevel();
            string levelSuffix = "";
            long _exp = 0;
            try
            {
                _exp = long.Parse(tc.txtCExp.Text);
            }
            catch { }

            if (expStruc.expToNext(_exp) > 0)
            {
                levelSuffix = " (" + expStruc.expProgressToNext(_exp).ToString() + "/" + expStruc.expToNext(_exp).ToString() + ")";
            }
            tc.lblLevel.Content = expStruc.calcLevel(_exp).ToString() + levelSuffix;

        }

        public static List<T> GetLogicalChildCollection<T>(object parent) where T : DependencyObject
        {
            List<T> logicalCollection = new List<T>();
            GetLogicalChildCollection(parent as DependencyObject, logicalCollection);
            return logicalCollection;
        }
        private static void GetLogicalChildCollection<T>(DependencyObject parent, List<T> logicalCollection) where T : DependencyObject
        {
            IEnumerable children = LogicalTreeHelper.GetChildren(parent);
            foreach (object child in children)
            {
                if (child is DependencyObject)
                {
                    DependencyObject depChild = child as DependencyObject;
                    if (child is T)
                    {
                        logicalCollection.Add(child as T);
                    }
                    GetLogicalChildCollection(depChild, logicalCollection);
                }
            }
        }

        #endregion Functions

        #region Event Handlers

        private void txtSlot_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !validSaveSlot(e.Text);
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            if( validateInputs() )
            {
                Functions.log("Editor Validated.", Functions.LC_SUCCESS);
                saveAll();

                // Show 
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "Too Human SaveGame"; // Default file name
                dlg.DefaultExt = ".txt"; // Default file extension
                dlg.Filter = "savegame (.txt)|*.txt"; // Filter files by extension
                dlg.FileName = "savegame.txt"; // Default Filename

                // Show save file dialog box
                Nullable<bool> result = dlg.ShowDialog();

                // Process save file dialog box results
                if (result == true)
                {
                // Save document
                _save.writeSaveFile(dlg.FileName);
                }
            } else
            {
                Functions.log("Editor Contains Invalid Entries. Saving Cancelled.", Functions.LC_CRITICAL);
            }

        }

        private void txtExp_TextChanged(object sender, TextChangedEventArgs e)
        {
            refreshLevelLabel();
        }

        #endregion Event Handlers
    }
}
