using Microsoft.EntityFrameworkCore;
using MuseumServer.Data;
using MuseumServer.DTOs;
using MuseumServer.Models;

namespace MuseumServer.Services
{
    public class DocumentService
    {
        private readonly MuseumContext _context;

        public DocumentService(MuseumContext context)
        {
            _context = context;
        }

        // Получить все документы
        public async Task<List<DocumentResponse>> GetAllDocumentsAsync()
        {
            return await _context.Documents
                .Include(d => d.Department)
                .Select(d => new DocumentResponse
                {
                    DocumentId = d.DocumentId,
                    FilePath = d.FilePath,
                    FileType = d.FileType,
                    ExhibitId = d.ExhibitId,
                    Department = new DepartmentInfo
                    {
                        DepartmentId = d.Department.DepartmentId,
                        Name = d.Department.Name,
                    }
                })
                .ToListAsync();
        }

        // Получить один документ по id
        public async Task<DocumentResponse?> GetDocumentAsync(int id)
        {
            return await _context.Documents
                .Where(d => d.DocumentId == id)
                .Select(d => new DocumentResponse
                {
                    DocumentId = d.DocumentId,
                    FilePath = d.FilePath,
                    FileType = d.FileType,
                    ExhibitId = d.ExhibitId,
                    Department = new DepartmentInfo
                    {
                        DepartmentId = d.Department.DepartmentId,
                        Name = d.Department.Name
                    }
                })
                .FirstOrDefaultAsync();
        }

        // Получить количество документов
        public async Task<int> GetDocumentCountAsync() =>
            await _context.Documents.CountAsync();

        // Создать документ
        public async Task<Document> CreateDocumentAsync(Document document)
        {
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();
            return document;
        }

        // Удалить документ
        public async Task<bool> DeleteDocumentAsync(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) return false;

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}