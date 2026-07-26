using Microsoft.Win32;
using MuseumClient.Commands;
using MuseumClient.Models;
using MuseumClient.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MuseumClient.ViewModels.Details
{
    public class DocumentViewerViewModel : INotifyPropertyChanged
    {
        // Document — обычная статья из БД (Document/{id})
        // Guide     — статический файл руководства (system/guide), без метаданных в БД
        private enum DocumentSource
        {
            Document,
            Guide
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly ApiService _apiService;
        private readonly int _id;
        private readonly DocumentSource _source;

        private string _fileType = "";
        public string FileType
        {
            get => _fileType;
            set
            {
                _fileType = value;
                OnPropertyChanged(nameof(FileType));
                OnPropertyChanged(nameof(IsPdf));
                OnPropertyChanged(nameof(IsText));
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        private string _status = "";
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        private string? _localPdfPath;
        public string? LocalPdfPath
        {
            get => _localPdfPath;
            set
            {
                _localPdfPath = value;
                OnPropertyChanged(nameof(LocalPdfPath));
            }
        }

        private string? _text;
        public string? Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(nameof(Text)); }
        }

        private bool _isPdfReady;
        public bool IsPdfReady
        {
            get => _isPdfReady;
            set
            {
                _isPdfReady = value;
                OnPropertyChanged(nameof(IsPdfReady));
            }
        }

        private byte[]? _rawFile;

        public bool IsPdf => FileType?.ToLower() == "pdf";
        public bool IsText => FileType?.ToLower() is "txt";
        public bool IsHtml => FileType?.ToLower() is "pdf" or "docx" or "md";

        private string _title = "";
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public string Subtitle => "Статья музея";
        public bool ShowSubtitle => _source != DocumentSource.Guide;

        private string? _htmlPath;
        public string? HtmlPath
        {
            get => _htmlPath;
            set
            {
                _htmlPath = value;
                OnPropertyChanged(nameof(HtmlPath));
            }
        }

        public RelayCommand DownloadCommand { get; }

        // Обычный режим — статья из БД
        public DocumentViewerViewModel(int id, string fileType)
            : this(id, fileType, DocumentSource.Document)
        {
        }

        private DocumentViewerViewModel(int id, string fileType, DocumentSource source)
        {
            _id = id;
            _source = source;
            FileType = fileType;

            _apiService = new ApiService(
                new ConfigService().Server,
                AuthService.Instance()
            );

            DownloadCommand = new RelayCommand(async _ => await DownloadAsync());

            _ = InitializeAsync();
        }

        // Второй режим — руководство пользователя (system/guide), без Document/{id}
        public static DocumentViewerViewModel CreateForGuide()
        {
            var vm = new DocumentViewerViewModel(0, "pdf", DocumentSource.Guide)
            {
                Title = "Руководство пользователя"
            };

            return vm;
        }

        private async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                Status = "Загрузка документа...";

                if (_source == DocumentSource.Document)
                {
                    await LoadMetadataAsync();
                }

                Status = "Загрузка содержимого...";
                await LoadAsync();

                Status = "";
            }
            catch
            {
                Status = "Ошибка загрузки документа";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadMetadataAsync()
        {
            var response = await _apiService.GetAsync<DocumentResponse>($"Document/{_id}");

            if (response?.Data != null)
            {
                Title = response.Data.Title;
                FileType = response.Data.FileType;
            }
        }

        // Отдельные временные имена файлов для гайда, чтобы не пересекаться
        // с реальными Document Id (у гайда _id всегда 0)
        private string TempBaseName => _source == DocumentSource.Guide
            ? "user_guide"
            : _id.ToString();

        private async Task LoadAsync()
        {
            var streamEndpoint = _source == DocumentSource.Guide
                ? "system/guide"
                : $"Document/stream/{_id}";

            var bytes = await _apiService.GetBytesAsync(streamEndpoint);

            _rawFile = bytes;

            switch (FileType.ToLower())
            {
                case "txt":
                    Text = Encoding.UTF8.GetString(bytes);
                    break;
                case "md":
                    {
                        var markdown = Encoding.UTF8.GetString(bytes);
                        var html = MarkdownConverter.ConvertToHtml(markdown);

                        var htmlPath = Path.Combine(Path.GetTempPath(), $"{TempBaseName}_md.html");
                        File.WriteAllText(htmlPath, html, Encoding.UTF8);

                        HtmlPath = new Uri(htmlPath).AbsoluteUri;
                        break;
                    }
                case "pdf":
                    {
                        var path = Path.Combine(Path.GetTempPath(), $"{TempBaseName}.pdf");
                        File.WriteAllBytes(path, _rawFile!);

                        LocalPdfPath = path;
                        break;
                    }
                case "docx":
                    {
                        var path = Path.Combine(Path.GetTempPath(), $"{TempBaseName}.docx");
                        File.WriteAllBytes(path, _rawFile!);

                        var html = DocxToHtmlConverter.Convert(path);

                        var htmlPath = Path.Combine(Path.GetTempPath(), $"{TempBaseName}.html");
                        File.WriteAllText(htmlPath, html, Encoding.UTF8);

                        HtmlPath = new Uri(htmlPath).AbsoluteUri;
                        OnPropertyChanged(nameof(HtmlPath));
                        break;
                    }
                default:
                    Text = "Формат откроется через внешнее приложение";
                    break;
            }
        }

        private async Task DownloadAsync()
        {
            string extension = FileType?.ToLower() switch
            {
                "pdf" => ".pdf",
                "txt" => ".txt",
                "md" => ".md",
                "doc" => ".doc",
                "docx" => ".docx",
                _ => ""
            };

            var dialog = new SaveFileDialog
            {
                FileName = string.IsNullOrWhiteSpace(Title)
                    ? $"document{extension}"
                    : $"{Title}{extension}",

                Filter = FileType?.ToLower() switch
                {
                    "txt" => "Текстовый документ (*.txt)|*.txt|Все файлы (*.*)|*.*",
                    "md" => "Markdown файл (*.md)|*.md|Все файлы (*.*)|*.*",
                    "pdf" => "PDF документ (*.pdf)|*.pdf|Все файлы (*.*)|*.*",
                    "doc" => "Документ Word 97-2003 (*.doc)|*.doc|Все файлы (*.*)|*.*",
                    "docx" => "Документ Word(*.docx)|*.docx|Все файлы (*.*)|*.*",
                    _ => "Все файлы (*.*)|*.*"
                }
            };

            if (dialog.ShowDialog() == true && _rawFile != null)
            {
                string path = dialog.FileName;

                if (!Path.HasExtension(path))
                    path += extension;

                await File.WriteAllBytesAsync(path, _rawFile);
            }
        }
    }
}