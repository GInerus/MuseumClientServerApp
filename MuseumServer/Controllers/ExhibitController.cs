using Microsoft.AspNetCore.Mvc;
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

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllExhibitsAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var exhibit = await _service.GetExhibitAsync(id);
            if (exhibit == null) return NotFound(new { status = "error", message = "Exhibit not found" });
            return Ok(exhibit);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Exhibit exhibit)
        {
            var created = await _service.CreateExhibitAsync(exhibit);
            return Ok(created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Exhibit exhibit)
        {
            // ставим id из URL
            exhibit.ExhibitId = id;

            var updated = await _service.UpdateExhibitAsync(exhibit);
            if (!updated)
                return NotFound(new { status = "error", message = "Exhibit not found" });

            return Ok(new { status = "ok", data = exhibit });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteExhibitAsync(id);
            if (!deleted) return NotFound(new { status = "error", message = "Exhibit not found" });

            return Ok(new { status = "ok" });
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            var count = (await _service.GetAllExhibitsAsync()).Count;
            return Ok(new { status = "ok", count });
        }
    }
}