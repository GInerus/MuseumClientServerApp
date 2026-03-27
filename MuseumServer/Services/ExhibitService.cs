using MuseumServer.Data;
using MuseumServer.Models;
using Microsoft.EntityFrameworkCore;

namespace MuseumServer.Services
{
    public class ExhibitService
    {
        private readonly MuseumContext _context;

        public ExhibitService(MuseumContext context)
        {
            _context = context;
        }

        public async Task<List<Exhibit>> GetAllExhibitsAsync() =>
            await _context.Exhibits.Include(e => e.Department).ToListAsync();

        public async Task<Exhibit?> GetExhibitAsync(int id) =>
            await _context.Exhibits.Include(e => e.Department)
                                   .FirstOrDefaultAsync(e => e.ExhibitId == id);

        public async Task<Exhibit> CreateExhibitAsync(Exhibit exhibit)
        {
            _context.Exhibits.Add(exhibit);
            await _context.SaveChangesAsync();
            return exhibit;
        }

        public async Task<bool> UpdateExhibitAsync(Exhibit exhibit)
        {
            var existing = await _context.Exhibits.FindAsync(exhibit.ExhibitId);
            if (existing == null) return false;

            existing.Name = exhibit.Name;
            existing.Description = exhibit.Description;
            existing.Materials = exhibit.Materials;
            existing.IsPermanent = exhibit.IsPermanent;
            existing.DepartmentId = exhibit.DepartmentId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteExhibitAsync(int id)
        {
            var exhibit = await _context.Exhibits.FindAsync(id);
            if (exhibit == null) return false;

            _context.Exhibits.Remove(exhibit);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}