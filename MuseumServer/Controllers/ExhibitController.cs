using Microsoft.AspNetCore.Mvc;
using MuseumServer.DTOs;
using MuseumServer.Models;
using MuseumServer.Services;

namespace MuseumServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExhibitController : ControllerBase
    {
        private readonly ExhibitService _service;

        public ExhibitController(ExhibitService service)
        {
            _service = service;
        }

        // GET: api/exhibit
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var exhibits = await _service.GetAllExhibitsAsync();
            return Ok(new { status = "ok", data = exhibits });
        }

        // GET: api/exhibit/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var exhibit = await _service.GetExhibitAsync(id);
            if (exhibit == null)
                return NotFound(new { status = "error", message = "Exhibit not found" });

            return Ok(new { status = "ok", data = exhibit });
        }

        // GET: api/exhibit/count
        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            var count = await _service.GetExhibitCountAsync();
            return Ok(new { status = "ok", count });
        }

        // POST: api/exhibit
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateExhibitRequest request)
        {
            var exhibit = new Exhibit
            {
                Name = request.Name,
                Description = request.Description,
                Materials = request.Materials,
                IsPermanent = request.IsPermanent,
                ImagePath = request.ImagePath,
                DepartmentId = request.DepartmentId
            };

            var created = await _service.CreateExhibitAsync(exhibit);
            return Ok(new { status = "ok", data = created });
        }

        // PUT: api/exhibit/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateExhibitRequest request)
        {
            var exhibit = new Exhibit
            {
                ExhibitId = id, // id берём из URL
                Name = request.Name,
                Description = request.Description,
                Materials = request.Materials,
                IsPermanent = request.IsPermanent,
                DepartmentId = request.DepartmentId
            };

            var updated = await _service.UpdateExhibitAsync(exhibit);
            if (!updated)
                return NotFound(new { status = "error", message = "Exhibit not found" });

            return Ok(new { status = "ok", data = exhibit });
        }

        // DELETE: api/exhibit/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteExhibitAsync(id);
            if (!deleted)
                return NotFound(new { status = "error", message = "Exhibit not found" });

            return Ok(new { status = "ok" });
        }
    }
}