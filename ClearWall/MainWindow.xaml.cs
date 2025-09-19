using System.Windows;

namespace ClearWall
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            LoadInterfaces();
        }

        //Content control for Firewall tab
        private void RulesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new UserControlShowRules();
        }
        private void CreateIPMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new UserControlCreateRulesIP();
        }
        private void CreateProtMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new UserControlCreateRulesProt();
        }
        private void DeleteRuleMenuItem_Click(Object sender, RoutedEventArgs e)
        {
            MainContent.Content = new UserControlDeleteRule();
        }

        //Load interfaces into selection drop down
        private void LoadInterfaces()
        {
            var devices = SharpPcap.CaptureDeviceList.Instance;

            InterfaceComboBox.Items.Add("All Interfaces");
            foreach (var dev in devices)
            {
                InterfaceComboBox.Items.Add(dev.Description);
            }
            InterfaceComboBox.SelectedIndex = 0;
        }

        //Button click logic for starting and stopping capture
        private void CaptureToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (CaptureToggleButton.IsChecked == true)
            {
                CaptureToggleButton.Content = "Stop Capture";
                int interfaceIndex = InterfaceComboBox.SelectedIndex - 1; // Adjust for "All Interfaces"
                PacketDisplay.StartCapture(interfaceIndex);
            }
            else
            {
                CaptureToggleButton.Content = "Start Capture";
                PacketDisplay.StopCapture();
            }
        }

        //Clear captured packets
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            PacketDisplay.ClearPackets();
        }
    }
}
