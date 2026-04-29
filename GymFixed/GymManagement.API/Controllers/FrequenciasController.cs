using GymManagement.Data;
using GymManagement.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GymManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FrequenciasController : ControllerBase
    {
        private readonly GymDbContext _db;
        public FrequenciasController(GymDbContext db) => _db = db;

        private int UtilizadorId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>GET /api/Frequencias — lista todas (Admin) ou as do utilizador atual.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var isAdmin = User.IsInRole("Admin");
            var query = _db.Frequencias
                .Include(f => f.Utilizador)
                .AsQueryable();

            if (!isAdmin)
                query = query.Where(f => f.UtilizadorId == UtilizadorId);

            var result = await query
                .OrderByDescending(f => f.Entrada)
                .Select(f => new {
                    f.Id,
                    f.Entrada,
                    f.Saida,
                    f.Observacoes,
                    EmGinasio = !f.Saida.HasValue,
                    Duracao = f.Saida.HasValue
                        ? (f.Saida.Value - f.Entrada).Hours + "h " + (f.Saida.Value - f.Entrada).Minutes + "min"
                        : "Em curso",
                    Utilizador = f.Utilizador!.Nome,
                    f.UtilizadorId
                })
                .ToListAsync();

            return Ok(result);
        }

        /// <summary>POST /api/Frequencias/Entrada — regista entrada do próprio utilizador.</summary>
        [HttpPost("Entrada")]
        public async Task<IActionResult> RegistarEntrada([FromBody] string? observacoes = null)
        {
            var jaEntrou = await _db.Frequencias
                .AnyAsync(f => f.UtilizadorId == UtilizadorId && !f.Saida.HasValue);

            if (jaEntrou)
                return BadRequest(new { mensagem = "Já tens uma entrada registada sem saída." });

            var freq = new Frequencia
            {
                UtilizadorId = UtilizadorId,
                Entrada = DateTime.Now,
                Observacoes = observacoes
            };

            _db.Frequencias.Add(freq);
            await _db.SaveChangesAsync();
            return Ok(new { mensagem = "Entrada registada com sucesso.", id = freq.Id, entrada = freq.Entrada });
        }

        /// <summary>POST /api/Frequencias/Entrada/{utilizadorId} — Admin regista entrada de um cliente.</summary>
        [HttpPost("Entrada/{utilizadorId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegistarEntradaAdmin(int utilizadorId, [FromBody] string? observacoes = null)
        {
            var utilizador = await _db.Utilizadores.FindAsync(utilizadorId);
            if (utilizador == null)
                return NotFound(new { mensagem = "Utilizador não encontrado." });

            var jaEntrou = await _db.Frequencias
                .AnyAsync(f => f.UtilizadorId == utilizadorId && !f.Saida.HasValue);

            if (jaEntrou)
                return BadRequest(new { mensagem = $"{utilizador.Nome} já tem uma entrada registada sem saída." });

            var freq = new Frequencia
            {
                UtilizadorId = utilizadorId,
                Entrada = DateTime.Now,
                Observacoes = observacoes
            };

            _db.Frequencias.Add(freq);
            await _db.SaveChangesAsync();
            return Ok(new { mensagem = $"Entrada de {utilizador.Nome} registada com sucesso.", id = freq.Id, entrada = freq.Entrada });
        }

        /// <summary>PUT /api/Frequencias/{id}/Saida — regista saída do ginásio.</summary>
        // FIX #5: verificar que o utilizador é dono da frequência ou é Admin
        [HttpPut("{id}/Saida")]
        public async Task<IActionResult> RegistarSaida(int id)
        {
            var freq = await _db.Frequencias.FindAsync(id);
            if (freq == null) return NotFound();

            // FIX #5: apenas o dono ou um Admin podem registar a saída
            if (freq.UtilizadorId != UtilizadorId && !User.IsInRole("Admin"))
                return Forbid();

            if (freq.Saida.HasValue)
                return BadRequest(new { mensagem = "Saída já foi registada." });

            freq.Saida = DateTime.Now;
            await _db.SaveChangesAsync();

            var duracao = freq.Saida.Value - freq.Entrada;
            return Ok(new {
                mensagem = "Saída registada com sucesso.",
                saida = freq.Saida,
                duracao = $"{(int)duracao.TotalHours}h {duracao.Minutes}min"
            });
        }

        /// <summary>DELETE /api/Frequencias/{id} — elimina registo (Admin).</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var freq = await _db.Frequencias.FindAsync(id);
            if (freq == null) return NotFound();
            _db.Frequencias.Remove(freq);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
