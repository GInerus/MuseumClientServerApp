using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MuseumClient.Services
{
    public static class WebView2EnvironmentService
    {
        private static readonly string UserDataFolder = Path.Combine(
            System.Environment.GetFolderPath(
                System.Environment.SpecialFolder.LocalApplicationData),
            "MuseumSpace",
            "WebView2");

        private static readonly Lazy<Task<CoreWebView2Environment>> Environment =
            new Lazy<Task<CoreWebView2Environment>>(CreateEnvironmentAsync);

        public static Task<CoreWebView2Environment> CreateAsync()
        {
            return Environment.Value;
        }

        private static async Task<CoreWebView2Environment> CreateEnvironmentAsync()
        {
            Directory.CreateDirectory(UserDataFolder);

            return await CoreWebView2Environment.CreateAsync(
                null,
                UserDataFolder);
        }
    }
}