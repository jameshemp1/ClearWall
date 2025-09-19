using ClearWall.Services;
using System.Windows;
using System.Windows.Controls;

namespace ClearWall
{
    //User control logic for deleting rules
    public partial class UserControlDeleteRule : UserControl
    {
        public UserControlDeleteRule()
        {
            InitializeComponent();
        }
        private void DeleteRuleButton_Click(object sender, RoutedEventArgs e)
        {
            //variable for user input
            string name = RuleName.Text.Trim();

            //Check user input
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please Input a Name", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //Call the delete rull function in FirewallManager
            try
            {
                FirewallManager.DeleteFWRule(
                    ruleName: name
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting firewall rule: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
