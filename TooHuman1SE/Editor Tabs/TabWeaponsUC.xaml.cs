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
    }
}
