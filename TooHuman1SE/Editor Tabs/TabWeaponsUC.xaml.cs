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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TooHuman1SE.Windows;
using TooHuman1SE.SEStructure;
using TooHuman1SE.SEFunctions;

namespace TooHuman1SE.Editor_Tabs
{
    /// <summary>
    /// Interaction logic for TabWeaponsUC.xaml
    /// </summary>
    public partial class TabWeaponsUC : UserControl
    {
        public EditorWindow _ewin;
        public TH1WeaponCollection weaponCollection;

        public TabWeaponsUC()
        {
            InitializeComponent();
        }

        private void ShowWeaponEditorWindow(int weaponIndex)
        {
            WeaponEditorWindow dlg = new WeaponEditorWindow();

            dlg.Owner = Window.GetWindow(this);
            dlg.weaponIndex = weaponIndex;
            dlg.db = _ewin.db;
                        
            if (weaponIndex > -1)
            {
                dlg._thisWeapon = (TH1WeaponExt)gridWeapons.Items[weaponIndex];
                Functions.log(string.Format("Editing Weapon #{0} ({1})", weaponIndex, dlg._thisWeapon.weaponLongIdName), Functions.LC_PRIMARY);
            }
            else
            {
                // Check Inventory Full
                Functions.log("Creating New Weapon", Functions.LC_PRIMARY);
            }
            

            if (dlg.ShowDialog().Equals(true))
            {
                Functions.log("Saving Weapon");

                gridWeapons.ItemsSource = null;

                // Add Or Edit
                if (weaponIndex > -1) _ewin._save.weapons[weaponIndex] = dlg._thisWeapon;
                else _ewin._save.weapons.Add(dlg._thisWeapon);

                gridWeapons.ItemsSource = _ewin._save.weapons;
            }
            else Functions.log("User Cancelled");
        }

        #region Event Handlers
        private void btnAddRune_Click(object sender, RoutedEventArgs e)
        {
            ShowWeaponEditorWindow(-1);
        }
        
        private void gridWeapons_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var src = VisualTreeHelper.GetParent((DependencyObject)e.OriginalSource);
            var srcType = src.GetType();
            if (srcType == typeof(ContentPresenter) || srcType == typeof(DataGridCell))
            {
                if (gridWeapons.SelectedItem != null)
                {
                    ShowWeaponEditorWindow(gridWeapons.SelectedIndex);
                }
            }
        }

        private void mnuDelete_Click(object sender, RoutedEventArgs e)
        {
            int itemcount = gridWeapons.SelectedItems.Count;
            if (itemcount > 0)
            {
                if (MessageBox.Show(string.Format("Are You Sure You Want To Delete {0} Weapon{1}?", itemcount, itemcount == 1 ? "" : "s"), "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

                foreach( TH1WeaponExt weap in gridWeapons.SelectedItems)
                {
                    if (!weap.isEquipt) _ewin._save.weapons.Remove(weap);
                    else MessageBox.Show(string.Format("You cannot delete an equipt weapon ({0}).",weap.weaponLongName), "Delete Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                gridWeapons.ItemsSource = null;
                gridWeapons.ItemsSource = _ewin._save.weapons;
            }
        }

        private void mnuDuplicate_Click(object sender, RoutedEventArgs e)
        {
            foreach (TH1WeaponExt weap in gridWeapons.SelectedItems)
            {
                TH1WeaponExt newWeap = new TH1WeaponExt(weap.weapon);
                newWeap.valueA = weap.valueA;
                newWeap.valueB = weap.valueB;
                newWeap.condition = newWeap.condition;
                newWeap.paint = weap.paint;
                newWeap.runesInserted = weap.runesInserted;
                _ewin._save.weapons.Add(newWeap);
            }
            gridWeapons.ItemsSource = null;
            gridWeapons.ItemsSource = _ewin._save.weapons;
        }

        private void mnuEquip_Click(object sender, RoutedEventArgs e)
        {
            if (gridWeapons.SelectedItem != null)
            {
                TH1WeaponExt selectedWeapon = ((TH1WeaponExt)gridWeapons.SelectedItem);
                int weaponGroup = _ewin.db.helper.getWeaponGroup(selectedWeapon.weaponType);

                for ( int i=0; i < _ewin._save.weapons.Count; i++)
                {
                    if (_ewin.db.helper.getWeaponGroup(_ewin._save.weapons[i].weaponType) == weaponGroup)
                        _ewin._save.weapons[i].isEquipt = gridWeapons.SelectedIndex == i;
                }
                gridWeapons.ItemsSource = null;
                gridWeapons.ItemsSource = _ewin._save.weapons;
            }
        }

        private void mnuEdit_Click(object sender, RoutedEventArgs e)
        {
            if (gridWeapons.SelectedItem != null)
            {
                ShowWeaponEditorWindow(gridWeapons.SelectedIndex);
            }
        }
        
        private void gridWeapons_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            mnuDelete.IsEnabled = gridWeapons.SelectedItem != null; // is selected
            mnuAdd.IsEnabled = true; // can add

            mnuDuplicate.IsEnabled = mnuDelete.IsEnabled && mnuAdd.IsEnabled;
            mnuEdit.IsEnabled = mnuDelete.IsEnabled;
            mnuEquip.IsEnabled = mnuDelete.IsEnabled;
        }
        #endregion Event Handlers
    }
}
