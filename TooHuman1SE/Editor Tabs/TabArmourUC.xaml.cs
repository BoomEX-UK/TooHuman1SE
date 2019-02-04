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
    /// Interaction logic for TabArmourUC.xaml
    /// </summary>
    public partial class TabArmourUC : UserControl
    {

        public EditorWindow _ewin;
        public TH1ArmourCollection armourCollection;

        public TabArmourUC()
        {
            InitializeComponent();
        }

        private void ShowArmourEditorWindow(int armourIndex, bool isBlueprint)
        {
            ArmourEditorWindow dlg = new ArmourEditorWindow();
            DataGrid _grid;
            List<TH1ArmourExt> _list;

            dlg.Owner = Window.GetWindow(this);
            dlg.armourIndex = armourIndex;
            dlg.db = _ewin.db;
            dlg._thisArmour.crafted = !isBlueprint;

            if (isBlueprint)
            {
                _grid = gridBlueprints;
                _list = _ewin._save.armourBlueprints;
            }
            else
            {
                _grid = gridArmour;
                _list = _ewin._save.armourInventory;
            }

            if (armourIndex > -1)
            {
                dlg._thisArmour = (TH1ArmourExt)_grid.Items[armourIndex];
                Functions.log(string.Format("Editing Armour #{0} ({1})", armourIndex, dlg._thisArmour.armourLongIdName), Functions.LC_PRIMARY);
            }
            else
            {
                // Check Inventory Full
                Functions.log("Creating New Armour", Functions.LC_PRIMARY);
            }

            if (dlg.ShowDialog().Equals(true))
            {
                Functions.log("Saving Armour");
                _grid.ItemsSource = null;

                // Add Or Edit
                dlg._thisArmour.crafted = !isBlueprint;
                if (armourIndex > -1) _list[armourIndex] = dlg._thisArmour;
                else _list.Add(dlg._thisArmour);

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

            if (!isBlueprint) {
                MessageBox.Show("Add Armour is still under development. Try adding Armour as a blueprint and crafting in-game.","Add Armour ..",MessageBoxButton.OK,MessageBoxImage.Warning);
            } else  ShowArmourEditorWindow(-1, isBlueprint);
        }

        private void gridArmour_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid == null) return;

            var src = VisualTreeHelper.GetParent((DependencyObject)e.OriginalSource);
            var srcType = src.GetType();
            if (srcType == typeof(ContentPresenter) || srcType == typeof(DataGridCell))
            {
                if (grid.SelectedItem != null)
                {
                    ShowArmourEditorWindow(grid.SelectedIndex, (string)grid.Tag == "1");
                }
            }
        }

        private void mnuDelete_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mnu = sender as MenuItem;
            if (mnu == null) return;

            DataGrid grid;
            List<TH1ArmourExt> list;
            bool isBlueprint = (string)mnu.Tag == "1";

            if (isBlueprint)
            {
                grid = gridBlueprints;
                list = _ewin._save.armourBlueprints;
            }
            else
            {
                grid = gridArmour;
                list = _ewin._save.armourInventory;
            }

            int itemcount = grid.SelectedItems.Count;
            if (itemcount > 0)
            {
                if (MessageBox.Show(string.Format("Are You Sure You Want To Delete {0} Armour?", itemcount), "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

                foreach (TH1ArmourExt armo in grid.SelectedItems)
                {
                    if (!armo.isEquipt) list.Remove(armo);
                    else MessageBox.Show(string.Format("You Cannot Delete An Equipt Armour ({0}).", armo.armourLongName), "Delete Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                grid.ItemsSource = null;
                grid.ItemsSource = list;
            }
        }

        private void mnuDuplicate_Click(object sender, RoutedEventArgs e)
        {
            List<TH1ArmourExt> list;
            DataGrid grid;
            bool isBlueprint;

            MenuItem mnu = sender as MenuItem;
            if (mnu == null) return;
            isBlueprint = ((string)mnu.Tag == "1");

            if (isBlueprint)
            {
                grid = gridBlueprints;
                list = _ewin._save.armourBlueprints;
            }
            else
            {
                grid = gridArmour;
                list = _ewin._save.armourInventory;
            }

            foreach (TH1ArmourExt armo in grid.SelectedItems)
            {
                TH1ArmourExt newArmo = new TH1ArmourExt(armo.armour)
                {
                    crafted = !isBlueprint,
                    valueB = armo.valueB,
                    condition = armo.condition,
                    paint = armo.paint,
                    runesInserted = armo.runesInserted
                };
                list.Add(newArmo);
            }
            grid.ItemsSource = null;
            grid.ItemsSource = list;
        }

        private void mnuEquip_Click(object sender, RoutedEventArgs e)
        {
            if (gridArmour.SelectedItem != null)
            {
                TH1ArmourExt selectedArmour = ((TH1ArmourExt)gridArmour.SelectedItem);
                int armourGroup = _ewin.db.helper.getArmourGroup(selectedArmour.armourType);

                for (int i = 0; i < _ewin._save.armourInventory.Count; i++)
                {
                    if (_ewin.db.helper.getArmourGroup(_ewin._save.armourInventory[i].armourType) == armourGroup)
                        _ewin._save.armourInventory[i].isEquipt = gridArmour.SelectedIndex == i;
                }
                gridArmour.ItemsSource = null;
                gridArmour.ItemsSource = _ewin._save.armourInventory;
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
            else grid = gridArmour;

            if (grid.SelectedItem != null)
            {
                ShowArmourEditorWindow(grid.SelectedIndex, isBlueprint);
            }
        }

        private void grid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            mnuDeleteInv.IsEnabled = gridArmour.SelectedItem != null; // is selected
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
            List<TH1ArmourExt> listFrom;
            List<TH1ArmourExt> listTo;
            bool fromBlueprint;

            MenuItem mnu = sender as MenuItem;
            if (mnu == null) return;

            fromBlueprint = ((string)mnu.Tag == "1");
            if (fromBlueprint)
            {
                listFrom = _ewin._save.armourBlueprints;
                listTo = _ewin._save.armourInventory;
                grid = gridBlueprints;
            }
            else
            {
                listFrom = _ewin._save.armourInventory;
                listTo = _ewin._save.armourBlueprints;
                grid = gridArmour;
            }

            foreach (TH1ArmourExt armo in grid.SelectedItems)
            {
                if (!armo.isEquipt) // avoid equipt
                {
                    TH1ArmourExt newArmo = new TH1ArmourExt(armo.armour)
                    {
                        crafted = fromBlueprint,
                        valueB = armo.valueB,
                        condition = armo.condition,
                        paint = armo.paint,
                        runesInserted = armo.runesInserted
                    };
                    listTo.Add(newArmo);
                    listFrom.Remove(newArmo);
                }
            }

            gridBlueprints.ItemsSource = null;
            gridArmour.ItemsSource = null;

            gridBlueprints.ItemsSource = _ewin._save.armourBlueprints;
            gridArmour.ItemsSource = _ewin._save.armourInventory;
        }
        #endregion Event Handlers
    }
}
