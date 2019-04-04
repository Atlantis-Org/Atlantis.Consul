using System;
using System.Threading.Tasks;
using Consul;

namespace Atlantis.Consul.ConsoleSimple
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new ConsulClient();
            client.Config.Address = new Uri("http://127.0.0.1:8500");



            var service = new AgentServiceRegistration()
            {
                ID = $"atlantis-{Guid.NewGuid().ToString()}",
                Name = "atlantis",
                Tags = new string[] { "atlantis-tags" },
                Port = 4006,
                Address = "127.0.0.1",
                EnableTagOverride = false,
            };
            
            var check = new AgentCheckRegistration()
            {
                // ServiceID=service.ID,
                DeregisterCriticalServiceAfter = TimeSpan.FromMilliseconds(10),
                Status = HealthStatus.Critical,
                TTL = TimeSpan.FromMilliseconds(10)
            };
            service.Check =check;
            var result=client.Agent.ServiceRegister(service).Result;

            var checkResult = client.Agent.CheckRegister(check).Result;

            Task.Run(() =>
            {
                while (true)
                {
                    client.Agent.PassTTL(check.ID, "pass");
                    System.Threading.Thread.Sleep(8);
                }
            });

            Console.WriteLine("Consul register done!");
            Console.ReadKey();
        }
    }
}
