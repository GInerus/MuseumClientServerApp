using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MuseumClient.Services
{
    public static class WebView2EnvironmentService
    {
        private static readonly string UserDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MuseumSpace",
            "WebView2");

        public static Task<CoreWebView2Environment> CreateAsync()
        {
            Directory.CreateDirectory(UserDataFolder);

            return CoreWebView2Environment.CreateAsync(
                null,
                UserDataFolder);
        }
    }
}