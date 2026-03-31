using Microsoft.EntityFrameworkCore;
using MuseumServer.Data;
using MuseumServer.DTOs;
using MuseumServer.Models;

namespace MuseumServer.Services
{
    public class DepartmentService
    {
        private readonly MuseumContext _context;

        public DepartmentService(MuseumContext context)
        {
            _context = context;
        }

        public async Task<int> GetCountAsync() => await _context.Departments.CountAsync();

        public async Task<List<DepartmentResponse>> GetAllAsync()
        {
            return await _context.Departments
                .Select(d => new DepartmentResponse
                {
                    DepartmentId = d.DepartmentId,
                    Name = d.Name,
                    Description = d.Description,
                    ImagePath = d.ImagePath
                }).ToListAsync();
        }

        public async Task<DepartmentContentResponse?> GetContentAsync(int departmentId, string? types)
        {
            var department = await _context.Departments.FindAsync(departmentId);
            if (department == null) return null;

            var typeList = types?.Split(',').Select(t => t.Trim().ToLower()).ToList();

            var content = new DepartmentContentResponse
            {
                Department = new DepartmentResponse
                {
                    DepartmentId = department.DepartmentId,
                    Name = department.Name,
                    Description = department.Description,
                    ImagePath = department.ImagePath
                }
            };

            if (typeList == null || typeList.Contains("exhibits"))
            {
                content.Exhibits = await _context.Exhibits
                    .Where(e => e.DepartmentId == departmentId)
                    .Select(e => new ExhibitResponse
                    {
                        ExhibitId = e.ExhibitId,
                        Name = e.Name,
                        Description = e.Description,
                        Materials = e.Materials,
                        IsPermanent = e.IsPermanent,
                        ImagePath = e.ImagePath,
                        DepartmentId = e.DepartmentId
                    }).ToListAsync();
            }

            if (typeList == null || typeList.Contains("mediafiles"))
            {
                content.MediaFiles = await _context.MediaFiles
                    .Where(m => m.DepartmentId == departmentId)
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
                    }).ToListAsync();
            }

            if (typeList == null || typeList.Contains("documents"))
            {
                content.Documents = await _context.Documents
                    .Where(d => d.DepartmentId == departmentId)
                    .Select(d => new DocumentFullResponse
                    {
                        DocumentId = d.DocumentId,
                        FilePath = d.FilePath,
                        FileType = d.FileType,
                        ExhibitId = d.ExhibitId,
                        Department = new DepartmentInfo
                        {
                            DepartmentId = department.DepartmentId,
                            Name = department.Name
                        }
                    }).ToListAsync();
            }

            return content;
        }

        public async Task<Department> CreateAsync(CreateDepartmentRequest request)
        {
            var dept = new Department
            {
                Name = request.Name,
                Description = request.Description,
                ImagePath = request.ImagePath
            };
            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();
            return dept;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return false;

            _context.Departments.Remove(dept);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(int id, UpdateDepartmentRequest request)
        {
            var dept = await _context.Departments.FindAsync(id);
            if (dept == null) return false;

            if (!string.IsNullOrEmpty(request.Name)) dept.Name = request.Name;
            if (request.Description != null) dept.Description = request.Description;
            if (request.ImagePath != null) dept.ImagePath = request.ImagePath;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}