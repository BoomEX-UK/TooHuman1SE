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
    /// Interaction logic for TabCharmsUC.xaml
    /// </summary>
    public partial class TabCharmsUC : UserControl
    {
        public TabCharmsUC()
        {
            InitializeComponent();
        }

        public void loadCharmsInPlace()
        {
            EditorWindow ewin = (EditorWindow)Window.GetWindow(this);
            TH1CharmExt _thisCharm;

            _thisCharm = ewin._save.charmsActive[0];
            comboAQuest.Items.Clear();
            charmALabel.Content = _thisCharm.charmName;
            charmALabel.ToolTip = _thisCharm.exDataString;
            if (_thisCharm.isEquip)
            {
                foreach (TH1CharmQuest _quest in _thisCharm.charm.questList)
                {
                    comboAQuest.Items.Add(_quest.questLongName);
                }
                comboAQuest.SelectedItem = _thisCharm.activeQuest.questLongName;
                sliderProgA.Maximum = _thisCharm.activeQuest.questTarget;
                sliderProgA.Value = _thisCharm.progress;
            }
            charmAImage.Source = _thisCharm.image;
            txtAValue.Text = _thisCharm.valueModifier.ToString();
            lblMutationA.Content = _thisCharm.mutationString;

            groupAMod.IsEnabled = _thisCharm.isEquip;
            comboAQuest.IsEnabled = _thisCharm.isEquip;
            sliderProgA.IsEnabled = _thisCharm.isEquip;
            btnUnequipA.IsEnabled = _thisCharm.isEquip;

            foreach (object _child in ((Grid)groupARunes.Content).Children)
            {
                if (_child is CheckBox)
                {
                    CheckBox _check = _child as CheckBox;
                    int tagno = int.Parse((string)_check.Tag);

                    if ((!_thisCharm.isEquip) || (tagno >= _thisCharm.charm.runeList.Count)) _check.Visibility = Visibility.Hidden;
                    else _check.Visibility = Visibility.Visible;

                    _check.IsChecked = (_thisCharm.isEquip) && (_thisCharm.runesReq[tagno]);
                }
            }

            _thisCharm = ewin._save.charmsActive[1];
            comboBQuest.Items.Clear();
            charmBLabel.Content = _thisCharm.charmName;
            charmBLabel.ToolTip = _thisCharm.exDataString;
            if (_thisCharm.isEquip)
            {
                foreach (TH1CharmQuest _quest in _thisCharm.charm.questList)
                {
                    comboBQuest.Items.Add(_quest.questLongName);
                }
                comboBQuest.SelectedItem = _thisCharm.activeQuest.questLongName;
                sliderProgB.Maximum = _thisCharm.activeQuest.questTarget;
                sliderProgB.Value = _thisCharm.progress;
            }
            charmBImage.Source = _thisCharm.image;
            txtBValue.Text = _thisCharm.valueModifier.ToString();
            lblMutationB.Content = _thisCharm.mutationString;

            groupBMod.IsEnabled = _thisCharm.isEquip;
            comboBQuest.IsEnabled = _thisCharm.isEquip;
            sliderProgB.IsEnabled = _thisCharm.isEquip;
            btnUnequipB.IsEnabled = _thisCharm.isEquip;

            foreach (object _child in ((Grid)groupBRunes.Content).Children)
            {
                if (_child is CheckBox)
                {
                    CheckBox _check = _child as CheckBox;
                    int tagno = int.Parse((string)_check.Tag);

                    if ((!_thisCharm.isEquip) || (tagno >= _thisCharm.charm.runeList.Count)) _check.Visibility = Visibility.Hidden;
                    else _check.Visibility = Visibility.Visible;

                    _check.IsChecked = (_thisCharm.isEquip) && (_thisCharm.runesReq[tagno]);
                }
            }
        }

        public void recountCharms()
        {
            lblCharmCount.Content = String.Format("{0}/{1} Charm{2}", dataInventry.Items.Count, new TH1Helper().LIMIT_MAX_CHARMS, dataInventry.Items.Count == 1 ? "" : "s");
        }

        private void showCharmEditorWindow(int index)
        {
            EditorWindow ewin = (EditorWindow)Window.GetWindow(this);
            TH1Helper _help = new TH1Helper();

            CharmEditorWindow dlg = new CharmEditorWindow();
            dlg.Owner = Window.GetWindow(this);
            dlg.charmIndex = index;
            dlg.charmCollection = ewin.db.charmCollection;

            if (index > -1)
            {
                dlg._thisCharm = (TH1CharmExt)dataInventry.Items[index];
                Functions.log(string.Format("Editing Charm #{0} ({1})", index, dlg._thisCharm.charmString), Functions.LC_PRIMARY);
            }
            else
            {
                if (ewin._save.charmsInventry.Count >= _help.LIMIT_MAX_CHARMS)
                {
                    MessageBox.Show(string.Format("You Have Reached The Maximum Number Of Charms Possible ({0})", _help.LIMIT_MAX_CHARMS));
                    Functions.log("Unable To Create New Charm As Inventry Is Full", Functions.LC_WARNING);
                    return;
                }
                Functions.log("Creating New Charm", Functions.LC_PRIMARY);
            }

            if (dlg.ShowDialog().Equals(true))
            {
                Functions.log("Saving Charm");
                /*
                 if ((dlg.thisRune.calcValue == 0) && (dlg.thisRune.valueModifier != 0))
                 {
                     Functions.log(string.Format("Value Modifier Caused The Rune Value To Go Out Of Bounds ({0})", dlg.thisRune.valueModifier), Functions.LC_WARNING);
                     dlg.thisRune.valueModifier = 0;
                 }
                 */

                dataInventry.ItemsSource = null;
                // Add Or Edit
                if (index > -1) ewin._save.charmsInventry[dlg.charmIndex] = dlg._thisCharm;
                else ewin._save.charmsInventry.Add(dlg._thisCharm);

                dataInventry.ItemsSource = ewin._save.charmsInventry;
                recountCharms();
            }
            else Functions.log("User Cancelled");
        }

        #region Event Handlers

        public void unequipItem(int index)
        {
            // Window
            EditorWindow ewin = (EditorWindow)Window.GetWindow(this);
            if (ewin == null) return;
            // Save
            var _save = ewin._save;
            if (_save == null) return;

            TH1Helper _help = new TH1Helper();

            try
            {
                if (_save.charmsInventry.Count >= _help.LIMIT_MAX_CHARMS)
                {
                    MessageBox.Show("Your Charm Inventry Is Full - You Cannot Unequip This Charm.", "Unequip Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                dataInventry.ItemsSource = null;
                _save.charmsActive[index].inActiveSlot = false; // No Longer Active
                _save.charmsInventry.Add(_save.charmsActive[index]);
                _save.charmsActive[index] = new TH1CharmExt(null);
                dataInventry.ItemsSource = _save.charmsInventry;
                recountCharms();
                loadCharmsInPlace();

            } catch { MessageBox.Show("There Was An Error When Trying To Unequip This Charm", "Unequip Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void btnUnequip_Click(object sender, RoutedEventArgs e)
        {
            // Window
            EditorWindow ewin = (EditorWindow)Window.GetWindow(this);
            if (ewin == null) return;

            // Save
            var _save = ewin._save;
            if (_save == null) return;

            // Button
            Button _button = sender as Button;
            if (_button == null) return;

            try
            {
                int activeRune = int.Parse((string)_button.Tag);
                unequipItem(activeRune);
            } catch { }

        }

        private void comboQuest_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EditorWindow ewin = (EditorWindow)Window.GetWindow(this);
            ComboBox _combo = sender as ComboBox;
            if ((_combo != null) && (_combo.SelectedIndex > -1))
            {
                int val = int.Parse((string)_combo.Tag);
                int index = _combo.SelectedIndex;
                TH1CharmExt _thisCharm = ewin._save.charmsActive[val];
                if (_thisCharm.isEquip)
                {
                    _thisCharm.activeQuestId = (uint)_thisCharm.charm.questList[index].questID;
                }
                if (val == 0)
                {
                    sliderProgA.Maximum = _thisCharm.target;
                    sliderProgA.Value = _thisCharm.progress;
                } else
                {
                    sliderProgB.Maximum = _thisCharm.target;
                    sliderProgB.Value = _thisCharm.progress;
                }
            }
        }

        private void chkRune_Click(object sender, RoutedEventArgs e)
        {
            // Window
            EditorWindow ewin = (EditorWindow)Window.GetWindow(this);
            if (ewin == null) return;

            // Save
            var _save = ewin._save;
            if (_save == null) return;

            // Check Box
            CheckBox _check = sender as CheckBox;
            if (_check == null) return;

            // Go
            try
            {
                int activeCharm = int.Parse((string)((Grid)_check.Parent).Tag);
                int rune = int.Parse((string)_check.Tag);
                bool value = _check.IsChecked.HasValue ? _check.IsChecked.Value : false;

                _save.charmsActive[activeCharm].runesReq[rune] = value;
            }
            catch { MessageBox.Show("Failed To Set Rune Value"); } // Meh

        }

        private void sliderProg_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Window
            EditorWindow ewin = (EditorWindow)Window.GetWindow(this);
            if (ewin == null) return;

            // Save
            var _save = ewin._save;
            if (_save == null) return;

            // Slider
            Slider _slide = sender as Slider;
            if (_slide == null) return;

            try
            {
                int activeCharm = int.Parse((string)_slide.Tag);
                _save.charmsActive[activeCharm].progress = (uint)_slide.Value;
            } catch { }
        }

        private void ValueModifier_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Window
            EditorWindow ewin = (EditorWindow)Window.GetWindow(this);
            if (ewin == null) return;

            // Save
            var _save = ewin._save;
            if (_save == null) return;

            // Slider
            TextBox _text = sender as TextBox;
            if (_text == null) return;

            try
            {
                int activeCharm = int.Parse((string)_text.Tag);
                _save.charmsActive[activeCharm].valueModifier = uint.Parse(_text.Text);
            }
            catch { }
        }

        private void btnAddCharm_Click(object sender, RoutedEventArgs e)
        {
            showCharmEditorWindow(-1);
        }

        private void DataGridRowInventry_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            showCharmEditorWindow(dataInventry.SelectedIndex);
        }

        private void mnuDuplicateSelected(object sender, RoutedEventArgs e)
        {
            EditorWindow ewin = (EditorWindow)Window.GetWindow(this);
            if (ewin == null) return;
            var _save = ewin._save;
            if (_save == null) return;

            TH1Helper _help = new TH1Helper();

            foreach (TH1CharmExt _charm in dataInventry.SelectedItems)
            {
                if (_save.charmsInventry.Count < _help.LIMIT_MAX_CHARMS)
                {
                    _save.charmsInventry.Add(_charm);
                }
            }

            dataInventry.ItemsSource = null;
            dataInventry.ItemsSource = _save.charmsInventry;
            recountCharms();
        }

        private void mnuCompleteSelected(object sender, RoutedEventArgs e)
        {
            EditorWindow ewin = (EditorWindow)Window.GetWindow(this);
            if (ewin == null) return;
            var _save = ewin._save;
            if (_save == null) return;

            TH1Helper _help = new TH1Helper();

            foreach (TH1CharmExt _charm in dataInventry.SelectedItems)
            {
                _charm.progress = (uint)_charm.target;
                for( int i = 0; i < _charm.runesReq.Count; i++)
                {
                    _charm.runesReq[i] = i < _charm.charm.runeList.Count;
                }
            }

            _save.charmsInventry = (List<TH1CharmExt>)dataInventry.ItemsSource;
            dataInventry.ItemsSource = null;
            dataInventry.ItemsSource = _save.charmsInventry;
            recountCharms();
        }

        private void mnuDeleteSelected(object sender, RoutedEventArgs e)
        {
            EditorWindow ewin = (EditorWindow)Window.GetWindow(this);
            if (ewin == null) return;
            var _save = ewin._save;
            if (_save == null) return;

            if (dataInventry.SelectedItems.Count < 1) return;
            if (MessageBox.Show(string.Format("Are You Sure You Want To Delete {0} Charm{1}", dataInventry.SelectedItems.Count, dataInventry.SelectedItems.Count == 1 ? "":"s"), "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            foreach (TH1CharmExt _charm in dataInventry.SelectedItems)
            {
                _save.charmsInventry.Remove(_charm);
            }

            dataInventry.ItemsSource = null;
            dataInventry.ItemsSource = _save.charmsInventry;
            recountCharms();
        }

        private void dataInventry_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            mnuSlot1.Header = "Slot 1";
            mnuSlot2.Header = "Slot 2";

            EditorWindow ewin = (EditorWindow)Window.GetWindow(this);
            if (ewin == null) return;
            var _save = ewin._save;
            if (_save == null) return;

            mnuSlot1.Header += " - " + _save.charmsActive[0].charmName;
            mnuSlot2.Header += " - " + _save.charmsActive[1].charmName;
        }

        private void mnuEquip(object sender, RoutedEventArgs e)
        {
            EditorWindow ewin = (EditorWindow)Window.GetWindow(this);
            if (ewin == null) return;
            var _save = ewin._save;
            if (_save == null) return;

            MenuItem _menu = sender as MenuItem;
            if( _menu != null && dataInventry.SelectedItem != null) try { 
                int activeCharm = int.Parse((string)_menu.Tag);
                    TH1CharmExt _selected = (TH1CharmExt)dataInventry.SelectedItem;
                    TH1CharmExt _tmpCharm = _save.charmsActive[activeCharm];

                    _save.charmsActive[activeCharm] = _selected;
                    _save.charmsActive[activeCharm].inActiveSlot = true; // Now Active
                    _save.charmsInventry.Remove(_selected);
                    if (_tmpCharm.isEquip)
                    {
                        _tmpCharm.inActiveSlot = false; // No Longer Active
                        _save.charmsInventry.Add(_tmpCharm);
                    }

                    dataInventry.ItemsSource = null;
                    dataInventry.ItemsSource = _save.charmsInventry;

                    recountCharms();
                    loadCharmsInPlace();
                } catch { MessageBox.Show("Unable To Equip That Charm", "Charm Equip Error", MessageBoxButton.OK, MessageBoxImage.Warning);  }
        }

        #endregion Event Handlers
    }
}
