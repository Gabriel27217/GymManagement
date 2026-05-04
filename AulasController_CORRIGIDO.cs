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
    /// <summary>
    /// Controlador responsável pela gestão das aulas de grupo do ginásio.
    /// Permite a consulta, criação, atualização e eliminação de aulas, 
    /// garantindo a integridade dos horários e notificando os clientes em tempo real.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AulasController : ControllerBase
    {
        private readonly GymDbContext _db;
        private readonly IHubContext<GymHub> _hub;

        /// <summary>
        /// Inicializa uma nova instância do controlador com o contexto da base de dados e o hub SignalR.
        /// </summary>
        /// <param name="db">Contexto da base de dados.</param>
        /// <param name="hub">Contexto do hub para notificações em tempo real.</param>
        public AulasController(GymDbContext db, IHubContext<GymHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        private static readonly string[] DiasLabel = { "", "Segunda", "Terça", "Quarta", "Quinta", "Sexta", "Sábado", "Domingo" };

        /// <summary>
        /// Obtém a lista de todas as aulas ativas no ginásio.
        /// </summary>
        /// <returns>Uma lista de objetos <see cref="AulaListDto"/>.</returns>
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

        /// <summary>
        /// Obtém os detalhes de uma aula específica através do seu identificador.
        /// </summary>
        /// <param name="id">ID da aula.</param>
        /// <returns>Os detalhes da aula ou NotFound caso não exista.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var aula = await _db.Aulas
                .Include(a => a.Instrutor).Include(a => a.Sala).Include(a => a.Inscricoes)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (aula == null) return NotFound();
            return Ok(MapToDto(aula));
        }

        /// <summary>
        /// Cria uma nova aula no sistema, validando conflitos de sala e instrutor.
        /// Requer privilégios de Administrador.
        /// </summary>
        /// <param name="dto">Dados para a criação da aula.</param>
        /// <returns>Resultado da criação com o objeto criado.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] AulaCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var hora = TimeOnly.Parse(dto.Hora);
            var horaFim = hora.AddMinutes(dto.DuracaoMinutos);

            // ========================================
            // VALIDAÇÃO 1: CONFLITO DE HORÁRIO NA SALA
            // ========================================
            var conflitoSala = await _db.Aulas
                .Where(a => a.SalaId == dto.SalaId
                         && a.DiaSemana == (DiaSemana)dto.DiaSemana
                         && a.Ativa)
                .AnyAsync(a =>
                    // A nova aula começa durante uma aula existente
                    (hora >= a.Hora && hora < a.Hora.AddMinutes(a.DuracaoMinutos)) ||
                    // A nova aula termina durante uma aula existente
                    (horaFim > a.Hora && horaFim <= a.Hora.AddMinutes(a.DuracaoMinutos)) ||
                    // A nova aula engloba completamente uma aula existente
                    (hora <= a.Hora && horaFim >= a.Hora.AddMinutes(a.DuracaoMinutos))
                );

            if (conflitoSala)
            {
                return BadRequest(new
                {
                    mensagem = "Já existe uma aula agendada nesta sala neste horário!",
                    tipo = "ConflitoDeSala"
                });
            }

            // ========================================
            // VALIDAÇÃO 2: CONFLITO DE HORÁRIO DO INSTRUTOR
            // ========================================
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
            {
                return BadRequest(new
                {
                    mensagem = "O instrutor já tem uma aula agendada neste horário!",
                    tipo = "ConflitoDeInstrutor"
                });
            }

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

            // Notificar todos os clientes ligados via SignalR sobre a nova aula
            await _hub.Clients.All.SendAsync("NovaAulaCriada", new
            {
                aulaId = aula.Id,
                nome = aula.Nome,
                dia = DiasLabel[(int)aula.DiaSemana],
                hora = aula.Hora.ToString("HH:mm")
            });

            return CreatedAtAction(nameof(GetById), new { id = aula.Id }, new { mensagem = "Aula criada com sucesso.", id = aula.Id });
        }

        /// <summary>
        /// Atualiza os dados de uma aula existente.
        /// Verifica se as novas alterações não causam conflitos com outras aulas.
        /// </summary>
        /// <param name="id">ID da aula a editar.</param>
        /// <param name="dto">Novos dados da aula.</param>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] AulaCreateDto dto)
        {
            var aula = await _db.Aulas.FindAsync(id);
            if (aula == null) return NotFound();

            var hora = TimeOnly.Parse(dto.Hora);
            var horaFim = hora.AddMinutes(dto.DuracaoMinutos);

            // ========================================
            // VALIDAÇÃO 1: CONFLITO DE HORÁRIO NA SALA (EXCLUINDO A PRÓPRIA AULA)
            // ========================================
            var conflitoSala = await _db.Aulas
                .Where(a => a.Id != id  // IMPORTANTE: Ignorar a própria aula na verificação
                         && a.SalaId == dto.SalaId
                         && a.DiaSemana == (DiaSemana)dto.DiaSemana
                         && a.Ativa)
                .AnyAsync(a =>
                    (hora >= a.Hora && hora < a.Hora.AddMinutes(a.DuracaoMinutos)) ||
                    (horaFim > a.Hora && horaFim <= a.Hora.AddMinutes(a.DuracaoMinutos)) ||
                    (hora <= a.Hora && horaFim >= a.Hora.AddMinutes(a.DuracaoMinutos))
                );

            if (conflitoSala)
            {
                return BadRequest(new
                {
                    mensagem = "Já existe uma aula agendada nesta sala neste horário!",
                    tipo = "ConflitoDeSala"
                });
            }

            // ========================================
            // VALIDAÇÃO 2: CONFLITO DE HORÁRIO DO INSTRUTOR (EXCLUINDO A PRÓPRIA AULA)
            // ========================================
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
            {
                return BadRequest(new
                {
                    mensagem = "O instrutor já tem uma aula agendada neste horário!",
                    tipo = "ConflitoDeInstrutor"
                });
            }

            aula.Nome = dto.Nome;
            aula.Categoria = (CategoriaAula)dto.Categoria;
            aula.DiaSemana = (DiaSemana)dto.DiaSemana;
            aula.Hora = hora;
            aula.DuracaoMinutos = dto.DuracaoMinutos;
            aula.InstrutorId = dto.InstrutorId;
            aula.SalaId = dto.SalaId;

            await _db.SaveChangesAsync();

            // Notificar atualização via SignalR
            await _hub.Clients.All.SendAsync("AulaAtualizada", new
            {
                aulaId = aula.Id,
                nome = aula.Nome
            });

            return NoContent();
        }

        /// <summary>
        /// Remove permanentemente uma aula da base de dados.
        /// </summary>
        /// <param name="id">ID da aula a eliminar.</param>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var aula = await _db.Aulas.FindAsync(id);
            if (aula == null) return NotFound();

            _db.Aulas.Remove(aula);
            await _db.SaveChangesAsync();

            // Notificar eliminação via SignalR para atualização imediata na interface dos clientes
            await _hub.Clients.All.SendAsync("AulaEliminada", new { aulaId = id });

            return NoContent();
        }

        /// <summary>
        /// Método auxiliar para mapear a entidade Aula para o seu DTO de visualização.
        /// </summary>
        /// <param name="a">Objeto da entidade Aula.</param>
        /// <returns>Um DTO formatado para o cliente.</returns>
        private static AulaListDto MapToDto(Aula a) => new()
        {
            Id = a.Id,
            Nome = a.Nome,
            Categoria = a.Categoria.ToString(),
            DiaSemana = (int)a.DiaSemana,
            DiaSemanaLabel = DiasLabel[(int)a.DiaSemana],
            Hora = a.Hora.ToString("HH:mm"),
            HoraFim = a.Hora.AddMinutes(a.DuracaoMinutos).ToString("HH:mm"),
            DuracaoMinutos = a.DuracaoMinutos,
            Instrutor = a.Instrutor?.Nome ?? "",
            Sala = a.Sala?.Nome ?? "",
            CapacidadeMaxima = a.Sala?.CapacidadeMaxima ?? 0,
            Inscritos = a.Inscricoes.Count,
            VagasDisponiveis = (a.Sala?.CapacidadeMaxima ?? 0) - a.Inscricoes.Count,
            Lotada = (a.Sala?.CapacidadeMaxima ?? 0) - a.Inscricoes.Count <= 0,
            Ativa = a.Ativa
        };
    }
}