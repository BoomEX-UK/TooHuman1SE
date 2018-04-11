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
    /// Interaction logic for WeaponEditorWindow.xaml
    /// </summary>
    public partial class WeaponEditorWindow : Window
    {
        public TH1WeaponExt _thisWeapon = new TH1WeaponExt(new TH1Weapon());
        public TH1WeaponExt _originalWeapon;
        public int weaponIndex = -1;
        public TH1Collections db;
        public TH1Helper helper = new TH1Helper();
        private bool formReady = false;

        public WeaponEditorWindow()
        {
            InitializeComponent();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            // On Show Event Stuff Goes Below..
            buildDropDowns();
            setupForm( true );
        }

        private void buildDropDowns()
        {
            // Any?
            comboClass.Items.Add("Any");
            comboAlignment.Items.Add("Any");

            // Selections
            foreach (string wecol in helper.weaponColourNamesArray) comboColour.Items.Add(wecol);
            foreach (string wecol in helper.weaponTypesArray) comboType.Items.Add(wecol);
            foreach (string wecol in helper.classNamesArray) comboClass.Items.Add(wecol);
            foreach (string wecol in helper.alignmentNamesArray) comboAlignment.Items.Add(wecol);
            comboPaint.ItemsSource = db.paintCollection.paintNameArray();
        }

        private void setupForm(bool initial)
        {
            formReady = false;
            if (_originalWeapon == null) _originalWeapon = _thisWeapon;

            if (_thisWeapon.crafted) this.Title = "Weapon Editor";
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
            comboColour.SelectedItem = helper.getRuneColourName(_thisWeapon.colourKey);
            comboType.SelectedItem = _thisWeapon.weaponTypeName;

            // Tabs
            setupExtraDataTab();
            setupRunesTab(initial);
            refreshWeaponList();
            lockdownForm();

            // Selection
            gridWeapons.SelectedItem = _thisWeapon.weapon;
            if (gridWeapons.SelectedItem != null) gridWeapons.ScrollIntoView(gridWeapons.SelectedItem);
            else if(gridWeapons.Items.Count > 0) gridWeapons.SelectedIndex = 0;

            // Ready
            formReady = true;
        }

        private void setupExtraDataTab()
        {
            txtB.Text = _thisWeapon.valueB.ToString();
            slideCondition.Maximum = _thisWeapon.maxCondition;
            slideCondition.Value = _thisWeapon.condition;
            comboPaint.SelectedItem = _thisWeapon.paint.paintName;
        }

        private void setupRunesTab( bool initial )
        {
            gridBonus.ItemsSource = null;
            List<TH1RuneSummary> bonusRunes = new List<TH1RuneSummary>();
            foreach (string runeID in _thisWeapon.bonusRunes)
            {
                TH1RuneSummary tmpRune = new TH1RuneSummary();
                tmpRune.setRune(db, runeID);
                if (tmpRune.runeType != helper.RUNE_TYPE_ERROR) bonusRunes.Add(tmpRune);
            }
            gridBonus.ItemsSource = bonusRunes;

            if (initial)
            {
                gridInserted.ItemsSource = null;
                gridInserted.ItemsSource = _thisWeapon.runesInserted;
            }

            groupInsert.Header = string.Format("{0} Free Slot{1} (Right-Click For Options)", _thisWeapon.emptyRuneSlots, _thisWeapon.emptyRuneSlots == 1 ? "" : "s");
        }

        private void refreshWeaponList()
        {
            gridWeapons.ItemsSource = null;
            gridWeapons.ItemsSource = db.weaponCollection.listWeapons(helper.weaponColourNames.Keys.ToArray()[comboColour.SelectedIndex], comboType.SelectedIndex, comboClass.SelectedIndex - 1, comboAlignment.SelectedIndex - 1);
        }

        private void lockdownForm()
        {
            if (_thisWeapon.isEquipt) Functions.log("Editing Equipt Wepaon");
            gridWeapons.IsEnabled = !_thisWeapon.isEquipt;
            comboAlignment.IsEnabled = gridWeapons.IsEnabled;
            comboClass.IsEnabled = gridWeapons.IsEnabled;
            comboColour.IsEnabled = gridWeapons.IsEnabled;
            comboType.IsEnabled = gridWeapons.IsEnabled;
        }

        private void combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (formReady) // saves the overkill
            {
                refreshWeaponList();
                if( gridWeapons.Items.Count > 0 ) gridWeapons.SelectedIndex = 0;
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            _thisWeapon = _originalWeapon;
            setupForm(true);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if(_thisWeapon == null )
            {
                MessageBox.Show("You Must Select A Weapon To Save", "Weapon Selection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                tabControl.SelectedIndex = 0;
                return;
            }

            if((_thisWeapon.emptyRuneSlots - gridInserted.Items.Count) < 0)
            {
                MessageBox.Show(string.Format("You Have Too Many Runes Inserted (Max {0}).",_thisWeapon.emptyRuneSlots),"Inserted Runes Error",MessageBoxButton.OK,MessageBoxImage.Error);
                tabControl.SelectedIndex = 1;
                return;
            }

            _thisWeapon.runesInserted = (List<TH1RuneMExt>)gridInserted.ItemsSource;
            this.DialogResult = true;
        }

        private void gridWeapons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnAdd.IsEnabled = ((DataGrid)sender).SelectedItem != null;
            if (formReady)
            {
                if (btnAdd.IsEnabled) _thisWeapon = new TH1WeaponExt((TH1Weapon)((DataGrid)sender).SelectedItem);
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
                Functions.log(string.Format("Inserting Rune To Weapon [{0}]", dlg.thisRune.rune.runeString));
                List<TH1RuneMExt> tmpRunes = (List<TH1RuneMExt>)gridInserted.ItemsSource;
                gridInserted.ItemsSource = null;

                if ((dlg.thisRune.calcValue == 0) && (dlg.thisRune.valueModifier != 0))
                {
                    Functions.log(string.Format("Value Modifier Caused The Rune Value To Go Out Of Bounds ({0})"), Functions.LC_WARNING);
                    dlg.thisRune.valueModifier = 0;
                }

                tmpRunes.Add(dlg.thisRune);

                gridInserted.ItemsSource = tmpRunes;
            }
            else Functions.log("User Cancelled");

        }

        private void contextRemove_Click(object sender, RoutedEventArgs e)
        {
            if( MessageBox.Show(string.Format("Remove Selected Rune{0}?", gridInserted.SelectedItems.Count == 1 ? "" : "s"),"Confirm Remove",MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                List<TH1RuneMExt> tmpList = (List<TH1RuneMExt>)gridInserted.ItemsSource;
                foreach(TH1RuneMExt tmpRune in gridInserted.SelectedItems)
                {
                    tmpList.Remove(tmpRune);
                }

                gridInserted.ItemsSource = null;
                gridInserted.ItemsSource = tmpList;
            }
        }

        private void gridInserted_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            contextInsert.IsEnabled = (_thisWeapon.emptyRuneSlots - gridInserted.Items.Count > 0);
            contextRemove.IsEnabled = (gridInserted.SelectedItem != null);
            contextDuplicate.IsEnabled = contextInsert.IsEnabled && contextRemove.IsEnabled;
        }

        private void contextDuplicate_Click(object sender, RoutedEventArgs e)
        {
            List<TH1RuneMExt> tmpList = (List<TH1RuneMExt>)gridInserted.ItemsSource;
            foreach (TH1RuneMExt tmpRune in gridInserted.SelectedItems)
            {
                if (_thisWeapon.emptyRuneSlots - gridInserted.Items.Count > 0)
                    tmpList.Add(tmpRune);
            }
            gridInserted.ItemsSource = null;
            gridInserted.ItemsSource = tmpList;
        }

        private void slideCondition_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (formReady) _thisWeapon.condition = (uint)slideCondition.Value;
        }

        private void comboPaint_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (formReady) _thisWeapon.paint = db.paintCollection.findPaint((string)comboPaint.SelectedItem);
        }
    }
}
