using GymManagement.Data;
using GymManagement.Models.DTOs;
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
            var p = await _db.PlanosTreino.Include(x => x.Atribuicoes).FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();
            return Ok(new { p.Id, p.Nome, p.Descricao, p.DuracaoMinutos, Nivel = p.Nivel.ToString(), Objetivo = p.Objetivo.ToString(), p.Ativo });
        }

        // FIX #3: recebe PlanoTreinoCreateDto em vez da entidade diretamente
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] PlanoTreinoCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var plano = new PlanoTreino
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                DuracaoMinutos = dto.DuracaoMinutos,
                Nivel = (NivelDificuldade)dto.Nivel,
                Objetivo = (ObjetivoTreino)dto.Objetivo,
                Ativo = dto.Ativo,
                DataCriacao = DateTime.Now
            };

            _db.PlanosTreino.Add(plano);
            await _db.SaveChangesAsync();
            await _hub.Clients.All.SendAsync("NovoPlano", plano.Nome);
            return CreatedAtAction(nameof(GetById), new { id = plano.Id }, new { plano.Id, plano.Nome });
        }

        // FIX #3: recebe PlanoTreinoCreateDto em vez da entidade diretamente
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] PlanoTreinoCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var plano = await _db.PlanosTreino.FindAsync(id);
            if (plano == null) return NotFound();
            plano.Nome = dto.Nome;
            plano.Descricao = dto.Descricao;
            plano.DuracaoMinutos = dto.DuracaoMinutos;
            plano.Nivel = (NivelDificuldade)dto.Nivel;
            plano.Objetivo = (ObjetivoTreino)dto.Objetivo;
            plano.Ativo = dto.Ativo;
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

        // FIX #3/#4: usa PlanoTreinoAlunoCreateDto e valida se o utilizador existe
        [HttpPost("{id}/atribuir")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Atribuir(int id, [FromBody] PlanoTreinoAlunoCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Verificar se o plano existe
            var planoExiste = await _db.PlanosTreino.AnyAsync(p => p.Id == id);
            if (!planoExiste) return NotFound(new { mensagem = "Plano de treino não encontrado." });

            // FIX #4: validar que o utilizador existe
            var utilizadorExiste = await _db.Utilizadores.AnyAsync(u => u.Id == dto.UtilizadorId);
            if (!utilizadorExiste)
                return BadRequest(new { mensagem = "Utilizador não encontrado." });

            // FIX #4: validar que o instrutor existe (se fornecido)
            if (dto.InstrutorId.HasValue)
            {
                var instrutorExiste = await _db.Instrutores.AnyAsync(i => i.Id == dto.InstrutorId.Value);
                if (!instrutorExiste)
                    return BadRequest(new { mensagem = "Instrutor não encontrado." });
            }

            var atribuicao = new PlanoTreinoAluno
            {
                PlanoTreinoId = id,
                UtilizadorId = dto.UtilizadorId,
                InstrutorId = dto.InstrutorId,
                DataInicio = DateTime.Now,
                DataFim = dto.DataFim,
                Observacoes = dto.Observacoes,
                Ativo = dto.Ativo
            };

            _db.PlanosTreinoAluno.Add(atribuicao);
            await _db.SaveChangesAsync();
            return Ok(new { mensagem = "Plano atribuído com sucesso." });
        }
    }
}
