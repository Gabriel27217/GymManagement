using GymManagement.Data;
using GymManagement.Models.DTOs;
using GymManagement.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalasController : ControllerBase
    {
        private readonly GymDbContext _db;
        public SalasController(GymDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var salas = await _db.Salas
                .Include(s => s.Aulas)
                .OrderBy(s => s.Nome)
                .Select(s => new {
                    s.Id, s.Nome, s.CapacidadeMaxima, s.Descricao, s.Ativa,
                    TotalAulas = s.Aulas.Count
                })
                .ToListAsync();
            return Ok(salas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var s = await _db.Salas.Include(x => x.Aulas).FirstOrDefaultAsync(x => x.Id == id);
            if (s == null) return NotFound();
            return Ok(new { s.Id, s.Nome, s.CapacidadeMaxima, s.Descricao, s.Ativa });
        }

        // FIX #3: recebe SalaCreateDto em vez da entidade diretamente
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] SalaCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var sala = new Sala
            {
                Nome = dto.Nome,
                CapacidadeMaxima = dto.CapacidadeMaxima,
                Descricao = dto.Descricao,
                Ativa = dto.Ativa
            };

            _db.Salas.Add(sala);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = sala.Id }, new { sala.Id, sala.Nome });
        }

        // FIX #3: recebe SalaCreateDto em vez da entidade diretamente
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] SalaCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var sala = await _db.Salas.FindAsync(id);
            if (sala == null) return NotFound();
            sala.Nome = dto.Nome;
            sala.CapacidadeMaxima = dto.CapacidadeMaxima;
            sala.Descricao = dto.Descricao;
            sala.Ativa = dto.Ativa;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var sala = await _db.Salas.Include(s => s.Aulas).FirstOrDefaultAsync(s => s.Id == id);
            if (sala == null) return NotFound();
            if (sala.Aulas.Any())
                return BadRequest(new { mensagem = "Não é possível eliminar uma sala com aulas associadas." });
            _db.Salas.Remove(sala);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
