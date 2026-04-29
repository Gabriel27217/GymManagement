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

        public AulasController(GymDbContext db, IHubContext<GymHub> hub) { _db = db; _hub = hub; }

        private static readonly string[] DiasLabel = { "", "Segunda", "Terça", "Quarta", "Quinta", "Sexta", "Sábado", "Domingo" };

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var aulas = await _db.Aulas
                .Include(a => a.Instrutor).Include(a => a.Sala).Include(a => a.Inscricoes)
                .Where(a => a.Ativa)
                .OrderBy(a => a.DiaSemana).ThenBy(a => a.Hora)
                .ToListAsync();
            return Ok(aulas.Select(MapToDto));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var aula = await _db.Aulas
                .Include(a => a.Instrutor).Include(a => a.Sala).Include(a => a.Inscricoes)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (aula == null) return NotFound();
            return Ok(MapToDto(aula));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] AulaCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var hora = TimeOnly.Parse(dto.Hora);
            var horaFim = hora.AddMinutes(dto.DuracaoMinutos);

            var conflitoSala = await _db.Aulas
                .Where(a => a.SalaId == dto.SalaId
                         && a.DiaSemana == (DiaSemana)dto.DiaSemana
                         && a.Ativa)
                .AnyAsync(a =>
                    (hora >= a.Hora && hora < a.Hora.AddMinutes(a.DuracaoMinutos)) ||
                    (horaFim > a.Hora && horaFim <= a.Hora.AddMinutes(a.DuracaoMinutos)) ||
                    (hora <= a.Hora && horaFim >= a.Hora.AddMinutes(a.DuracaoMinutos))
                );

            if (conflitoSala)
                return BadRequest(new { mensagem = "Já existe uma aula agendada nesta sala neste horário!", tipo = "ConflitoDeSala" });

            var conflitoInstrutor = await _db.Aulas
                .Where(a => a.InstrutorId == dto.InstrutorId
                         && a.DiaSemana == (DiaSemana)dto.DiaSemana
                         && a.Ativa)
                .AnyAsync(a =>
                    (hora >= a.Hora && hora < a.Hora.AddMinutes(a.DuracaoMinutos)) ||
                    (horaFim > a.Hora && horaFim <= a.Hora.AddMinutes(a.DuracaoMinutos)) ||
                    (hora <= a.Hora && horaFim >= a.Hora.AddMinutes(a.DuracaoMinutos))
                );

            if (conflitoInstrutor)
                return BadRequest(new { mensagem = "O instrutor já tem uma aula agendada neste horário!", tipo = "ConflitoDeInstrutor" });

            var aula = new Aula
            {
                Nome = dto.Nome,
                Categoria = (CategoriaAula)dto.Categoria,
                DiaSemana = (DiaSemana)dto.DiaSemana,
                Hora = hora,
                DuracaoMinutos = dto.DuracaoMinutos,
                InstrutorId = dto.InstrutorId,
                SalaId = dto.SalaId
            };

            _db.Aulas.Add(aula);
            await _db.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("NovaAulaCriada", new
            {
                aulaId = aula.Id,
                nome = aula.Nome,
                dia = DiasLabel[(int)aula.DiaSemana],
                hora = aula.Hora.ToString("HH:mm")
            });

            return CreatedAtAction(nameof(GetById), new { id = aula.Id }, new { mensagem = "Aula criada com sucesso.", id = aula.Id });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] AulaCreateDto dto)
        {
            var aula = await _db.Aulas.FindAsync(id);
            if (aula == null) return NotFound();

            var hora = TimeOnly.Parse(dto.Hora);
            var horaFim = hora.AddMinutes(dto.DuracaoMinutos);

            var conflitoSala = await _db.Aulas
                .Where(a => a.Id != id
                         && a.SalaId == dto.SalaId
                         && a.DiaSemana == (DiaSemana)dto.DiaSemana
                         && a.Ativa)
                .AnyAsync(a =>
                    (hora >= a.Hora && hora < a.Hora.AddMinutes(a.DuracaoMinutos)) ||
                    (horaFim > a.Hora && horaFim <= a.Hora.AddMinutes(a.DuracaoMinutos)) ||
                    (hora <= a.Hora && horaFim >= a.Hora.AddMinutes(a.DuracaoMinutos))
                );

            if (conflitoSala)
                return BadRequest(new { mensagem = "Já existe uma aula agendada nesta sala neste horário!", tipo = "ConflitoDeSala" });

            var conflitoInstrutor = await _db.Aulas
                .Where(a => a.Id != id
                         && a.InstrutorId == dto.InstrutorId
                         && a.DiaSemana == (DiaSemana)dto.DiaSemana
                         && a.Ativa)
                .AnyAsync(a =>
                    (hora >= a.Hora && hora < a.Hora.AddMinutes(a.DuracaoMinutos)) ||
                    (horaFim > a.Hora && horaFim <= a.Hora.AddMinutes(a.DuracaoMinutos)) ||
                    (hora <= a.Hora && horaFim >= a.Hora.AddMinutes(a.DuracaoMinutos))
                );

            if (conflitoInstrutor)
                return BadRequest(new { mensagem = "O instrutor já tem uma aula agendada neste horário!", tipo = "ConflitoDeInstrutor" });

            aula.Nome = dto.Nome;
            aula.Categoria = (CategoriaAula)dto.Categoria;
            aula.DiaSemana = (DiaSemana)dto.DiaSemana;
            aula.Hora = hora;
            aula.DuracaoMinutos = dto.DuracaoMinutos;
            aula.InstrutorId = dto.InstrutorId;
            aula.SalaId = dto.SalaId;

            await _db.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("AulaAtualizada", new { aulaId = aula.Id, nome = aula.Nome });

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var aula = await _db.Aulas.FindAsync(id);
            if (aula == null) return NotFound();
            _db.Aulas.Remove(aula);
            await _db.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("AulaEliminada", new { aulaId = id });

            return NoContent();
        }

        // FIX #1/#2: MapToDto agora inclui InstrutorId, SalaId e CategoriaId
        private static AulaListDto MapToDto(Aula a) => new()
        {
            Id = a.Id, Nome = a.Nome,
            Categoria = a.Categoria.ToString(),
            CategoriaId = (int)a.Categoria,
            DiaSemana = (int)a.DiaSemana,
            DiaSemanaLabel = DiasLabel[(int)a.DiaSemana],
            Hora = a.Hora.ToString("HH:mm"),
            HoraFim = a.Hora.AddMinutes(a.DuracaoMinutos).ToString("HH:mm"),
            DuracaoMinutos = a.DuracaoMinutos,
            Instrutor = a.Instrutor?.Nome ?? "",
            InstrutorId = a.InstrutorId,
            Sala = a.Sala?.Nome ?? "",
            SalaId = a.SalaId,
            CapacidadeMaxima = a.Sala?.CapacidadeMaxima ?? 0,
            Inscritos = a.Inscricoes.Count,
            VagasDisponiveis = (a.Sala?.CapacidadeMaxima ?? 0) - a.Inscricoes.Count,
            Lotada = (a.Sala?.CapacidadeMaxima ?? 0) - a.Inscricoes.Count <= 0,
            Ativa = a.Ativa
        };
    }
}
