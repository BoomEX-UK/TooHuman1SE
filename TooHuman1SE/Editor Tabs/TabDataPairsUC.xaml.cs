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
            KeyValuePair<string, uint> kvpA;
            KeyValuePair<string, float> kvpB;
            DataPairEditorWindow dpe = new DataPairEditorWindow();
            dpe.Owner = Window.GetWindow(this);

            switch (activeTab)
            {
                case 1:
                    kvpB = (KeyValuePair<string, float>)dataPairsB.SelectedItem;
                    dpe.dataPairB = (Dictionary<string, float>)dataPairsB.ItemsSource;
                    dpe.activeKey = kvpB.Key;
                    Functions.log("Loading Value Editor float(" + kvpB.Key + ")");
                    break;
                default:
                    kvpA = (KeyValuePair<string, uint>)dataPairsA.SelectedItem;
                    dpe.dataPairA = (Dictionary<string, uint>)dataPairsA.ItemsSource;
                    dpe.activeKey = kvpA.Key;
                    Functions.log("Loading Value Editor uint(" + kvpA.Key + ")");
                    break;
            }
            
            if ( dpe.ShowDialog().Equals(true))
            {
                dataPairsA.ItemsSource = null;
                dataPairsB.ItemsSource = null;

                EditorWindow par = (EditorWindow)Window.GetWindow(this);
                if (dpe.pairTypeA)
                {
                    KeyValuePair<string, uint> edit = (KeyValuePair<string, uint>)dpe.Tag;
                    par._save.character.dataPairsA[edit.Key] = edit.Value;
                    Functions.log("Edit Made uint(" + edit.Key + "," + edit.Value.ToString() + ")");
                }
                else
                {
                    KeyValuePair<string, float> edit = (KeyValuePair<string, float>)dpe.Tag;
                    par._save.character.dataPairsB[edit.Key] = edit.Value;
                    Functions.log("Edit Made float(" + edit.Key + "," + edit.Value.ToString() + ")");
                }

                dataPairsA.ItemsSource = par._save.character.dataPairsA;
                dataPairsB.ItemsSource = par._save.character.dataPairsB;
                
            }
            else Functions.log("User Cancelled");
        }
    }
}
