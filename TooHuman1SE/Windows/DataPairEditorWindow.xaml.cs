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

namespace TooHuman1SE.Windows
{
    /// <summary>
    /// Interaction logic for DataPairEditorWindow.xaml
    /// </summary>
    public partial class DataPairEditorWindow : Window
    {

        public Dictionary<string, uint> dataPairA = new Dictionary<string, uint>();
        public Dictionary<string, float> dataPairB = new Dictionary<string, float>();
        public string activeKey;
        public bool pairTypeA = true;

        public DataPairEditorWindow()
        {
            InitializeComponent();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            // On Show Event Stuff Goes Below..
            setSelectors();
            FocusManager.SetFocusedElement(this, txtValue);
            txtValue.SelectAll();
        }

        private void setSelectors()
        {
            comboStat.Items.Clear();
            if (dataPairA.Count > 0)
            {
                foreach( KeyValuePair<string,uint> kvp in dataPairA)
                {
                    comboStat.Items.Add(kvp.Key);
                }
                pairTypeA = true;
            }
            else if (dataPairB.Count > 0)
            {
                foreach( KeyValuePair<string,float> kvp in dataPairB)
                {
                    comboStat.Items.Add(kvp.Key);
                }
                pairTypeA = false;
            }
            else DialogResult = false;

            for (int i = 0; i < comboStat.Items.Count; i++) if ((string)comboStat.Items[i] == activeKey) comboStat.SelectedIndex = i;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            object res;

            if (pairTypeA)
            {
                uint activeValue;
                try { activeValue = uint.Parse(txtValue.Text); }
                catch { activeValue = 0; }
                res = new KeyValuePair<string, uint>(activeKey, activeValue);
            } else
            {
                float activeValue;
                try { activeValue = float.Parse(txtValue.Text); }
                catch { activeValue = 0; }
                res = new KeyValuePair<string, float>(activeKey, activeValue);
            }

            this.DialogResult = true;
            this.Tag = res;
        }

        private void comboStat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            activeKey = comboStat.SelectedValue.ToString();

            if (pairTypeA)
            {
                uint outval = 0;
                if (dataPairA.TryGetValue(activeKey, out outval)) txtValue.Text = outval.ToString();
                else txtValue.Text = "0";
            } else
            {
                float outval = 0;
                if (dataPairB.TryGetValue(activeKey, out outval)) txtValue.Text = outval.ToString();
                else txtValue.Text = "0";
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

        private void txtValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) btnOK_Click(btnOK, new RoutedEventArgs());
        }
    }
}
