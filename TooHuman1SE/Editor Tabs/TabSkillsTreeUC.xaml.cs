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

        private void skillsTreeChanged(object sender, TextChangedEventArgs e)
        {
            EditorWindow ewin = (EditorWindow)Window.GetWindow(this);
            if (ewin == null) return;
            var _save = ewin._save;
            if (_save == null) return;

            TextBox _text = sender as TextBox;
            if(_text != null) try
            {
                int tagno = int.Parse((string)_text.Tag);
                _save.skills.pairs[tagno].first = Math.Min(int.Parse(_text.Text), _save.skills.LIMIT_SKILL_MAX);
            } catch { }
        }
    }
}
