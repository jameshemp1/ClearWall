namespace ClearWall.Models
{
    //Model for rule data
    internal class FirewallRulesModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Direction { get; set; }
        public string Action { get; set; }
        public string Protocol { get; set; }
        public string LocalPorts { get; set; }
        public string RemotePorts { get; set; }
        public string RemoteAddresses { get; set; }
        public string LocalAddresses { get; set; }
        public string Enabled { get; set; }
    }
}
