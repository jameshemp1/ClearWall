using ClearWall.Services;
using System.Windows;
using System.Windows.Controls;

namespace ClearWall
{
    //User control logic for adding IP rules
    public partial class UserControlCreateRulesIP : UserControl
    {
        public UserControlCreateRulesIP()
        {
            InitializeComponent();
        }

        private void CreateRuleButton_Click(object sender, RoutedEventArgs e)
        {
            //Saves user entered fields to send to the firewall manager
            string protocol = (ProtocolComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string action = (ActionComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string direction = (DirectionComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string remoteAddress = RemoteAddressTextBox.Text.Trim();
            string incOrEx = (InclusiveExclusiveComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            //Check user input
            if (string.IsNullOrWhiteSpace(remoteAddress))
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
            //Only need to check action for null on inclusive rules
            if (incOrEx == "Inclusive")
            {
                if (string.IsNullOrWhiteSpace(action))
                {
                    MessageBox.Show("Please select an action (Allow or Block).", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            //Call the add ip rule function in FirewallManager
            try
            {
                FirewallManager.AddIPFirewallRule(
                    protocol: protocol,
                    action: action,
                    direction: direction,
                    remoteAddress: remoteAddress,
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
