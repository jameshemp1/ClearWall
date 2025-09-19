using ClearWall.ViewModels;
using System.Windows.Controls;

namespace ClearWall
{
    //user control logic for showing rules
    public partial class UserControlShowRules : UserControl
    {
        public UserControlShowRules()
        {
            InitializeComponent();
            this.DataContext = new FirewallRulesViewModel();
        }
    }
}
