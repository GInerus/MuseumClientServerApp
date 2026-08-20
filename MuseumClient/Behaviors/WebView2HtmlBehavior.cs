using Microsoft.Web.WebView2.Wpf;
using Microsoft.Xaml.Behaviors;
using MuseumClient.Services;
using System;
using System.Windows;

namespace MuseumClient.Behaviors
{
    public class WebView2HtmlBehavior : Behavior<WebView2>
    {
        public static readonly System.Windows.DependencyProperty HtmlPathProperty =
            System.Windows.DependencyProperty.Register(
                nameof(HtmlPath),
                typeof(string),
                typeof(WebView2HtmlBehavior),
                new PropertyMetadata(null, OnHtmlPathChanged));

        public string HtmlPath
        {
            get => (string)GetValue(HtmlPathProperty);
            set => SetValue(HtmlPathProperty, value);
        }

        protected override async void OnAttached()
        {
            base.OnAttached();

            var environment = await WebView2EnvironmentService.CreateAsync();
            await AssociatedObject.EnsureCoreWebView2Async(environment);

            // если путь уже был установлен до инициализации
            Navigate();
        }

        private static void OnHtmlPathChanged(
            System.Windows.DependencyObject d,
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var behavior = (WebView2HtmlBehavior)d;
            behavior.Navigate();
        }

        private void Navigate()
        {
            if (AssociatedObject?.CoreWebView2 == null)
                return;

            if (string.IsNullOrWhiteSpace(HtmlPath))
                return;

            AssociatedObject.CoreWebView2.Navigate(new Uri(HtmlPath).AbsoluteUri);
        }
    }
}