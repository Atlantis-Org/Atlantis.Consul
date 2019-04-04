using System.Threading.Tasks;

namespace Atlantis.Consul
{
    public interface IService
    {
        ConsulServiceOptions Options{get;set;}
        
        Task<bool> RegisteAsync();
        
        Task<bool> DeregisterAsync();

        Task<ServiceInfo> GetService(string serviceName);
    }
}
