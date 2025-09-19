using NetFwTypeLib;
using System.Windows;

namespace ClearWall.Services
{
    internal static class FirewallManager
    {
        //handle IP rule creation
        public static void AddIPFirewallRule(string protocol, string action, string direction, string remoteAddress, string incEx)
        {
            if (incEx == "Inclusive")
            {
                ConfigureTrafficFromIP(protocol, action, direction, remoteAddress);
            }
            else if (incEx == "Exclusive")
            {
                BlockAllExceptIP(protocol, direction, remoteAddress);
            }
        }
        //handle protocol rule creation
        public static void AddProtFirewallRule(string protocol, string action, string direction, string incEx)
        {
            if (incEx == "Inclusive")
            {
                ConfigureTraffic(protocol, action, direction);
            }
            else if (incEx == "Exclusive")
            {
                BlockAllExceptSpecifiedProtocol(protocol, direction);
            }
        }

        public static void ConfigureTrafficFromIP(string protocol, string action, string direction, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                throw new ArgumentException("IP address cannot be null or empty.");
            }

            //Check IP address format
            if (!System.Net.IPAddress.TryParse(ipAddress, out _))
            {
                throw new ArgumentException("Invalid IP address format.");
            }

            //set up the firewall rule
            var policy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            var rule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwRule"));

            rule.Name = $"{action} {direction} Traffic From {ipAddress}";
            rule.Description = $"{action}s all {direction} traffic from IP address {ipAddress}.";
            if (direction == "Inbound")
            {
                rule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
            }
            else
            {
                rule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
            }
            if (action == "Block")
            {
                rule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            }
            else
            {
                rule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
            }
            rule.Enabled = true;
            rule.RemoteAddresses = ipAddress;

            if (!(protocol == null))
            {
                int protocolNumber;
                switch (protocol.ToUpper())
                {
                    case "TCP":
                        protocolNumber = 6;
                        break;
                    case "UDP":
                        protocolNumber = 17;
                        break;
                    case "ICMP":
                        protocolNumber = 1;
                        break;
                    default:
                        throw new ArgumentException("Unsupported protocol. Valid options are: TCP, UDP, ICMP.");
                }
                rule.Protocol = protocolNumber;
            }

            // Check if the rule already exists
            foreach (INetFwRule existingRule in policy.Rules)
            {
                if (existingRule.Name == rule.Name)
                {
                    throw new InvalidOperationException($"The rule '{rule.Name}' already exists.");
                }
            }

            // Add the rule to policy
            policy.Rules.Add(rule);
            MessageBox.Show("Firewall rule created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void BlockAllExceptIP(string protocol, string direction, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                throw new ArgumentException("Allowed IP address cannot be null or empty.");
            }

            // Validate IP address format
            if (!System.Net.IPAddress.TryParse(ipAddress, out _))
            {
                throw new ArgumentException("Invalid IP address format.");
            }
            var ruleName = string.Empty;
            var ruleExName = string.Empty;

            if (String.IsNullOrEmpty(protocol))
            {
                ruleName = $"Block All {direction} Except Specified IPs";
                ruleExName = $"Allow {direction} From Specified IPs";
            }
            else
            {
                ruleName = $"Block All {direction} {protocol} Except Specified IPs";
                ruleExName = $"Allow {direction} {protocol} From Specified IPs";
            }

            //Get the firewall policy
            var policy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

            //Check if the rule already exists
            INetFwRule existingRule = null;
            foreach (INetFwRule rule in policy.Rules)
            {
                if (rule.Name == ruleExName)
                {
                    existingRule = rule;
                    break;
                }
            }

            if (existingRule != null)
            {
                //Rule exists, update the allowed IP list
                var currentAllowedIPs = existingRule.RemoteAddresses;

                if (!currentAllowedIPs.Contains(ipAddress))
                {
                    policy.Rules.Remove(existingRule.Name);

                    var addIPRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwRule"));
                    addIPRule.Name = ruleExName;
                    addIPRule.Description = existingRule.Description;
                    addIPRule.Direction = existingRule.Direction;
                    addIPRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                    addIPRule.Enabled = true;
                    addIPRule.Protocol = existingRule.Protocol;
                    addIPRule.RemoteAddresses = currentAllowedIPs + $",{ipAddress}";

                    policy.Rules.Add(addIPRule);

                    Console.WriteLine($"Added {ipAddress} to the allowed list of the existing rule.");
                    MessageBox.Show("Firewall rule created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Console.WriteLine($"The IP address {ipAddress} is already in the allowed list.");
                }
            }
            else
            {
                //Rule doesn't exist, create a new one
                var newRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwRule"));

                newRule.Name = ruleName;
                newRule.Description = $"Block all {direction.ToLower()} traffic except from specified IPs.";
                if (direction == "Inbound")
                {
                    newRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
                }
                else
                {
                    newRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
                }

                newRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
                newRule.Enabled = true;
                newRule.RemoteAddresses = $"*";
                int protocolNumber = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_ANY;
                if (!(protocol == null))
                {
                    switch (protocol.ToUpper())
                    {
                        case "TCP":
                            protocolNumber = 6;
                            break;
                        case "UDP":
                            protocolNumber = 17;
                            break;
                        case "ICMP":
                            protocolNumber = 1;
                            break;
                        default:
                            throw new ArgumentException("Unsupported protocol. Valid options are: TCP, UDP, ICMP.");
                    }
                    newRule.Protocol = protocolNumber;
                }

                INetFwRule existingExclusiveRule = null;
                foreach (INetFwRule rule in policy.Rules)
                {
                    if (rule.Name == ruleName)
                    {
                        existingExclusiveRule = rule;
                        break;
                    }
                }
                if (existingExclusiveRule != null)
                {
                    Console.WriteLine($"Rule for blocking all {direction} exists, only creating new exception");
                }
                else
                {
                    policy.Rules.Add(newRule);
                }


                var newExRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwRule"));

                newExRule.Name = ruleExName;
                newExRule.Description = $"Allow exception for {direction.ToLower()} traffic";
                newExRule.Direction = newRule.Direction;
                newExRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                newExRule.Enabled = true;
                newExRule.RemoteAddresses = $"{ipAddress}";
                newExRule.Protocol = protocolNumber;

                policy.Rules.Add(newExRule);
                MessageBox.Show("Firewall rule created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                Console.WriteLine($"Created a new rule and blocked all traffic execpt from {ipAddress}.");
            }
        }

        public static void ConfigureTraffic(string protocol, string action, string direction)
        {
            //Map protocol names to numbers
            int protocolNumber;
            switch (protocol.ToUpper())
            {
                case "TCP":
                    protocolNumber = 6;
                    break;
                case "UDP":
                    protocolNumber = 17;
                    break;
                case "ICMP":
                    protocolNumber = 1;
                    break;
                default:
                    throw new ArgumentException("Unsupported protocol. Valid options are: TCP, UDP, ICMP.");
            }

            //Create the firewall rule
            var policy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            var rule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwRule"));

            rule.Name = $"{action} {direction} {protocol.ToUpper()} Traffic";
            rule.Description = $"{action}s all {direction.ToLower()} {protocol.ToUpper()} traffic.";
            rule.Protocol = protocolNumber;
            if (direction == "Inbound")
            {
                rule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
            }
            else
            {
                rule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
            }
            if (action == "Block")
            {
                rule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            }
            else
            {
                rule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
            }
            rule.Enabled = true;

            //Check if the rule already exists
            foreach (INetFwRule existingRule in policy.Rules)
            {
                if (existingRule.Name == rule.Name)
                {
                    throw new InvalidOperationException($"The rule '{rule.Name}' already exists.");
                }
            }

            //Add the rule
            policy.Rules.Add(rule);
            MessageBox.Show("Firewall rule created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        public static void BlockAllExceptSpecifiedProtocol(string protocol, string direction)
        {
            if (string.IsNullOrWhiteSpace(protocol))
            {
                throw new ArgumentException($"Protocol cannot be null or empty.");
            }

            int protocolNumber;
            switch (protocol.ToUpper())
            {
                case "TCP":
                    protocolNumber = 6;
                    break;
                case "UDP":
                    protocolNumber = 17;
                    break;
                case "ICMP":
                    protocolNumber = 1;
                    break;
                default:
                    throw new ArgumentException("Unsupported protocol. Valid options are: TCP, UDP, ICMP.");
            }

            var ruleName = $"Block All {direction} Except Specified Protocols";
            var exceptRuleName = $"Execption for Block all {direction} Protocols, Allows {protocol}";

            //Get the firewall policy
            var policy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

            //Check if the rule already exists
            INetFwRule existingRule = null;
            foreach (INetFwRule rule in policy.Rules)
            {
                if (rule.Name == ruleName)
                {
                    existingRule = rule;
                    break;
                }
            }
            INetFwRule existingExceptRule = null;
            foreach (INetFwRule rule in policy.Rules)
            {
                if (rule.Name == exceptRuleName)
                {
                    existingExceptRule = rule;
                    break;
                }
            }

            if (existingRule != null)
            {
                //Rule exists, replace old rule for protocol
                if (existingExceptRule != null)
                {
                    Console.WriteLine($"Rule already exisat to allow traffic for {protocol}.");
                    MessageBox.Show($"Rule already exists to allow traffic for {protocol}.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                var addExcept = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwRule")); ;
                addExcept.Name = exceptRuleName;
                addExcept.Description = $"Allows all {protocol} traffic.";
                addExcept.Protocol = protocolNumber;
                addExcept.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                if (direction == "Inbound")
                {
                    addExcept.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
                }
                else
                {
                    addExcept.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
                }
                addExcept.Enabled = true;
                policy.Rules.Add(addExcept);
                Console.WriteLine($"Updated the rule to traffic only for {protocol}.");
                MessageBox.Show("Firewall rule created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                //Block rule doesn't exist, create one
                var newRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwRule"));

                newRule.Name = ruleName;
                newRule.Description = $"Blocks all {direction.ToLower()} traffic.";
                if (direction == "Inbound")
                {
                    newRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
                }
                else
                {
                    newRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
                }
                newRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
                newRule.Enabled = true;
                newRule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_ANY; //Block all protocols initially

                var newExecpt = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwRule"));

                newExecpt.Name = exceptRuleName;
                newExecpt.Description = $"Allows all {protocol} traffic.";
                newExecpt.Direction = newRule.Direction;
                newExecpt.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                newExecpt.Enabled = true;
                newExecpt.Protocol = protocolNumber;


                policy.Rules.Add(newRule);
                policy.Rules.Add(newExecpt);
                Console.WriteLine($"Created new rules to allow traffic only for {protocol}.");
                MessageBox.Show("Firewall rule created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public static void DeleteFWRule(string ruleName)
        {
            var policy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            //Check if rule exists and delete it if it does
            INetFwRule existingRule = null;
            foreach (INetFwRule rule in policy.Rules)
            {
                if (rule.Name == ruleName)
                {
                    policy.Rules.Remove(ruleName);
                    MessageBox.Show("Firewall rule deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    existingRule = rule;
                    break;
                }
            }
            if (existingRule == null)
            {
                MessageBox.Show("No such rule exists");
            }
        }

    }
}
