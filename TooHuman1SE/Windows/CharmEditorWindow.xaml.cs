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
using TooHuman1SE.Windows;

namespace TooHuman1SE.Windows
{
    /// <summary>
    /// Interaction logic for CharmEditorWindow.xaml
    /// </summary>
    public partial class CharmEditorWindow : Window
    {
        public TH1CharmEx _thisCharm;
        public TH1CharmEx _originalCharm;
        public int charmIndex = -1;
        TH1CharmCollection _charms = new TH1CharmCollection();

        public CharmEditorWindow()
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
            // Charm Colour
            string[] charmTiers = new TH1Helper().charmTierNameArray;
            comboTier.ItemsSource = null;
            comboTier.ItemsSource = charmTiers;
        }

        private void setupForm( bool initial )
        {
            if( charmIndex == -1 ) comboTier.SelectedIndex = 0; // New Charm
            if( initial ) _originalCharm = _thisCharm;
            lblCharm.ToolTip = _originalCharm.exDataString; // temp
            comboTier.SelectedIndex = _originalCharm.charmLevel - 1;
            comboCharms.SelectedItem = _originalCharm.charmLongName;
            comboQuests.SelectedItem = _originalCharm.activeQuest.questLongName;
            txtValue.Text = _originalCharm.valueModifier.ToString();
            sliderProgress.Maximum = _originalCharm.target;
            sliderProgress.Value = _originalCharm.progress;

            foreach (Control ctrl in ((Grid)groupBeta.Content).Children)
            {
                TextBox _text = ctrl as TextBox;
                if (_text != null)
                {
                    try
                    {
                        int _tag = int.Parse((string)_text.Tag);
                        switch (_tag)
                        {
                            case 2:
                                _text.Text = _originalCharm.val2.ToString();
                                break;
                            case 8:
                                _text.Text = _originalCharm.val8.ToString();
                                break;
                        }
                    }
                    catch { }
                }
            }

            if (initial) _thisCharm.runesReq = _originalCharm.runesReq;
            foreach (Control ctrl in ((Grid)groupRunes.Content).Children)
            {
                CheckBox _check = ctrl as CheckBox;
                if (_check != null)
                {
                    try
                    {
                        int _tag = int.Parse((string)_check.Tag);
                        _check.IsChecked = _originalCharm.runesReq[_tag];
                    }
                    catch { }
                }
            }
        }

        private void comboColour_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboTier.SelectedIndex > -1)
            {
                string[] charmsByTier = _charms.charmLongNamesByLevel(comboTier.SelectedIndex + 1);
                comboCharms.ItemsSource = null;
                comboCharms.ItemsSource = charmsByTier;
                comboCharms.SelectedIndex = 0;
            }
        }

        private void btnRandomCharm_Click(object sender, RoutedEventArgs e)
        {
            Random r = new Random();
            comboCharms.SelectedIndex = r.Next(comboCharms.Items.Count - 1);
        }

        private void comboCharms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboCharms.SelectedIndex > -1)
            {
                _thisCharm = new TH1CharmEx(_charms.findCharmByLongName((string)comboCharms.SelectedItem));
                imgCharm.Source = _thisCharm.image;
                lblCharm.Content = _thisCharm.charmLongName;
                comboQuests.ItemsSource = null;
                comboQuests.ItemsSource = _thisCharm.questsLongNameArray;
                comboQuests.SelectedItem = _thisCharm.questLongName;

                int maxRunes = _thisCharm.charm.runeList.Count;
                _thisCharm.runesReq = new List<bool>(new bool[8]);

                foreach (Control ctrl in ((Grid)groupRunes.Content).Children)
                {
                    CheckBox _check = ctrl as CheckBox;
                    if (_check != null)
                    {
                        try
                        {
                            int _tag = int.Parse((string)_check.Tag);
                            _check.Visibility = (_tag < maxRunes) ? Visibility.Visible : Visibility.Hidden;
                            _check.IsChecked = false;
                        }
                        catch { }
                    }
                }
            }
        }

        private void txtValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox _text = sender as TextBox;

            if( _text != null ) try
            {
                    uint valueMod = uint.Parse(_text.Text);
                    _thisCharm.valueModifier = valueMod;
            } catch { _text.Text = "0"; }
        }

        private void txt_NumberOnly(object sender, TextCompositionEventArgs e)
        {
            try
            {
                int.Parse(e.Text);
            }
            catch { e.Handled = true; }
        }

        private void comboQuests_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox _combo = sender as ComboBox;
            if (_combo != null && _combo.SelectedIndex > -1)
            {
                _thisCharm.activeQuestId = (uint)_thisCharm.charm.questList[_combo.SelectedIndex].questID;
                sliderProgress.Maximum = _thisCharm.target;
                sliderProgress.Value = 0;
            }
        }

        private void txtBeta_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox _text = sender as TextBox;
            if (_text != null) try
                {
                    uint _uint = uint.Parse(_text.Text);
                    switch (int.Parse((string)_text.Tag))
                    {
                        case 2: _thisCharm.val2 = _uint;
                            break;
                        case 8: _thisCharm.val8 = _uint;
                            break;
                    }
                } catch { _text.Text = "0"; }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            setupForm(false);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void sliderProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider _slider = sender as Slider;
            if( _slider != null)
            {
                _thisCharm.progress = (uint)_slider.Value;
            }
        }

        private void chkRune_Click(object sender, RoutedEventArgs e)
        {
            CheckBox _check = sender as CheckBox;
            if( _check != null)
            {
                try
                {
                    int _tag = int.Parse((string)_check.Tag);
                    bool _checked = _check.IsChecked.HasValue ? _check.IsChecked.Value : false;
                    _thisCharm.runesReq[_tag] = _checked;
                }
                catch { }
            }
        }
    }
}
