using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeseumClient.Services
{
    public class SessionService
    {
        private readonly NetworkDiscoveryService _discovery;
        private readonly TcpClientService _tcp;

        public string? ServerIp { get; private set; }
        public string? Token { get; private set; }

        public SessionService(NetworkDiscoveryService discovery, TcpClientService tcp)
        {
            _discovery = discovery;
            _tcp = tcp;
        }

        public async Task<bool> DiscoverServerAsync()
        {
            ServerIp = await _discovery.DiscoverServerAsync();
            return ServerIp != null;
        }

        public async Task<bool> RegisterSessionAsync(string userType, string password = "")
        {
            if (ServerIp == null) return false;

            string msg = userType == "admin"
                ? $"REGISTER_SESSION|admin|{password}"
                : $"REGISTER_SESSION|guest";

            string resp = await _tcp.SendRequestAsync(ServerIp, msg);
            if (resp == "ADMIN_AUTH_FAIL") return false;

            Token = resp;
            return true;
        }

        public async Task<string> SendCommandAsync(string command, string? id = null)
        {
            if (ServerIp == null || Token == null) return "Нет сессии";

            string msg = id == null
                ? $"{command}|{Token}"
                : $"{command}|{Token}|{id}";

            return await _tcp.SendRequestAsync(ServerIp, msg);
        }
    }
}
