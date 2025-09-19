using ClearWall.Models;
using NetFwTypeLib;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace ClearWall.ViewModels
{
    internal class FirewallRulesViewModel
    {
        private readonly DispatcherTimer _timer;
        //List for holding rules
        public ObservableCollection<FirewallRulesModel> FirewallRules { get; set; }

        public FirewallRulesViewModel()
        {
            FirewallRules = new ObservableCollection<FirewallRulesModel>();
            LoadFirewallRules();
            //Load the firewall rules every 5 seconds so list is up to date
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5) //Refresh every 5 seconds
            };
            //every timer tick load rules
            _timer.Tick += (s, e) => LoadFirewallRules();
            _timer.Start();
        }
        public void LoadFirewallRules()
        {
            //Clears current list
            FirewallRules.Clear();
            try
            {
                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                //For each rule found in th policy add it to the list
                foreach (INetFwRule rule in firewallPolicy.Rules)
                {
                    //Translating protocols into readable format
                    string protocol;
                    switch (rule.Protocol)
                    {
                        case 6:
                            protocol = "TCP";
                            break;
                        case 17:
                            protocol = "UDP";
                            break;
                        case 1:
                            protocol = "ICMP";
                            break;
                        default:
                            protocol = "";
                            break;
                    }
                    //Set string for boolean Enabled
                    string enabled;
                    if (rule.Enabled)
                    {
                        enabled = "true";
                    }
                    else
                    {
                        enabled = "false";
                    }
                    //Add data model for rule to list
                    FirewallRules.Add(new FirewallRulesModel
                    {
                        Name = rule.Name,
                        Description = rule.Description,
                        Protocol = protocol,
                        Action = rule.Action == NET_FW_ACTION_.NET_FW_ACTION_ALLOW ? "Allow" : "Block",
                        Direction = rule.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN ? "Inbound" : "Outbound",
                        Enabled = enabled,
                        RemoteAddresses = rule.RemoteAddresses
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading firewall rules: {ex.Message}");
            }
        }
    }
}
