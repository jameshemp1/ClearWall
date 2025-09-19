namespace ClearWall.Models
{
    //Model for Storing Packet Details
    public class PacketDetails
    {
        public DateTime Timestamp { get; set; }
        public string SourceIP { get; set; }
        public string DestinationIP { get; set; }
        public string Protocol { get; set; }
        public int Length { get; set; }
        public string Payload { get; set; }

    }
}
