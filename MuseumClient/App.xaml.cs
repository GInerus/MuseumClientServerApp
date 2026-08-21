using LibVLCSharp.Shared;
using MuseumClient.Services;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;

namespace MuseumClient
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Режим сохранения настроек от администратора
            if (e.Args.Length > 0 && e.Args[0] == "--save-settings")
            {
                SaveSettingsAsAdministrator(e.Args);
                Shutdown();
                return;
            }

            Core.Initialize();

            base.OnStartup(e);
        }

        private void SaveSettingsAsAdministrator(string[] args)
        {
            try
            {
                if (args.Length < 3)
                    return;

                string localUrl = Encoding.UTF8.GetString(
                    Convert.FromBase64String(args[1]));

                string remoteUrl = Encoding.UTF8.GetString(
                    Convert.FromBase64String(args[2]));

                var root = new JsonObject
                {
                    ["Server"] = new JsonObject
                    {
                        ["LocalUrl"] = localUrl,
                        ["RemoteUrl"] = remoteUrl
                    }
                };

                string path = Path.Combine(
                    AppContext.BaseDirectory,
                    "AppSettings.json");

                File.WriteAllText(
                    path,
                    root.ToJsonString(new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }));
            }
            catch
            {
                // При необходимости здесь можно добавить логирование.
            }
        }
    }
}