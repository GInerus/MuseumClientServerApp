using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using MuseumServer.Data;
using MuseumServer.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MuseumServer.Services
{
    public class ReportService
    {
        private readonly MuseumContext _context;
        private readonly IWebHostEnvironment _env;

        public ReportService(MuseumContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<int> GetExhibitsCountAsync()
            => await _context.Exhibits.CountAsync();

        public async Task<byte[]> GenerateExhibitsReportAsync(ExhibitsReportRequest request)
        {
            var query = _context.Exhibits
                .Include(e => e.Department)
                .OrderBy(e => e.ExhibitId)
                .AsQueryable();

            if (request.Limit is > 0)
                query = query.Take(request.Limit.Value);

            var exhibits = await query.ToListAsync();

            byte[]? LoadThumbnail(string? imagePath)
            {
                if (!request.IncludeImages || string.IsNullOrEmpty(imagePath))
                    return null;

                var thumbPath = Path.Combine(_env.WebRootPath, "exhibits", "thumbnails", imagePath);
                if (File.Exists(thumbPath))
                    return File.ReadAllBytes(thumbPath);

                var originalPath = Path.Combine(_env.WebRootPath, "exhibits", "images", imagePath);
                return File.Exists(originalPath) ? File.ReadAllBytes(originalPath) : null;
            }

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Отчёт по экспонатам").FontSize(18).Bold();
                        col.Item().Text($"Сформировано: {DateTime.Now:dd.MM.yyyy HH:mm}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                        col.Item().Text($"Всего в отчёте: {exhibits.Count}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    });

                    page.Content().PaddingTop(10).Column(col =>
                    {
                        if (request.Compact)
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(35);
                                    if (request.IncludeImages)
                                        columns.ConstantColumn(50);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("ID").Bold();
                                    if (request.IncludeImages)
                                        header.Cell().Text("");
                                    header.Cell().Text("Название").Bold();
                                    header.Cell().Text("Отдел").Bold();
                                    header.Cell().Text("Материалы").Bold();
                                    header.Cell().Text("Хранение").Bold();
                                });

                                foreach (var exhibit in exhibits)
                                {
                                    table.Cell().Text(exhibit.ExhibitId.ToString());

                                    if (request.IncludeImages)
                                    {
                                        var bytes = LoadThumbnail(exhibit.ImagePath);
                                        if (bytes != null)
                                            table.Cell().Height(40).Width(40).Image(bytes).FitArea();
                                        else
                                            table.Cell().Text("—");
                                    }

                                    table.Cell().Text(exhibit.Name);
                                    table.Cell().Text(exhibit.Department?.Name ?? "Без отдела");
                                    table.Cell().Text(exhibit.Materials ?? "—");
                                    table.Cell().Text(exhibit.IsPermanent ? "Постоянное" : "Временное");
                                }
                            });
                        }
                        else
                        {
                            foreach (var exhibit in exhibits)
                            {
                                col.Item().PaddingBottom(10)
                                    .Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10)
                                    .Row(row =>
                                    {
                                        if (request.IncludeImages)
                                        {
                                            var bytes = LoadThumbnail(exhibit.ImagePath);
                                            if (bytes != null)
                                                row.ConstantItem(70).Height(70).Image(bytes).FitArea();
                                            else
                                                row.ConstantItem(70);
                                        }

                                        row.RelativeItem()
                                            .PaddingLeft(request.IncludeImages ? 10 : 0)
                                            .Column(info =>
                                            {
                                                info.Item().Text($"№{exhibit.ExhibitId} — {exhibit.Name}")
                                                .Bold().FontSize(13);
                                                info.Item().Text($"Отдел: {exhibit.Department?.Name ?? "Без отдела"}");
                                                info.Item().Text($"Материалы: {exhibit.Materials ?? "—"}");
                                                info.Item().Text($"Тип хранения: {(exhibit.IsPermanent ? "Постоянное" : "Временное")}");
                                            });
                                    });
                            }
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" из ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}