using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;

namespace MuseumClient.Services
{
    public class ConfigService
    {
        public ServerConfig Server { get; private set; }
        public StreamingConfig Streaming { get; private set; }

        public ConfigService()
        {
            var json = File.ReadAllText("AppSettings.json");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Server = new ServerConfig
            {
                Protocol = root.GetProperty("Server").GetProperty("Protocol").GetString(),
                Host = root.GetProperty("Server").GetProperty("Host").GetString(),
                Port = root.GetProperty("Server").GetProperty("Port").GetInt32()
            };

            Streaming = new StreamingConfig
            {
                Protocol = root.GetProperty("Streaming").GetProperty("Protocol").GetString(),
                Host = root.GetProperty("Streaming").GetProperty("Host").GetString(),
                Port = root.GetProperty("Streaming").GetProperty("Port").GetInt32()
            };
        }
    }

    public class ServerConfig
    {
        public string Protocol { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }

    public class StreamingConfig
    {
        public string Protocol { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }
}
