using GymManagement.API.Controllers;
using GymManagement.Data;
using GymManagement.Models.DTOs;
using GymManagement.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace GymManagement.Tests.Controllers
{
    /// <summary>
    /// Testes unitários para validação de conflitos de horários em aulas.
    /// </summary>
    public class AulasControllerTests : IDisposable
    {
        private readonly GymDbContext _context;
        private readonly AulasController _controller;
        private readonly Mock<IHubContext<GymHub>> _mockHub;

        public AulasControllerTests()
        {
            // Configurar base de dados em memória
            var options = new DbContextOptionsBuilder<GymDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GymDbContext(options);
            _mockHub = new Mock<IHubContext<GymHub>>();
            _controller = new AulasController(_context, _mockHub.Object);

            // Seed inicial
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Instrutor
            _context.Instrutores.Add(new Instrutor 
            { 
                Id = 1, 
                Nome = "Carlos Silva", 
                Email = "carlos@gym.pt",
                Especialidade = "Yoga"
            });

            // Salas
            _context.Salas.AddRange(
                new Sala { Id = 1, Nome = "Sala A", CapacidadeMaxima = 20 },
                new Sala { Id = 2, Nome = "Sala B", CapacidadeMaxima = 15 }
            );

            // Aula existente: Segunda-feira, 18:00-19:00, Sala A, Instrutor 1
            _context.Aulas.Add(new Aula
            {
                Id = 1,
                Nome = "Yoga Básico",
                Categoria = CategoriaAula.Yoga,
                DiaSemana = DiaSemana.Segunda,
                Hora = new TimeOnly(18, 0),
                DuracaoMinutos = 60,
                InstrutorId = 1,
                SalaId = 1,
                Ativa = true
            });

            _context.SaveChanges();
        }

        /// <summary>
        /// Teste 1: Deve REJEITAR aula com conflito de sala
        /// </summary>
        [Fact]
        public async Task Create_DeveRetornarBadRequest_QuandoHaConflitoSala()
        {
            // Arrange
            var dto = new AulaCreateDto
            {
                Nome = "Pilates Intermédio",
                Categoria = 2, // Pilates
                DiaSemana = 1, // Segunda-feira
                Hora = "18:30", // Sobrepõe com 18:00-19:00
                DuracaoMinutos = 60,
                InstrutorId = 1,
                SalaId = 1 // Mesma sala que aula existente
            };

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value;
            
            // Verificar que a mensagem contém "sala"
            Assert.NotNull(response);
        }

        /// <summary>
        /// Teste 2: Deve REJEITAR aula com conflito de instrutor
        /// </summary>
        [Fact]
        public async Task Create_DeveRetornarBadRequest_QuandoHaConflitoInstrutor()
        {
            // Arrange
            var dto = new AulaCreateDto
            {
                Nome = "Pilates Manhã",
                Categoria = 2,
                DiaSemana = 1, // Segunda-feira
                Hora = "18:00", // Mesmo horário
                DuracaoMinutos = 60,
                InstrutorId = 1, // Mesmo instrutor
                SalaId = 2 // SALA DIFERENTE (mas instrutor igual)
            };

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        /// <summary>
        /// Teste 3: Deve ACEITAR aula SEM conflitos
        /// </summary>
        [Fact]
        public async Task Create_DeveRetornarCreated_QuandoNaoHaConflito()
        {
            // Arrange
            var dto = new AulaCreateDto
            {
                Nome = "Pilates Avançado",
                Categoria = 2,
                DiaSemana = 2, // TERÇA-FEIRA (dia diferente)
                Hora = "18:00",
                DuracaoMinutos = 60,
                InstrutorId = 1,
                SalaId = 1
            };

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.NotNull(createdResult.Value);

            // Verificar que foi realmente criada na BD
            var aulasCriadas = await _context.Aulas.CountAsync();
            Assert.Equal(2, aulasCriadas); // 1 inicial + 1 nova
        }

        /// <summary>
        /// Teste 4: Deve ACEITAR aula logo APÓS outra (sem sobreposição)
        /// </summary>
        [Fact]
        public async Task Create_DeveAceitar_AulaLogoAposOutra()
        {
            // Arrange
            var dto = new AulaCreateDto
            {
                Nome = "Spinning",
                Categoria = 3,
                DiaSemana = 1, // Segunda-feira
                Hora = "19:00", // LOGO APÓS 18:00-19:00
                DuracaoMinutos = 60,
                InstrutorId = 1,
                SalaId = 1
            };

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.NotNull(createdResult.Value);
        }

        /// <summary>
        /// Teste 5: Deve REJEITAR aula que ENGLOBA outra completamente
        /// </summary>
        [Fact]
        public async Task Create_DeveRejeitarAula_QueEngloba()
        {
            // Arrange
            var dto = new AulaCreateDto
            {
                Nome = "Aula Longa",
                Categoria = 99,
                DiaSemana = 1, // Segunda-feira
                Hora = "17:00", // ANTES de 18:00
                DuracaoMinutos = 180, // 3 horas (17:00-20:00, engloba 18:00-19:00)
                InstrutorId = 1,
                SalaId = 1
            };

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        /// <summary>
        /// Teste 6: Update - Deve REJEITAR edição que causa conflito
        /// </summary>
        [Fact]
        public async Task Update_DeveRejeitarEdicao_QueCausaConflito()
        {
            // Criar segunda aula sem conflito
            var aula2 = new Aula
            {
                Id = 2,
                Nome = "Zumba",
                Categoria = CategoriaAula.Zumba,
                DiaSemana = DiaSemana.Terca,
                Hora = new TimeOnly(20, 0),
                DuracaoMinutos = 60,
                InstrutorId = 1,
                SalaId = 2,
                Ativa = true
            };
            _context.Aulas.Add(aula2);
            await _context.SaveChangesAsync();

            // Tentar editar para conflitar com aula ID=1
            var dto = new AulaCreateDto
            {
                Nome = "Zumba Atualizada",
                Categoria = 5,
                DiaSemana = 1, // Mudar para SEGUNDA
                Hora = "18:30", // Conflita com 18:00-19:00
                DuracaoMinutos = 60,
                InstrutorId = 1,
                SalaId = 1 // Mesma sala
            };

            // Act
            var result = await _controller.Update(2, dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        /// <summary>
        /// Teste 7: Update - Deve ACEITAR edição da PRÓPRIA aula sem mudança de horário
        /// </summary>
        [Fact]
        public async Task Update_DeveAceitarEdicao_DaPrópriaAula()
        {
            // Arrange - Editar aula ID=1, mantendo horário
            var dto = new AulaCreateDto
            {
                Nome = "Yoga Avançado", // Mudar apenas o nome
                Categoria = 1,
                DiaSemana = 1,
                Hora = "18:00", // MESMO horário
                DuracaoMinutos = 60,
                InstrutorId = 1,
                SalaId = 1
            };

            // Act
            var result = await _controller.Update(1, dto);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verificar que o nome mudou
            var aulaAtualizada = await _context.Aulas.FindAsync(1);
            Assert.Equal("Yoga Avançado", aulaAtualizada!.Nome);
        }

        /// <summary>
        /// Teste 8: Verificar cenário de sobreposição parcial no INÍCIO
        /// </summary>
        [Fact]
        public async Task Create_DeveRejeitarSobreposicao_NoInicio()
        {
            // Nova aula: 17:30-18:30 (sobrepõe últimos 30min de 18:00-19:00)
            var dto = new AulaCreateDto
            {
                Nome = "Teste Início",
                Categoria = 99,
                DiaSemana = 1,
                Hora = "17:30",
                DuracaoMinutos = 60, // Termina às 18:30
                InstrutorId = 1,
                SalaId = 1
            };

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        /// <summary>
        /// Teste 9: Verificar cenário de sobreposição parcial no FIM
        /// </summary>
        [Fact]
        public async Task Create_DeveRejeitarSobreposicao_NoFim()
        {
            // Nova aula: 18:30-19:30 (sobrepõe primeiros 30min de 18:00-19:00)
            var dto = new AulaCreateDto
            {
                Nome = "Teste Fim",
                Categoria = 99,
                DiaSemana = 1,
                Hora = "18:30",
                DuracaoMinutos = 60,
                InstrutorId = 1,
                SalaId = 1
            };

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        /// <summary>
        /// Teste 10: Múltiplas aulas no mesmo dia SEM conflito
        /// </summary>
        [Fact]
        public async Task Create_DeveAceitarMultiplasAulas_SemConflito()
        {
            // Aula 1: 09:00-10:00
            // Aula 2: 10:00-11:00
            // Aula 3: 11:00-12:00

            var aulas = new[]
            {
                new AulaCreateDto { Nome = "Manhã 1", Categoria = 1, DiaSemana = 1, Hora = "09:00", DuracaoMinutos = 60, InstrutorId = 1, SalaId = 1 },
                new AulaCreateDto { Nome = "Manhã 2", Categoria = 1, DiaSemana = 1, Hora = "10:00", DuracaoMinutos = 60, InstrutorId = 1, SalaId = 1 },
                new AulaCreateDto { Nome = "Manhã 3", Categoria = 1, DiaSemana = 1, Hora = "11:00", DuracaoMinutos = 60, InstrutorId = 1, SalaId = 1 }
            };

            foreach (var dto in aulas)
            {
                var result = await _controller.Create(dto);
                Assert.IsType<CreatedAtActionResult>(result);
            }

            // Verificar que todas foram criadas
            var total = await _context.Aulas.CountAsync();
            Assert.Equal(4, total); // 1 inicial + 3 novas
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
