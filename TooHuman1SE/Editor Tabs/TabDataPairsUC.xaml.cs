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
using TooHuman1SE.SEFunctions;

namespace TooHuman1SE.Editor_Tabs
{
    /// <summary>
    /// Interaction logic for TabDataPairsUC.xaml
    /// </summary>
    public partial class TabDataPairsUC : UserControl
    {
        public TabDataPairsUC()
        {
            InitializeComponent();
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int activeTab = tabDataPairs.SelectedIndex;
            KeyValuePair<string, uint> kvp;
            DataPairEditorWindow dpe = new DataPairEditorWindow();
            dpe.Owner = Window.GetWindow(this);

            switch (activeTab)
            {
                case 1:
                    kvp = (KeyValuePair<string, uint>)dataPairsB.SelectedItem;
                    dpe.dataPair = (Dictionary<string, uint>)dataPairsB.ItemsSource;
                    break;
                default:
                    kvp = (KeyValuePair<string, uint>)dataPairsA.SelectedItem;
                    dpe.dataPair = (Dictionary<string, uint>)dataPairsA.ItemsSource;
                    break;
            }
            
            dpe.activeKey = kvp.Key;

            Functions.log("Loading Value Editor (" + kvp.Key + ")");
            if ( dpe.ShowDialog().Equals(true))
            {
                dataPairsA.ItemsSource = null;
                dataPairsB.ItemsSource = null;

                EditorWindow par = (EditorWindow)Window.GetWindow(this);
                KeyValuePair <string, uint> edit = (KeyValuePair<string, uint>)dpe.Tag;

                switch(activeTab)
                {
                    case 1:
                        par._save.character.dataPairsB[edit.Key] = edit.Value;
                        break;
                    default:
                        par._save.character.dataPairsA[edit.Key] = edit.Value;
                        break;
                }
                
                dataPairsA.ItemsSource = par._save.character.dataPairsA;
                dataPairsB.ItemsSource = par._save.character.dataPairsB;
                Functions.log("Edit Made (" + edit.Key + "," + edit.Value.ToString() + ")");
            }
            else Functions.log("User Cancelled");
        }
    }
}
