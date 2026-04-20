using GymManagement.API.Hubs;
using GymManagement.Data;
using GymManagement.Models.DTOs;
using GymManagement.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AulasController : ControllerBase
    {
        private readonly GymDbContext _db;
        private readonly IHubContext<GymHub> _hub;

        public AulasController(GymDbContext db, IHubContext<GymHub> hub)
        {
            _db  = db;
            _hub = hub;
        }

        private static readonly string[] DiasLabel = { "", "Segunda", "Terça", "Quarta", "Quinta", "Sexta", "Sábado", "Domingo" };

        /// <summary>GET /api/Aulas — lista todas as aulas ordenadas por dia e hora.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var aulas = await _db.Aulas
                .Include(a => a.PlanoTreino)
                .Include(a => a.Instrutor)
                .Include(a => a.Sala)
                .Include(a => a.Inscricoes)
                .Where(a => a.Ativa)
                .OrderBy(a => a.DiaSemana)
                .ThenBy(a => a.Hora)
                .Select(a => MapToDto(a))
                .ToListAsync();

            return Ok(aulas);
        }

        /// <summary>GET /api/Aulas/{id}.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var aula = await _db.Aulas
                .Include(a => a.PlanoTreino)
                .Include(a => a.Instrutor)
                .Include(a => a.Sala)
                .Include(a => a.Inscricoes)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (aula == null) return NotFound(new { mensagem = "Aula não encontrada." });
            return Ok(MapToDto(aula));
        }

        /// <summary>POST /api/Aulas — cria nova aula (Admin).</summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] AulaCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var aula = new Aula
            {
                Nome           = dto.Nome,
                DiaSemana      = (DiaSemana)dto.DiaSemana,
                Hora           = TimeOnly.Parse(dto.Hora),
                DuracaoMinutos = dto.DuracaoMinutos,
                PlanoTreinoId  = dto.PlanoTreinoId,
                InstrutorId    = dto.InstrutorId,
                SalaId         = dto.SalaId
            };

            _db.Aulas.Add(aula);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = aula.Id }, new { mensagem = "Aula criada.", id = aula.Id });
        }

        /// <summary>PUT /api/Aulas/{id} — atualiza aula (Admin).</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] AulaCreateDto dto)
        {
            var aula = await _db.Aulas.FindAsync(id);
            if (aula == null) return NotFound();

            aula.Nome           = dto.Nome;
            aula.DiaSemana      = (DiaSemana)dto.DiaSemana;
            aula.Hora           = TimeOnly.Parse(dto.Hora);
            aula.DuracaoMinutos = dto.DuracaoMinutos;
            aula.PlanoTreinoId  = dto.PlanoTreinoId;
            aula.InstrutorId    = dto.InstrutorId;
            aula.SalaId         = dto.SalaId;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>DELETE /api/Aulas/{id} — elimina aula (Admin).</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var aula = await _db.Aulas.FindAsync(id);
            if (aula == null) return NotFound();
            _db.Aulas.Remove(aula);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        private static AulaListDto MapToDto(Aula a) => new()
        {
            Id               = a.Id,
            Nome             = a.Nome,
            DiaSemana        = (int)a.DiaSemana,
            DiaSemanaLabel   = DiasLabel[(int)a.DiaSemana],
            Hora             = a.Hora.ToString("HH:mm"),
            HoraFim          = a.Hora.AddMinutes(a.DuracaoMinutos).ToString("HH:mm"),
            DuracaoMinutos   = a.DuracaoMinutos,
            PlanoTreino      = a.PlanoTreino?.Nome ?? "",
            Instrutor        = a.Instrutor?.Nome ?? "",
            Sala             = a.Sala?.Nome ?? "",
            CapacidadeMaxima = a.Sala?.CapacidadeMaxima ?? 0,
            Inscritos        = a.Inscricoes.Count,
            VagasDisponiveis = (a.Sala?.CapacidadeMaxima ?? 0) - a.Inscricoes.Count,
            Lotada           = (a.Sala?.CapacidadeMaxima ?? 0) - a.Inscricoes.Count <= 0,
            Ativa            = a.Ativa
        };
    }
}
