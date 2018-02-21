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

namespace TooHuman1SE.Windows
{
    /// <summary>
    /// Interaction logic for RuneEditorWindow.xaml
    /// </summary>
    public partial class RuneEditorWindow : Window
    {
        public TH1RuneMExt thisRune;
        public TH1RuneMCollection runeCollection;
        public int runeIndex = -1;

        public RuneEditorWindow()
        {
            InitializeComponent();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            // On Show Event Stuff Goes Below..
            buildDropDowns();
            setupForm();
        }

        private void buildDropDowns()
        {
            // Rune Colour
            string[] colourNames = new TH1Helper().runeColourNameArray;
            comboColour.Items.Clear();
            comboColour.ItemsSource = colourNames;

            // Paint Colour
            string[] paintColours = new TH1PaintCollection().paintNameArray();
            comboPaint.Items.Clear();
            comboPaint.ItemsSource = paintColours;
        }

        private void setupForm()
        {

            if (runeIndex > -1)
            {
                Functions.log("Loading Rune Editor (" + runeIndex.ToString() +")");
                Window.GetWindow(this).Title = String.Format("Edit Rune #{0}", runeIndex + 1);

                comboColour.SelectedItem = thisRune.rune.runeColourName;
                comboBonus.SelectedItem = thisRune.rune.idLevelAndBonusType;              
                chkUnknown.IsChecked = (thisRune.dataB == 1);
                checkPurchased.IsChecked = (thisRune.purchased == 1);
                txtBaseValue.Text = thisRune.valueModifier.ToString();
                txtD.Text = thisRune.dataD.ToString();
                comboPaint.SelectedItem = thisRune.paint.paintName;
            }
            else
            {
                Functions.log("Loading Rune Editor (New Rune)");
                Window.GetWindow(this).Title = "New Rune";
                comboColour.SelectedIndex = 0;
                comboBonus.SelectedIndex = 0;
                comboPaint.SelectedIndex = 0;
                checkPurchased.IsChecked = false;
                chkUnknown.IsChecked = false;
                txtBaseValue.Text = "10000";
                txtD.Text = "0";
            }

        }

        private void txt_NumberOnly(object sender, TextCompositionEventArgs e)
        {
            try
            {
                int.Parse(e.Text);
            }
            catch { e.Handled = true; }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            setupForm();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            MainWindow _mwin = (MainWindow)((EditorWindow)this.Owner)._mainWindow;
                ;
            TH1PaintCollection _paintCollection = new TH1PaintCollection();
            string[] runeParts = ((string)comboBonus.SelectedItem).Split(':');
            TH1RuneM newRune = runeCollection.findRune(new TH1Helper().getRuneColourID((string)comboColour.SelectedValue), int.Parse(runeParts[0]));

            thisRune = new TH1RuneMExt(newRune);
            try { thisRune.purchased = (uint)((bool)checkPurchased.IsChecked ? 1 : 0); } catch { thisRune.purchased = 0; }
            try { thisRune.dataB = (uint)((bool)chkUnknown.IsChecked ? 1 : 0); } catch { thisRune.purchased = 0; }
            try { thisRune.valueModifier = uint.Parse(txtBaseValue.Text); } catch { thisRune.valueModifier = 10000; }
            try { thisRune.dataD = uint.Parse(txtD.Text); } catch { thisRune.dataD = 0; }
            try { thisRune.paint = _paintCollection.findPaint((string)comboPaint.SelectedItem); } catch { thisRune.paint = new TH1Paint(); }

            this.DialogResult = true;
        }

        private void btnRandom_Click(object sender, RoutedEventArgs e)
        {
            Random r = new Random();
            comboBonus.SelectedIndex = r.Next(comboBonus.Items.Count);
        }

        private void comboColour_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainWindow _mwin = (MainWindow)((EditorWindow)this.Owner)._mainWindow;

            comboBonus.ItemsSource = runeCollection.idLevelAndBonusArray(new TH1Helper().getRuneColourID((string)comboColour.SelectedValue));
            comboBonus.SelectedIndex = 0;
        }

        private void comboBonus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBonus.SelectedItem != null)
            {
                MainWindow _mwin = (MainWindow)((EditorWindow)this.Owner)._mainWindow;

                string[] runeParts = ((string)comboBonus.SelectedItem).Split(':');
                TH1RuneM _rune = runeCollection.findRune(new TH1Helper().getRuneColourID((string)comboColour.SelectedValue), int.Parse(runeParts[0]));

                imgRune.Source = _rune.runeImage;
                lblRune.ToolTip = _rune.longName;
                lblRune.Content = lblRune.ToolTip;

                comboPaint.IsEnabled = comboBonus.SelectedItem.ToString().Contains("Colour Module");
                if (!comboPaint.IsEnabled) comboPaint.SelectedIndex = 0;
                
            }
        }
    }
}
