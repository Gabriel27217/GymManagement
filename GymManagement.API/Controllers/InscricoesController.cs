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
            _db = db;
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
                        Categoria = i.Aula.Categoria.ToString(),
                        DiaSemana = (int)i.Aula.DiaSemana,
                        DiaSemanaLabel = i.Aula.DiaSemana.ToString(),
                        Hora = i.Aula.Hora.ToString("HH:mm"),
                        i.Aula.DuracaoMinutos,
                        Instrutor = i.Aula.Instrutor!.Nome,
                        Sala = i.Aula.Sala!.Nome
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

            await _hub.Clients.All.SendAsync("VagasLiberadas", nomeAula, 1);

            return Ok(new { mensagem = "Inscrição cancelada." });
        }
    }
}