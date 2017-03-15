using System;
using System.Fabric;
using System.Fabric.Query;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StartupTask
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            await Console.Out.WriteLineAsync("Starting StartupTask");

            var serviceTypeName = args.FirstOrDefault();
            if (string.IsNullOrEmpty(serviceTypeName))
                throw new ArgumentNullException(nameof(serviceTypeName));

            using (var client = new FabricClient())
            {
                var activationContext = FabricRuntime.GetActivationContext();
                var nodeContext = FabricRuntime.GetNodeContext();
                var nodeName = nodeContext.NodeName;
                var applicationName = new Uri(activationContext.ApplicationName);

                var deployedServiceReplicaList = await client.QueryManager.GetDeployedReplicaListAsync(nodeName, applicationName);
                var instance = deployedServiceReplicaList.OfType<DeployedStatelessServiceInstance>()
                    .FirstOrDefault(svcInstance => svcInstance.ServiceTypeName == serviceTypeName);

                if (instance == null)
                {
                    throw new InvalidOperationException($"Unable to find a service instance for {serviceTypeName}");
                }
                
                File.WriteAllText("FabricData.txt", instance.InstanceId.ToString());

                await Console.Out.WriteLineAsync("Finishing StartupTask");
            }
        }
    }
}