using ClearWall.Models;
using System.Windows;

namespace ClearWall
{
    public partial class PayloadWindow : Window
    {
       
        public PayloadWindow(PacketDetails packetInfo)
        {
            InitializeComponent();
            //Binds data context to the selected packet's PacketDetails
            DataContext = packetInfo;
        }
    }
}
