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
    /// Interaction logic for TabCharacterUC.xaml
    /// </summary>
    public partial class TabCharacterUC : UserControl
    {
        public TabCharacterUC()
        {
            InitializeComponent();
        }

        private void txt_NumberOnly(object sender, TextCompositionEventArgs e)
        {
            try
            {
                int.Parse(e.Text);
            }
            catch { e.Handled = true; }
        }

        private void txtCBounty_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditorWindow ewin = (EditorWindow)Window.GetWindow(this);
            if (ewin == null) return;
            var _save = ewin._save;

            TextBox _tb = sender as TextBox;
            if (_tb != null && _save != null)
            {
                try
                {
                    _save.character.bounty = long.Parse(_tb.Text);
                }
                catch { _save.character.bounty = 0; }
            }
        }

        private void txtCExp_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditorWindow ewin = (EditorWindow)Window.GetWindow(this);
            if (ewin == null) return;
            var _save = ewin._save;

            TextBox _tb = sender as TextBox;
            if (_tb != null && _save != null)
            {
                try
                {
                    TH1ExpToNextLevel ex = new TH1ExpToNextLevel();
                    _save.character.exp = long.Parse(_tb.Text);
                    _save.character.level = ex.calcLevel(_save.character.exp);
                }
                catch { _save.character.bounty = 0; _save.character.level = 1; }
            }
        }

        private void txtCSkillPoints_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditorWindow ewin = (EditorWindow)Window.GetWindow(this);
            if (ewin == null) return;
            var _save = ewin._save;

            TextBox _tb = sender as TextBox;
            if (_tb != null && _save != null)
            {
                try
                {
                    _save.character.skillPoints = long.Parse(_tb.Text);
                }
                catch { _save.character.skillPoints = 0; }
            }
            
        }
    }
}
