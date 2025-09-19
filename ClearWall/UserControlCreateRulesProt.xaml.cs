using ClearWall.Services;
using System.Windows;
using System.Windows.Controls;

namespace ClearWall
{
    //User control logic for adding protocol rules
    public partial class UserControlCreateRulesProt : UserControl
    {
        public UserControlCreateRulesProt()
        {
            InitializeComponent();
        }

        private void CreateRuleButton_Click(object sender, RoutedEventArgs e)
        {
            //saves user entred data to send to FireWallManager
            string protocol = (ProtocolComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string action = (ActionComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string direction = (DirectionComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string incOrEx = (InclusiveExclusiveComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            //Check user input
            if (string.IsNullOrWhiteSpace(protocol))
            {
                MessageBox.Show("Please select an IP address.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            if (string.IsNullOrWhiteSpace(direction))
            {
                MessageBox.Show("Please select a direction (Inbound or Outbound).", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(incOrEx))
            {
                MessageBox.Show("Please select a inclusivity (Inclusive or Exclusive).", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //Only check action on inclusive rule
            if (incOrEx == "Inclusive")
            {
                if (string.IsNullOrWhiteSpace(action))
                {
                    MessageBox.Show("Please select an action (Allow or Block).", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            //Call the add protocol rule function in FirewallManager
            try
            {
                FirewallManager.AddProtFirewallRule(
                    protocol: protocol,
                    action: action,
                    direction: direction,
                    incEx: incOrEx
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating firewall rule: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}