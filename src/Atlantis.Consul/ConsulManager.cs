using System;
using Consul;

namespace Atlantis.Consul
{
    public class ConsulManager
    {
        public static readonly ConsulManager Instance=new ConsulManager();

        private IService _service;
        private ConsulClient _client;
        private ConsulSettingOptions _settingOptions;

        private ConsulManager()
        {
        }

        public ConsulManager Init(ConsulSettingOptions setting)
        {
            _settingOptions=setting;
            _client=new ConsulClient();
            _client.Config.Address=new Uri(setting.ConsulAddressUrl);

            _service=new Service(_client);
            return this;
        }

        public IService Service=>_service;

        public ConsulManager WithServiceOptions(ConsulServiceOptions options)
        {
            _service.Options=options;
            return this;
        }
    }
}
