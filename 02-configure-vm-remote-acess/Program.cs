using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Network.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System;

namespace ConfigureVmRemoteAcess
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var groupName = "az204-ResourceGroup";
            var vmName = "az204VMTesting";
            var location = Region.USWest2;
            var vNetName = "az204VNET";
            var vNetAddress = "172.16.0.0/16";
            var subnetName = "az204Subnet";
            var subnetAddress = "172.16.0.0/24";
            var nicName = "az204NIC";
            var adminUser = "azureadminuser";
            var adminPassword = "Pa$$w0rd!2019";
            var publicIPName = "az204publicIP";
            var nsgName = "az204VNET-NSG";

            var credentials = SdkContext.AzureCredentialsFactory
                .FromFile("./azureauth.properties");

            var azure = Azure.Configure()
            .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
            .Authenticate(credentials)
            .WithDefaultSubscription();

            Console.WriteLine($"Creating resource group {groupName} ...");
            var resourceGroup = azure.ResourceGroups.Define(groupName)
                .WithRegion(location)
                .Create();

            Console.WriteLine($"Creating virtual network {vNetName} ...");
            var network = azure.Networks.Define(vNetName)
                .WithRegion(location)
                .WithExistingResourceGroup(groupName)
                .WithAddressSpace(vNetAddress)
                .WithSubnet(subnetName, subnetAddress)
                .Create();

            Console.WriteLine($"Creating public IP {publicIPName} ...");
            var publicIP = azure.PublicIPAddresses.Define(publicIPName)
                .WithRegion(location)
                .WithExistingResourceGroup(groupName)
                .Create();

            Console.WriteLine($"Creating Network Security Group {nsgName} ...");
            var nsg = azure.NetworkSecurityGroups.Define(nsgName)
                .WithRegion(location)
                .WithExistingResourceGroup(groupName)
                .Create();

            Console.WriteLine($"Creating a Security Rule for allowing the remoteaccess");
            nsg.Update()
                .DefineRule("Allow-RDP")
                .AllowInbound()
                .FromAnyAddress()
                .FromAnyPort()
                .ToAnyAddress()
                .ToPort(3389)
                .WithProtocol(SecurityRuleProtocol.Tcp)
                .WithPriority(100)
                .WithDescription("Allow-RDP")
                .Attach()
                .Apply();

            Console.WriteLine($"Creating network interface {nicName} ...");
            var nic = azure.NetworkInterfaces.Define(nicName)
                .WithRegion(location)
                .WithExistingResourceGroup(groupName)
                .WithExistingPrimaryNetwork(network)
                .WithSubnet(subnetName)
                .WithPrimaryPrivateIPAddressDynamic()
                .Create();

            Console.WriteLine($"Creating virtual machine {vmName} ...");
            azure.VirtualMachines.Define(vmName)
                .WithRegion(location)
                .WithExistingResourceGroup(groupName)
                .WithExistingPrimaryNetworkInterface(nic)
                .WithLatestWindowsImage("MicrosoftWindowsServer", "WindowsServer",
                "2012-R2-Datacenter")
                .WithAdminUsername(adminUser)
                .WithAdminPassword(adminPassword)
                .WithComputerName(vmName)
                .WithSize(VirtualMachineSizeTypes.StandardDS2V2)
                .Create();

            Console.ReadKey();
        }
    }
}
