using ClearWall.Models;
using PacketDotNet;
using SharpPcap;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace ClearWall
{
    //User control logic for displaying packets
    public partial class UserControlPacketCapture : UserControl
    {
        private ObservableCollection<PacketDetails> PacketList { get; set; }
        private CaptureDeviceList devices;
        private bool capturing = false;
        private ConcurrentQueue<PacketDetails> packetQueue = new ConcurrentQueue<PacketDetails>();
        private readonly DispatcherTimer uiUpdateTimer;

        public UserControlPacketCapture()
        {
            InitializeComponent();
            //list of packets for displaying in datagrid
            PacketList = new ObservableCollection<PacketDetails>();
            PacketDataGrid.ItemsSource = PacketList;
            //Double click mapping for popout payload window
            PacketDataGrid.MouseDoubleClick += PacketDataGrid_MouseDoubleClick;
            //Timer for displaying packets smoothly
            uiUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            uiUpdateTimer.Tick += UiUpdateTimer_Tick;
        }
        //display 1 packet in queue each timer tick
        private void UiUpdateTimer_Tick(object sender, EventArgs e)
        {
            List<PacketDetails> packetsToAdd = new List<PacketDetails>();

            while (packetQueue.TryDequeue(out var packetInfo))
            {
                packetsToAdd.Add(packetInfo);
            }

            if (packetsToAdd.Count > 0)
            {
                // Update the UI
                AddPacket(packetsToAdd.First());
                packetsToAdd.Remove(packetsToAdd.First());
            }
        }
        //pop out window on double click
        private void PacketDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (PacketDataGrid.SelectedItem is PacketDetails selectedPacket)
            {
                var detailsWindow = new PayloadWindow(selectedPacket);
                detailsWindow.Show();
            }
        }

        public void StartCapture(int interfaceIndex)
        {
            if (capturing)
                return;

            capturing = true;
            devices = CaptureDeviceList.Instance;

            if (interfaceIndex == -1) // All interfaces
            {
                //start capture on each interface
                foreach (var dev in devices)
                {
                    //configure interface to call function for handling incoming packets
                    dev.OnPacketArrival += DeviceOnPacketArrival;
                    dev.Open(DeviceModes.Promiscuous | DeviceModes.NoCaptureLocal, read_timeout: 1000);
                    dev.StartCapture();
                }
            }
            else
            {
                //start capture on selected interface
                if (interfaceIndex >= 0 && interfaceIndex < devices.Count)
                {
                    var dev = devices[interfaceIndex];
                    dev.OnPacketArrival += DeviceOnPacketArrival;
                    dev.Open(DeviceModes.Promiscuous | DeviceModes.NoCaptureLocal, read_timeout: 1000);
                    dev.StartCapture();
                }
            }
            //start the timer for displaying packets
            uiUpdateTimer.Start();
        }

        public void StopCapture()
        {
            if (!capturing)
                return;

            capturing = false;
            //stop capture on each actively capturing interface
            foreach (var dev in devices)
            {
                if (dev.Started)
                {
                    dev.StopCapture();
                    dev.Close();
                    //remove packet arrival logic for interface
                    dev.OnPacketArrival -= DeviceOnPacketArrival;
                }
            }
            uiUpdateTimer.Stop();

            // Clear any remaining packets in the queue
            packetQueue = new ConcurrentQueue<PacketDetails>();
        }

        private void DeviceOnPacketArrival(object sender, PacketCapture e)
        {
            try
            {
                var rawPacket = e.GetPacket(); //Retrieve the Raw Capture object
                var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);

                var ipPacket = packet.Extract<IPPacket>();
                if (ipPacket != null)
                {
                    ProcessIpPacket(ipPacket, rawPacket);
                }
                else
                {
                    ProcessNonIpPacket(packet, rawPacket);
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => MessageBox.Show($"Error parsing packet: {ex.Message}"));
            }
        }

        //Clear captured packets
        public void ClearPackets()
        {
            PacketList.Clear();
        }

        //Add packet to display list
        private void AddPacket(PacketDetails packet)
        {
            PacketList.Add(packet);
        }

        //process IP packets
        private void ProcessIpPacket(IPPacket ipPacket, RawCapture rawPacket)
        {
            string payloadData = string.Empty;
            //extract payload data using the protocols provided method if available
            if (ipPacket.Extract<TcpPacket>() is TcpPacket tcpPacket)
            {
                payloadData = GetPayloadData(tcpPacket.PayloadData);
            }
            else if (ipPacket.Extract<UdpPacket>() is UdpPacket udpPacket)
            {
                payloadData = GetPayloadData(udpPacket.PayloadData);
            }
            else
            {
                payloadData = GetPayloadData(ipPacket.PayloadData);
            }
            //Create PacketDetails model for the packet
            var packetInfo = new PacketDetails
            {
                Timestamp = rawPacket.Timeval.Date,
                SourceIP = ipPacket.SourceAddress.ToString(),
                DestinationIP = ipPacket.DestinationAddress.ToString(),
                Protocol = ipPacket.Protocol.ToString(),
                Length = ipPacket.TotalLength,
                Payload = payloadData
            };
            //Add packet to queue to be displayed
            packetQueue.Enqueue(packetInfo);
        }

        //process non IP packets
        private void ProcessNonIpPacket(Packet packet, RawCapture rawPacket)
        {
            //Determine the type of packet
            string protocol = GetProtocolEth(packet.Extract<EthernetPacket>().Type);
            //type cast packet to use EthernetPacket methods
            var ePacket = (EthernetPacket)packet;
            var source = ePacket.SourceHardwareAddress.ToString();
            var destination = ePacket.DestinationHardwareAddress.ToString();

            var packetInfo = new PacketDetails
            {
                Timestamp = rawPacket.Timeval.Date,
                SourceIP = source,
                DestinationIP = destination,
                Protocol = protocol,
                Length = packet.TotalPacketLength,
                Payload = GetPayloadData(packet.PayloadData)
            };
            packetQueue.Enqueue(packetInfo);
        }

        //Return protocol text from numeric value (
        private string GetProtocolEth(EthernetType type)
        {
            switch (type)
            {
                case EthernetType.None:
                    return "None";
                case EthernetType.IPv4:
                    return "IPv4";
                case EthernetType.Arp:
                    return "ARP";
                case EthernetType.WakeOnLan:
                    return "Wake-on-LAN";
                case EthernetType.ReverseArp:
                    return "RARP";
                case EthernetType.VLanTaggedFrame:
                    return "VLAN Tagged Frame";
                case EthernetType.IPv6:
                    return "IPv6";
                case EthernetType.Lldp:
                    return "LLDP";
                case EthernetType.NovellIpx:
                    return "Novell IPX";
                case EthernetType.Loop:
                    return "Loop";
                //cases for protocols not included in PacketDotNet.Packet
                case (EthernetType)36864:
                    return "Loopback";
                case (EthernetType)38:
                    return "IDRP";
                default:
                    return type.ToString(); //Use raw name if no specific mapping
            }
        }

        //Attempt to decode payload
        private string GetPayloadData(byte[] payloadData)
        {
            if (payloadData == null || payloadData.Length == 0)
            {
                return string.Empty;
            }

            try
            {
                return System.Text.Encoding.ASCII.GetString(payloadData);
            }
            catch
            {
                //If decoding fails, return hex string
                return BitConverter.ToString(payloadData).Replace("-", " ");
            }
        }
    }
}
