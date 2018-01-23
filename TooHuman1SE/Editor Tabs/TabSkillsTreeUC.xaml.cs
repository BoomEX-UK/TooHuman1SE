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

namespace TooHuman1SE.Editor_Tabs
{
    /// <summary>
    /// Interaction logic for TabSkillsTreeUC.xaml
    /// </summary>
    public partial class TabSkillsTreeUC : UserControl
    {
        public TabSkillsTreeUC()
        {
            InitializeComponent();
        }

        private void txt_NumberOnly(object sender, TextCompositionEventArgs e)
        {
            try { int.Parse(e.Text);
            } catch { e.Handled = true; }
        }

    }
}
