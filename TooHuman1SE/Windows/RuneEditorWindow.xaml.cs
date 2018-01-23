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
        public TH1Rune thisRune = new TH1Rune();
        public int runeIndex = -1;

        public RuneEditorWindow()
        {
            InitializeComponent();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            // On Show Event Stuff Goes Below..
            setupForm();
            // FocusManager.SetFocusedElement(this, txtValue);
            // txtValue.SelectAll();
        }

        private void setupForm()
        {
            string[] colourNames = thisRune.getColourNames;
            comboColour.Items.Clear();

            foreach (string colour in colourNames) comboColour.Items.Add(colour);

            if (runeIndex > -1)
            {
                Functions.log("Loading Rune Editor (" + runeIndex.ToString() +")");
                Window.GetWindow(this).Title = String.Format("Edit Rune #{0}", runeIndex + 1);
                comboColour.SelectedIndex = thisRune.runeColour - 1;
                txtID.Text = thisRune.runeID.ToString();
                txtB.Text = thisRune.dataB.ToString();
                checkPurchased.IsChecked = (thisRune.purchased == 1);
                txtBaseValue.Text = thisRune.baseValue.ToString();
                txtD.Text = thisRune.dataD.ToString();
                txtPaint.Text = thisRune.paintID.ToString();
            }
            else
            {
                Functions.log("Loading Rune Editor (New Rune)");
                Window.GetWindow(this).Title = "New Rune";
                comboColour.SelectedIndex = 0;
                txtID.Text = "1";
                checkPurchased.IsChecked = false;
                txtB.Text = "0";
                txtBaseValue.Text = "10000";
                txtD.Text = "0";
                txtPaint.Text = "1";
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
            Random r = new Random();
            
            thisRune.runeColour = comboColour.SelectedIndex + 1;
            try {
                thisRune.runeID = int.Parse(txtID.Text);
                if (thisRune.runeID < 1) thisRune.runeID = 1;
            } catch { thisRune.runeID = r.Next(1, 999); }

            try { thisRune.purchased = (uint)((bool)checkPurchased.IsChecked ? 1 : 0); } catch { thisRune.purchased = 0; }
            try { thisRune.dataB = uint.Parse(txtB.Text); } catch { thisRune.dataB = 0; }
            try { thisRune.baseValue = uint.Parse(txtBaseValue.Text); } catch { thisRune.baseValue = 10000; }
            try { thisRune.dataD = uint.Parse(txtD.Text); } catch { thisRune.dataD = 0; }
            try { thisRune.paintID = uint.Parse(txtPaint.Text); } catch { thisRune.paintID = 1; }

            this.DialogResult = true;
        }

        private void btnRandom_Click(object sender, RoutedEventArgs e)
        {
            Random r = new Random();
            txtID.Text = r.Next(1, 999).ToString();
        }
    }
}
