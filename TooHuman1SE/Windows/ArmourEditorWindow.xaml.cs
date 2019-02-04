using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TooHuman1SE.SEStructure;
using TooHuman1SE.SEFunctions;
using TooHuman1SE.Windows;

namespace TooHuman1SE.Windows
{
    /// <summary>
    /// Interaction logic for ArmourEditorWindow.xaml
    /// </summary>
    public partial class ArmourEditorWindow : Window
    {
        public TH1ArmourExt _thisArmour = new TH1ArmourExt(new TH1Armour());
        public TH1ArmourExt _originalArmour;
        public int armourIndex = -1;
        public TH1Collections db;
        public TH1Helper helper = new TH1Helper();
        private bool formReady = false;

        public ArmourEditorWindow()
        {
            InitializeComponent();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            // On Show Event Stuff Goes Below..
            buildDropDowns();
            setupForm(true);
        }

        private void buildDropDowns()
        {
            // Any?
            comboClass.Items.Add("Any");
            comboAlignment.Items.Add("Any");

            // Selections
            foreach (string arcol in helper.equipmentColourNamesArray) comboColour.Items.Add(arcol);
            foreach (string artyp in helper.armourTypesArray) comboType.Items.Add(artyp);
            foreach (string arcla in helper.classNamesArray) comboClass.Items.Add(arcla);
            foreach (string arali in helper.alignmentNamesArray) comboAlignment.Items.Add(arali);
            comboPaint.ItemsSource = db.paintCollection.paintNameArray();
        }

        private void setupForm(bool initial)
        {
            formReady = false;
            if (_originalArmour == null) _originalArmour = _thisArmour;

            if (_thisArmour.crafted) this.Title = "Armour Editor";
            else this.Title = "Blueprint Editor";

            // Combo's
            foreach (object comb in gridSelections.Children)
            {
                ComboBox _combo = comb as ComboBox;
                if (_combo != null) _combo.SelectedIndex = 0;
            }

            // Any Filter
            comboClass.SelectedIndex = 0;
            comboAlignment.SelectedIndex = 0;

            // Specific Filter
            comboColour.SelectedItem = helper.getRuneColourName(_thisArmour.colourKey);
            comboType.SelectedItem = _thisArmour.armourTypeName;

            // Tabs
            setupExtraDataTab();
            setupRunesTab(initial);
            refreshArmourList();
            lockdownForm();

            // Selection
            gridArmour.SelectedItem = _thisArmour.armour;
            if (gridArmour.SelectedItem != null) gridArmour.ScrollIntoView(gridArmour.SelectedItem);
            else if (gridArmour.Items.Count > 0) gridArmour.SelectedIndex = 0;

            // Ready
            formReady = true;
        }

        private void setupExtraDataTab()
        {
            txtB.Text = _thisArmour.valueB.ToString();
            slideCondition.Maximum = _thisArmour.maxCondition;
            slideCondition.Value = _thisArmour.condition;
            comboPaint.SelectedItem = _thisArmour.paint.paintName;
        }

        private void setupRunesTab(bool initial)
        {
            gridBonus.ItemsSource = null;
            List<TH1RuneSummary> bonusRunes = new List<TH1RuneSummary>();
            foreach (string runeID in _thisArmour.bonusRunes)
            {
                TH1RuneSummary tmpRune = new TH1RuneSummary();
                tmpRune.setRune(db, runeID);
                if (tmpRune.runeType != helper.RUNE_TYPE_ERROR) bonusRunes.Add(tmpRune);
            }
            gridBonus.ItemsSource = bonusRunes;

            if (initial)
            {
                gridInserted.ItemsSource = null;
                gridInserted.ItemsSource = _thisArmour.runesInserted;
            }

            groupInsert.Header = string.Format("{0} Free Slot{1} (Right-Click For Options)", _thisArmour.emptyRuneSlots, _thisArmour.emptyRuneSlots == 1 ? "" : "s");
        }

        private void refreshArmourList()
        {
            gridArmour.ItemsSource = null;
            gridArmour.ItemsSource = db.armourCollection.listArmour(helper.equipmentColourNames.Keys.ToArray()[comboColour.SelectedIndex], comboType.SelectedIndex, comboClass.SelectedIndex - 1, comboAlignment.SelectedIndex - 1);
        }

        private void lockdownForm()
        {
            if (_thisArmour.isEquipt) Functions.log("Editing Equipt Armour");
            gridArmour.IsEnabled = !_thisArmour.isEquipt;
            comboAlignment.IsEnabled = gridArmour.IsEnabled;
            comboClass.IsEnabled = gridArmour.IsEnabled;
            comboColour.IsEnabled = gridArmour.IsEnabled;
            comboType.IsEnabled = gridArmour.IsEnabled;
        }

        private void combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (formReady) // saves the overkill
            {
                refreshArmourList();
                if (gridArmour.Items.Count > 0) gridArmour.SelectedIndex = 0;
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            _thisArmour = _originalArmour;
            setupForm(true);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (_thisArmour == null)
            {
                MessageBox.Show("You Must Select An Item To Save", "Armour Selection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                tabControl.SelectedIndex = 0;
                return;
            }

            if ((_thisArmour.emptyRuneSlots - gridInserted.Items.Count) < 0)
            {
                MessageBox.Show(string.Format("You Have Too Many Runes Inserted (Max {0}).", _thisArmour.emptyRuneSlots), "Inserted Runes Error", MessageBoxButton.OK, MessageBoxImage.Error);
                tabControl.SelectedIndex = 1;
                return;
            }

            _thisArmour.runesInserted = (List<TH1RuneMExt>)gridInserted.ItemsSource;
            this.DialogResult = true;
        }

        private void gridArmour_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnAdd.IsEnabled = ((DataGrid)sender).SelectedItem != null;
            if (formReady)
            {
                if (btnAdd.IsEnabled) _thisArmour = new TH1ArmourExt((TH1Armour)((DataGrid)sender).SelectedItem);
                setupExtraDataTab();
                setupRunesTab(false);
            }
        }

        private void contextInsert_Click(object sender, RoutedEventArgs e)
        {
            RuneEditorWindow dlg = new RuneEditorWindow();
            dlg.Owner = Window.GetWindow(this).Owner;
            dlg.runeCollection = db.runeMCollection;

            if (dlg.ShowDialog().Equals(true))
            {
                Functions.log(string.Format("Inserting Rune To Armour [{0}]", dlg.thisRune.rune.runeString));
                List<TH1RuneMExt> tmpRunes = (List<TH1RuneMExt>)gridInserted.ItemsSource;
                gridInserted.ItemsSource = null;

                if ((dlg.thisRune.calcValue == 0) && (dlg.thisRune.valueModifier != 0))
                {
                    Functions.log(string.Format("Value Modifier Caused The Rune Value To Go Out Of Bounds ({0})", dlg.thisRune.valueModifier), Functions.LC_WARNING);
                    dlg.thisRune.valueModifier = 0;
                }

                tmpRunes.Add(dlg.thisRune);

                gridInserted.ItemsSource = tmpRunes;
            }
            else Functions.log("User Cancelled");

        }

        private void contextRemove_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(string.Format("Remove Selected Rune{0}?", gridInserted.SelectedItems.Count == 1 ? "" : "s"), "Confirm Remove", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                List<TH1RuneMExt> tmpList = (List<TH1RuneMExt>)gridInserted.ItemsSource;
                foreach (TH1RuneMExt tmpRune in gridInserted.SelectedItems)
                {
                    tmpList.Remove(tmpRune);
                }

                gridInserted.ItemsSource = null;
                gridInserted.ItemsSource = tmpList;
            }
        }

        private void gridInserted_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            contextInsert.IsEnabled = (_thisArmour.emptyRuneSlots - gridInserted.Items.Count > 0);
            contextRemove.IsEnabled = (gridInserted.SelectedItem != null);
            contextDuplicate.IsEnabled = contextInsert.IsEnabled && contextRemove.IsEnabled;
        }

        private void contextDuplicate_Click(object sender, RoutedEventArgs e)
        {
            List<TH1RuneMExt> tmpList = (List<TH1RuneMExt>)gridInserted.ItemsSource;
            foreach (TH1RuneMExt tmpRune in gridInserted.SelectedItems)
            {
                if (_thisArmour.emptyRuneSlots - gridInserted.Items.Count > 0)
                    tmpList.Add(tmpRune);
            }
            gridInserted.ItemsSource = null;
            gridInserted.ItemsSource = tmpList;
        }

        private void slideCondition_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (formReady) _thisArmour.condition = (uint)slideCondition.Value;
        }

        private void comboPaint_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (formReady) _thisArmour.paint = db.paintCollection.findPaint((string)comboPaint.SelectedItem);
        }
    }
}
