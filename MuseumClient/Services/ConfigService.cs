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
            dynamic config = JsonSerializer.Deserialize<dynamic>(json);
            Server = new ServerConfig
            {
                Protocol = config["Server"]["Protocol"],
                Host = config["Server"]["Host"],
                Port = config["Server"]["Port"]
            };
            Streaming = new StreamingConfig
            {
                Protocol = config["Streaming"]["Protocol"],
                Host = config["Streaming"]["Host"],
                Port = config["Streaming"]["Port"]
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
