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
using System.IO;
using System.Windows.Shapes;
using TooHuman1SE.Windows;
using TooHuman1SE.SEStructure;

namespace TooHuman1SE.Editor_Tabs
{
    /// <summary>
    /// Interaction logic for TabSectorsUC.xaml
    /// </summary>
    public partial class TabSectorsUC : UserControl
    {
        public TabSectorsUC()
        {
            InitializeComponent();
        }

        private void mnuExtractSelected_Click(object sender, RoutedEventArgs e)
        {
            if (dataSectors.SelectedItems.Count > 0) {
                EditorWindow ewin = (EditorWindow)Window.GetWindow(this);
                string name = ewin._save.character.name;

                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = string.Format("{0}", name);
                dlg.DefaultExt = "";
                dlg.Filter = "Sector Data|*";

                Nullable<bool> result = dlg.ShowDialog();
                if (result == true)
                {
                    string filename = dlg.FileName;
                    foreach (TH1Sector _sector in dataSectors.SelectedItems)
                    {
                        ewin._save.writeSectorToFile(string.Format("{0}-{1}-{2}.bin", filename, _sector.id.ToString().PadLeft(3,'0'), _sector.sectorName), (int)_sector.id,false);
                    }
                }
            }
            dataSectors.UnselectAll();
        }

        private void mnuExtractAll_Click(object sender, RoutedEventArgs e)
        {
            dataSectors.SelectAll();
            mnuExtractSelected_Click(sender, e);
        }
    }
}
