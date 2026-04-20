using GymManagement.API.Hubs;
using GymManagement.Data;
using GymManagement.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GymManagement.API.Controllers
{
    /// <summary>
    /// Gestão de inscrições em aulas (muitos-para-muitos).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InscricoesController : ControllerBase
    {
        private readonly GymDbContext _db;
        private readonly IHubContext<GymHub> _hub;

        public InscricoesController(GymDbContext db, IHubContext<GymHub> hub)
        {
            _db  = db;
            _hub = hub;
        }

        private int UtilizadorAtualId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>GET /api/Inscricoes/Minhas — inscrições do utilizador autenticado.</summary>
        [HttpGet("Minhas")]
        public async Task<IActionResult> GetMinhas()
        {
            var inscricoes = await _db.Inscricoes
                .Include(i => i.Aula).ThenInclude(a => a!.Instrutor)
                .Include(i => i.Aula).ThenInclude(a => a!.Sala)
                .Include(i => i.Aula).ThenInclude(a => a!.PlanoTreino)
                .Where(i => i.UtilizadorId == UtilizadorAtualId)
                .OrderBy(i => i.Aula!.DiaSemana).ThenBy(i => i.Aula!.Hora)
                .Select(i => new
                {
                    i.Id,
                    i.DataInscricao,
                    Aula = new
                    {
                        i.Aula!.Id,
                        i.Aula.Nome,
                        DiaSemana = (int)i.Aula.DiaSemana,
                        DiaSemanaLabel = i.Aula.DiaSemana.ToString(),
                        Hora = i.Aula.Hora.ToString("HH:mm"),
                        i.Aula.DuracaoMinutos,
                        Instrutor = i.Aula.Instrutor!.Nome,
                        Sala      = i.Aula.Sala!.Nome
                    }
                })
                .ToListAsync();

            return Ok(inscricoes);
        }

        /// <summary>POST /api/Inscricoes — inscreve o utilizador autenticado numa aula.</summary>
        [HttpPost]
        public async Task<IActionResult> Inscrever([FromBody] int aulaId)
        {
            var aula = await _db.Aulas
                .Include(a => a.Sala)
                .Include(a => a.Inscricoes)
                .FirstOrDefaultAsync(a => a.Id == aulaId);

            if (aula == null) return NotFound(new { mensagem = "Aula não encontrada." });

            if (aula.Lotada)
                return BadRequest(new { mensagem = "Aula lotada. Não há vagas disponíveis." });

            if (await _db.Inscricoes.AnyAsync(i => i.UtilizadorId == UtilizadorAtualId && i.AulaId == aulaId))
                return Conflict(new { mensagem = "Já está inscrito nesta aula." });

            _db.Inscricoes.Add(new InscricaoAula { UtilizadorId = UtilizadorAtualId, AulaId = aulaId });
            await _db.SaveChangesAsync();

            // SignalR: notificar se aula ficou lotada após esta inscrição
            await _db.Entry(aula).Collection(a => a.Inscricoes).LoadAsync();
            if (aula.Lotada)
                await _hub.Clients.All.SendAsync("AulaLotada", aula.Nome);

            return Ok(new { mensagem = "Inscrição realizada com sucesso." });
        }

        /// <summary>DELETE /api/Inscricoes/Aula/{aulaId} — cancela inscrição.</summary>
        [HttpDelete("Aula/{aulaId}")]
        public async Task<IActionResult> Cancelar(int aulaId)
        {
            var inscricao = await _db.Inscricoes
                .Include(i => i.Aula)
                .FirstOrDefaultAsync(i => i.UtilizadorId == UtilizadorAtualId && i.AulaId == aulaId);

            if (inscricao == null) return NotFound(new { mensagem = "Inscrição não encontrada." });

            var nomeAula = inscricao.Aula?.Nome ?? "";
            _db.Inscricoes.Remove(inscricao);
            await _db.SaveChangesAsync();

            // SignalR: notificar que ficou uma vaga disponível
            await _hub.Clients.All.SendAsync("VagasLiberadas", nomeAula, 1);

            return Ok(new { mensagem = "Inscrição cancelada." });
        }
    }

    /// <summary>
    /// Gestão de planos de treino.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PlanosTreinoController : ControllerBase
    {
        private readonly GymDbContext _db;
        private readonly IHubContext<GymHub> _hub;

        public PlanosTreinoController(GymDbContext db, IHubContext<GymHub> hub)
        {
            _db  = db;
            _hub = hub;
        }

        /// <summary>GET /api/PlanosTreino — lista todos os planos.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var planos = await _db.PlanosTreino
                .Include(p => p.Aulas)
                .Where(p => p.Ativo)
                .OrderBy(p => p.Nome)
                .Select(p => new
                {
                    p.Id, p.Nome, p.Descricao,
                    p.DuracaoMinutos,
                    Nivel      = p.Nivel.ToString(),
                    // TotalAulas removed
                })
                .ToListAsync();

            return Ok(planos);
        }

        /// <summary>GET /api/PlanosTreino/{id}.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var plano = await _db.PlanosTreino.Include(p => p.Aulas).FirstOrDefaultAsync(p => p.Id == id);
            if (plano == null) return NotFound();
            return Ok(plano);
        }

        /// <summary>POST /api/PlanosTreino — cria plano (Admin).</summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] GymManagement.Models.Entities.PlanoTreino dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _db.PlanosTreino.Add(dto);
            await _db.SaveChangesAsync();

            // SignalR: novo plano criado
            await _hub.Clients.All.SendAsync("NovoPlano", dto.Nome);

            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }

        /// <summary>PUT /api/PlanosTreino/{id} — atualiza plano (Admin).</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] GymManagement.Models.Entities.PlanoTreino dto)
        {
            if (id != dto.Id) return BadRequest();
            _db.Entry(dto).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>DELETE /api/PlanosTreino/{id} — elimina plano (Admin).</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var plano = await _db.PlanosTreino.Include(p => p.Aulas).FirstOrDefaultAsync(p => p.Id == id);
            if (plano == null) return NotFound();
            if (plano.Aulas.Any()) return BadRequest(new { mensagem = "Não é possível eliminar um plano com aulas associadas." });

            _db.PlanosTreino.Remove(plano);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
