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

        private void ShowWeaponEditorWindow(int weaponIndex, bool isBlueprint )
        {
            WeaponEditorWindow dlg = new WeaponEditorWindow();
            DataGrid _grid;
            List<TH1WeaponExt> _list;

            dlg.Owner = Window.GetWindow(this);
            dlg.weaponIndex = weaponIndex;
            dlg.db = _ewin.db;
            dlg._thisWeapon.crafted = !isBlueprint;

            if (isBlueprint)
            {
                _grid = gridBlueprints;
                _list = _ewin._save.weaponsBlueprints;
            }
            else
            {
                _grid = gridWeapons;
                _list = _ewin._save.weaponsInventory;
            }

            if (weaponIndex > -1)
            {
                dlg._thisWeapon = (TH1WeaponExt)_grid.Items[weaponIndex];
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
                _grid.ItemsSource = null;

                // Add Or Edit
                dlg._thisWeapon.crafted = !isBlueprint;
                if (weaponIndex > -1) _list[weaponIndex] = dlg._thisWeapon;
                else _list.Add(dlg._thisWeapon);

                _grid.ItemsSource = _list;
            }
            else Functions.log("User Cancelled");
        }

        #region Event Handlers
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            MenuItem mnu = sender as MenuItem;

            bool isBlueprint;

            if (btn != null) isBlueprint = (string)btn.Tag == "1";
            else if (mnu != null) isBlueprint = (string)mnu.Tag == "1";
            else return;

            ShowWeaponEditorWindow(-1, isBlueprint );
        }
        
        private void gridWeapons_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid == null) return;

            var src = VisualTreeHelper.GetParent((DependencyObject)e.OriginalSource);
            var srcType = src.GetType();
            if (srcType == typeof(ContentPresenter) || srcType == typeof(DataGridCell))
            {
                if (grid.SelectedItem != null)
                {
                    ShowWeaponEditorWindow(grid.SelectedIndex, (string)grid.Tag == "1");
                }
            }
        }

        private void mnuDelete_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mnu = sender as MenuItem;
            if (mnu == null) return;

            DataGrid grid;
            List<TH1WeaponExt> list;
            bool isBlueprint = (string)mnu.Tag == "1";

            if (isBlueprint)
            {
                grid = gridBlueprints;
                list = _ewin._save.weaponsBlueprints;
            }
            else
            {
                grid = gridWeapons;
                list = _ewin._save.weaponsInventory;
            }

            int itemcount = grid.SelectedItems.Count;
            if (itemcount > 0)
            {
                if (MessageBox.Show(string.Format("Are You Sure You Want To Delete {0} Weapon{1}?", itemcount, itemcount == 1 ? "" : "s"), "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

                foreach( TH1WeaponExt weap in grid.SelectedItems)
                {
                    if (!weap.isEquipt) list.Remove(weap);
                    else MessageBox.Show(string.Format("You cannot delete an equipt weapon ({0}).",weap.weaponLongName), "Delete Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                grid.ItemsSource = null;
                grid.ItemsSource = list;
            }
        }

        private void mnuDuplicate_Click(object sender, RoutedEventArgs e)
        {
            List<TH1WeaponExt> list;
            DataGrid grid;
            bool isBlueprint;

            MenuItem mnu = sender as MenuItem;
            if (mnu == null) return;
            isBlueprint = ((string)mnu.Tag == "1");

            if( isBlueprint)
            {
                grid = gridBlueprints;
                list = _ewin._save.weaponsBlueprints;
            } else
            {
                grid = gridWeapons;
                list = _ewin._save.weaponsInventory;
            }

            foreach (TH1WeaponExt weap in grid.SelectedItems)
            {
                TH1WeaponExt newWeap = new TH1WeaponExt(weap.weapon) {
                    crafted = !isBlueprint,
                    valueB = weap.valueB,
                    condition = weap.condition,
                    paint = weap.paint,
                    runesInserted = weap.runesInserted
                };
                list.Add(newWeap);
            }
            grid.ItemsSource = null;
            grid.ItemsSource = list;
        }

        private void mnuEquip_Click(object sender, RoutedEventArgs e)
        {
            if (gridWeapons.SelectedItem != null)
            {
                TH1WeaponExt selectedWeapon = ((TH1WeaponExt)gridWeapons.SelectedItem);
                int weaponGroup = _ewin.db.helper.getWeaponGroup(selectedWeapon.weaponType);

                for ( int i=0; i < _ewin._save.weaponsInventory.Count; i++)
                {
                    if (_ewin.db.helper.getWeaponGroup(_ewin._save.weaponsInventory[i].weaponType) == weaponGroup)
                        _ewin._save.weaponsInventory[i].isEquipt = gridWeapons.SelectedIndex == i;
                }
                gridWeapons.ItemsSource = null;
                gridWeapons.ItemsSource = _ewin._save.weaponsInventory;
            }
        }

        private void mnuEdit_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mnu = sender as MenuItem;
            DataGrid grid;
            bool isBlueprint;

            if (mnu != null) isBlueprint = (string)mnu.Tag == "1";
            else return;

            if (isBlueprint) grid = gridBlueprints;
            else grid = gridWeapons;

            if (grid.SelectedItem != null)
            {
                ShowWeaponEditorWindow(grid.SelectedIndex, isBlueprint);
            }
        }
        
        private void grid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            mnuDeleteInv.IsEnabled = gridWeapons.SelectedItem != null; // is selected
            mnuAddInv.IsEnabled = true; // can add

            mnuDuplicateInv.IsEnabled = mnuDeleteInv.IsEnabled && mnuAddInv.IsEnabled;
            mnuEditInv.IsEnabled = mnuDeleteInv.IsEnabled;
            mnuEquipInv.IsEnabled = mnuDeleteInv.IsEnabled;
            mnuCreateBP.IsEnabled = mnuDeleteInv.IsEnabled;

            // -----

            mnuDeleteBP.IsEnabled = gridBlueprints.SelectedItem != null; // is selected
            mnuAddBP.IsEnabled = true; // can add

            mnuDuplicateBP.IsEnabled = mnuDeleteBP.IsEnabled && mnuAddBP.IsEnabled;
            mnuEditBP.IsEnabled = mnuDeleteBP.IsEnabled;
            mnuCraftBP.IsEnabled = mnuDeleteBP.IsEnabled;
            mnuCreateBP.IsEnabled = mnuDeleteBP.IsEnabled;
        }

        private void mnuCraftBP_Click(object sender, RoutedEventArgs e)
        {
            DataGrid grid;
            List<TH1WeaponExt> listFrom;
            List<TH1WeaponExt> listTo;
            bool fromBlueprint;

            MenuItem mnu = sender as MenuItem;
            if (mnu == null) return;

            fromBlueprint = ((string)mnu.Tag == "1");
            if(fromBlueprint)
            {
                listFrom = _ewin._save.weaponsBlueprints;
                listTo = _ewin._save.weaponsInventory;
                grid = gridBlueprints;
            } else
            {
                listFrom = _ewin._save.weaponsInventory;
                listTo = _ewin._save.weaponsBlueprints;
                grid = gridWeapons;
            }

            foreach( TH1WeaponExt weap in grid.SelectedItems)
            {
                if (!weap.isEquipt) // avoid equipt
                {
                    TH1WeaponExt newWeap = new TH1WeaponExt(weap.weapon)
                    {
                        crafted = fromBlueprint,
                        valueB = weap.valueB,
                        condition = weap.condition,
                        paint = weap.paint,
                        runesInserted = weap.runesInserted
                    };
                    listTo.Add(newWeap);
                    listFrom.Remove(weap);
                }
            }

            gridBlueprints.ItemsSource = null;
            gridWeapons.ItemsSource = null;

            gridBlueprints.ItemsSource = _ewin._save.weaponsBlueprints;
            gridWeapons.ItemsSource = _ewin._save.weaponsInventory;
        }

        #endregion Event Handlers

    }
}
