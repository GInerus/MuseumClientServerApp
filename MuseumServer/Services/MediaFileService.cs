using Microsoft.EntityFrameworkCore;
using MuseumServer.Data;
using MuseumServer.Models;
using MuseumServer.DTOs;

namespace MuseumServer.Services
{
    public class MediaFileService
    {
        private readonly MuseumContext _context;

        public MediaFileService(MuseumContext context)
        {
            _context = context;
        }

        // Получить все медиафайлы
        public async Task<List<MediaFileResponse>> GetAllAsync()
        {
            return await _context.MediaFiles
                .Include(m => m.Department)
                .Select(m => new MediaFileResponse
                {
                    MediaFileId = m.MediaFileId,
                    FilePath = m.FilePath,
                    MediaType = m.MediaType,
                    Description = m.Description,
                    Department = new DepartmentInfo
                    {
                        DepartmentId = m.Department.DepartmentId,
                        Name = m.Department.Name
                    }
                })
                .ToListAsync();
        }

        // Получить один медиафайл по id
        public async Task<MediaFileResponse?> GetAsync(int id)
        {
            return await _context.MediaFiles
                .Where(m => m.MediaFileId == id)
                .Select(m => new MediaFileResponse
                {
                    MediaFileId = m.MediaFileId,
                    FilePath = m.FilePath,
                    MediaType = m.MediaType,
                    Description = m.Description,
                    Department = new DepartmentInfo
                    {
                        DepartmentId = m.Department.DepartmentId,
                        Name = m.Department.Name
                    }
                })
                .FirstOrDefaultAsync();
        }

        // Создать медиафайл
        public async Task<MediaFile> CreateAsync(MediaFile media)
        {
            _context.MediaFiles.Add(media);
            await _context.SaveChangesAsync();
            return media;
        }

        // Удалить медиафайл
        public async Task<bool> DeleteAsync(int id)
        {
            var media = await _context.MediaFiles.FindAsync(id);
            if (media == null) return false;

            _context.MediaFiles.Remove(media);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}