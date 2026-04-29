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
    public class InstrutoresController : ControllerBase
    {
        private readonly GymDbContext _db;
        public InstrutoresController(GymDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var instrutores = await _db.Instrutores
                .Include(i => i.Aulas)
                .OrderBy(i => i.Nome)
                .Select(i => new {
                    i.Id, i.Nome, i.Email, i.Especialidade, i.Telefone, i.Ativo,
                    TotalAulas = i.Aulas.Count
                })
                .ToListAsync();
            return Ok(instrutores);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var i = await _db.Instrutores.Include(x => x.Aulas).FirstOrDefaultAsync(x => x.Id == id);
            if (i == null) return NotFound();
            return Ok(new { i.Id, i.Nome, i.Email, i.Especialidade, i.Telefone, i.Ativo });
        }

        // FIX #3: recebe InstrutorCreateDto em vez da entidade diretamente
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] InstrutorCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var instrutor = new Instrutor
            {
                Nome = dto.Nome,
                Email = dto.Email,
                Especialidade = dto.Especialidade,
                Telefone = dto.Telefone,
                Ativo = dto.Ativo
            };

            _db.Instrutores.Add(instrutor);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = instrutor.Id }, new { instrutor.Id, instrutor.Nome });
        }

        // FIX #3: recebe InstrutorCreateDto em vez da entidade diretamente
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] InstrutorCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var instrutor = await _db.Instrutores.FindAsync(id);
            if (instrutor == null) return NotFound();
            instrutor.Nome = dto.Nome;
            instrutor.Email = dto.Email;
            instrutor.Especialidade = dto.Especialidade;
            instrutor.Telefone = dto.Telefone;
            instrutor.Ativo = dto.Ativo;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var instrutor = await _db.Instrutores.Include(i => i.Aulas).FirstOrDefaultAsync(i => i.Id == id);
            if (instrutor == null) return NotFound();
            if (instrutor.Aulas.Any())
                return BadRequest(new { mensagem = "Não é possível eliminar um instrutor com aulas associadas." });
            _db.Instrutores.Remove(instrutor);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
