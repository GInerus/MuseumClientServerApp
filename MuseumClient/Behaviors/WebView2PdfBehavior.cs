using Microsoft.Xaml.Behaviors;
using Microsoft.Web.WebView2.Wpf;
using MuseumClient.ViewModels.Details;
using System;
using System.ComponentModel;

namespace MuseumClient.Behaviors
{
    public class WebView2PdfBehavior : Behavior<WebView2>
    {
        private DocumentViewerViewModel? _vm;

        protected override async void OnAttached()
        {
            base.OnAttached();

            await AssociatedObject.EnsureCoreWebView2Async();

            _vm = AssociatedObject.DataContext as DocumentViewerViewModel;


            if (_vm != null)
            {
                _vm.PropertyChanged += Vm_PropertyChanged;

                TryLoadPdf();
            }
        }

        private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DocumentViewerViewModel.LocalPdfPath))
            {
                TryLoadPdf();
            }
        }

        private void TryLoadPdf()
        {
            if (_vm == null)
                return;

            if (string.IsNullOrWhiteSpace(_vm.LocalPdfPath))
                return;

            AssociatedObject.Source =
                new Uri(_vm.LocalPdfPath, UriKind.Absolute);
        }

        protected override void OnDetaching()
        {
            if (_vm != null)
                _vm.PropertyChanged -= Vm_PropertyChanged;

            base.OnDetaching();
        }
    }
}