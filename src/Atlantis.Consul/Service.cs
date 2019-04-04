using System;
using System.Threading.Tasks;
using Consul;

namespace Atlantis.Consul
{
    public class Service : IService
    {
        private readonly ConsulClient _client;
        private bool _isCancel = false;

        public Service(ConsulClient client)
        {
            _client = client;
        }

        public ConsulServiceOptions Options { get; set; }

        public async Task<bool> DeregisterAsync()
        {
            await _client.Agent.ServiceDeregister(Options.ServiceID);
            _isCancel = true;
            return true;
        }

        public async Task<ServiceInfo> GetService(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                throw new ArgumentNullException("Service name cannot be null");
            }
            var services = await _client.Health
                .Service(serviceName, string.Empty, true);
            if(services.Response==null||services.Response.LongLength==0)
            {
                throw new ArgumentNullException($"No passing service for {serviceName}");
            }

            var service=services.Response[0];
            return new ServiceInfo()
            {
                Address=service.Service.Address,
                Port=service.Service.Port
            };
        }

        public async Task<bool> RegisteAsync()
        {
            var check = new AgentCheckRegistration()
            {
                DeregisterCriticalServiceAfter = TimeSpan.FromMilliseconds(10),
                Status = HealthStatus.Critical,
                TTL = TimeSpan.FromMilliseconds(Options.TTL),
                Timeout = TimeSpan.FromMilliseconds(Options.Timeout)
            };
            var service = new AgentServiceRegistration()
            {
                ID = Options.ServiceID,
                Name = Options.ServiceName,
                Tags = Options.Tags,
                Port = Options.Port,
                Address = Options.Address,
                Check = check
            };

            await _client.Agent.ServiceRegister(service);

            var task = Task.Run(() =>
             {
                 while (!_isCancel)
                 {
                     _client.Agent.PassTTL($"service:{service.ID}", "pass");
                     System.Threading.Thread.Sleep((int)(Options.TTL * 0.9));
                 }
             });

            return true;
        }
    }
}
