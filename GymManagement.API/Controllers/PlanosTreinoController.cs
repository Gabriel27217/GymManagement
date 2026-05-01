using GymManagement.Data;
using GymManagement.Models.Entities;
using GymManagement.API.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlanosTreinoController : ControllerBase
    {
        private readonly GymDbContext _db;
        private readonly IHubContext<GymHub> _hub;

        public PlanosTreinoController(GymDbContext db, IHubContext<GymHub> hub) { _db = db; _hub = hub; }

        /// <summary>GET /api/PlanosTreino — lista planos genéricos.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var planos = await _db.PlanosTreino
                .Include(p => p.Atribuicoes)
                .Where(p => p.Ativo)
                .OrderBy(p => p.Nome)
                .Select(p => new {
                    p.Id, p.Nome, p.Descricao, p.DuracaoMinutos,
                    Nivel = p.Nivel.ToString(),
                    Objetivo = p.Objetivo.ToString(),
                    p.Ativo,
                    TotalAtribuicoes = p.Atribuicoes.Count
                })
                .ToListAsync();
            return Ok(planos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            // FIX: projetar para evitar referência circular
            var p = await _db.PlanosTreino
                .Where(x => x.Id == id)
                .Select(x => new {
                    x.Id, x.Nome, x.Descricao, x.DuracaoMinutos,
                    Nivel = (int)x.Nivel,
                    Objetivo = (int)x.Objetivo,
                    x.Ativo, x.DataCriacao,
                    TotalAtribuicoes = x.Atribuicoes.Count
                })
                .FirstOrDefaultAsync();
            if (p == null) return NotFound();
            return Ok(p);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] PlanoTreino dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _db.PlanosTreino.Add(dto);
            await _db.SaveChangesAsync();
            await _hub.Clients.All.SendAsync("NovoPlano", dto.Nome);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] PlanoTreino dto)
        {
            var plano = await _db.PlanosTreino.FindAsync(id);
            if (plano == null) return NotFound();
            plano.Nome = dto.Nome; plano.Descricao = dto.Descricao;
            plano.DuracaoMinutos = dto.DuracaoMinutos;
            plano.Nivel = dto.Nivel; plano.Objetivo = dto.Objetivo; plano.Ativo = dto.Ativo;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var plano = await _db.PlanosTreino.FindAsync(id);
            if (plano == null) return NotFound();
            _db.PlanosTreino.Remove(plano);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ── Atribuições a alunos ────────────────────────────────

        [HttpGet("{id}/atribuicoes")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAtribuicoes(int id)
        {
            var atribuicoes = await _db.PlanosTreinoAluno
                .Include(a => a.Utilizador).Include(a => a.Instrutor)
                .Where(a => a.PlanoTreinoId == id)
                .Select(a => new {
                    a.Id, a.DataInicio, a.DataFim, a.Observacoes, a.Ativo,
                    Utilizador = a.Utilizador!.Nome,
                    Instrutor = a.Instrutor != null ? a.Instrutor.Nome : null
                })
                .ToListAsync();
            return Ok(atribuicoes);
        }

        [HttpPost("{id}/atribuir")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Atribuir(int id, [FromBody] PlanoTreinoAluno dto)
        {
            dto.PlanoTreinoId = id;
            dto.DataInicio = DateTime.Now;
            _db.PlanosTreinoAluno.Add(dto);
            await _db.SaveChangesAsync();
            return Ok(new { mensagem = "Plano atribuído com sucesso." });
        }
    }
}
