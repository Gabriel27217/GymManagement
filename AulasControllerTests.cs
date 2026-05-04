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
    /// Garante que não existem sobreposições de salas ou instrutores no mesmo período.
    /// </summary>
    public class AulasControllerTests : IDisposable
    {
        private readonly GymDbContext _context;
        private readonly AulasController _controller;
        private readonly Mock<IHubContext<GymHub>> _mockHub;

        /// <summary>
        /// Construtor de testes: Configura o ambiente isolado com uma base de dados em memória.
        /// </summary>
        public AulasControllerTests()
        {
            // Configurar base de dados em memória para isolamento total dos testes
            var options = new DbContextOptionsBuilder<GymDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GymDbContext(options);
            _mockHub = new Mock<IHubContext<GymHub>>();
            _controller = new AulasController(_context, _mockHub.Object);

            // Inserção de dados iniciais para suporte aos testes
            SeedTestData();
        }

        /// <summary>
        /// Popula a base de dados temporária com dados de teste (Instrutor, Salas e uma Aula base).
        /// </summary>
        private void SeedTestData()
        {
            // Registo de Instrutor de teste
            _context.Instrutores.Add(new Instrutor
            {
                Id = 1,
                Nome = "Carlos Silva",
                Email = "carlos@gym.pt",
                Especialidade = "Yoga"
            });

            // Registo de Salas de teste
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
        /// Valida que o sistema impede a criação de uma aula na mesma sala e horário de uma já existente.
        /// </summary>
        [Fact]
        public async Task Create_DeveRetornarBadRequest_QuandoHaConflitoSala()
        {
            // Arrange - Tentativa de criar aula que sobrepõe o horário na mesma sala
            var dto = new AulaCreateDto
            {
                Nome = "Pilates Intermédio",
                Categoria = 2, // Pilates
                DiaSemana = 1, // Segunda-feira
                Hora = "18:30", // Sobrepõe com a aula das 18:00-19:00
                DuracaoMinutos = 60,
                InstrutorId = 1,
                SalaId = 1 // Conflito de sala
            };

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        /// <summary>
        /// Valida que o sistema impede que um instrutor seja alocado em duas aulas diferentes no mesmo horário.
        /// </summary>
        [Fact]
        public async Task Create_DeveRetornarBadRequest_QuandoHaConflitoInstrutor()
        {
            // Arrange - Mesmo horário e instrutor, mas em sala diferente
            var dto = new AulaCreateDto
            {
                Nome = "Pilates Manhã",
                Categoria = 2,
                DiaSemana = 1,
                Hora = "18:00",
                DuracaoMinutos = 60,
                InstrutorId = 1, // Conflito de instrutor
                SalaId = 2
            };

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        /// <summary>
        /// Valida a criação bem-sucedida de uma aula quando não existem conflitos de agenda.
        /// </summary>
        [Fact]
        public async Task Create_DeveRetornarCreated_QuandoNaoHaConflito()
        {
            // Arrange - Aula num dia diferente (Terça-feira)
            var dto = new AulaCreateDto
            {
                Nome = "Pilates Avançado",
                Categoria = 2,
                DiaSemana = 2,
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

            // Verificar persistência na base de dados
            var aulasCriadas = await _context.Aulas.CountAsync();
            Assert.Equal(2, aulasCriadas);
        }

        /// <summary>
        /// Valida que aulas consecutivas (uma terminando no exato momento em que a outra começa) são permitidas.
        /// </summary>
        [Fact]
        public async Task Create_DeveAceitar_AulaLogoAposOutra()
        {
            // Arrange - Aula começa às 19:00, exatamente quando a anterior termina
            var dto = new AulaCreateDto
            {
                Nome = "Spinning",
                Categoria = 3,
                DiaSemana = 1,
                Hora = "19:00",
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
        /// Valida a rejeição de uma aula que "engloba" totalmente o horário de uma aula já existente.
        /// </summary>
        [Fact]
        public async Task Create_DeveRejeitarAula_QueEngloba()
        {
            // Arrange - Aula de 3 horas que apanha o intervalo da aula existente
            var dto = new AulaCreateDto
            {
                Nome = "Aula Longa",
                Categoria = 99,
                DiaSemana = 1,
                Hora = "17:00",
                DuracaoMinutos = 180, // Termina às 20:00 (conflito total)
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
        /// Valida que a atualização de uma aula não pode resultar num novo conflito com terceiros.
        /// </summary>
        [Fact]
        public async Task Update_DeveRejeitarEdicao_QueCausaConflito()
        {
            // Arrange - Criar uma segunda aula válida noutro dia
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

            // Tentar editar a aula 2 para o horário e sala da aula 1
            var dto = new AulaCreateDto
            {
                Nome = "Zumba Atualizada",
                Categoria = 5,
                DiaSemana = 1,
                Hora = "18:30",
                DuracaoMinutos = 60,
                InstrutorId = 1,
                SalaId = 1
            };

            // Act
            var result = await _controller.Update(2, dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        /// <summary>
        /// Garante que é possível editar campos de uma aula (como o nome) sem disparar falsos positivos de conflito de horário com ela própria.
        /// </summary>
        [Fact]
        public async Task Update_DeveAceitarEdicao_DaPrópriaAula()
        {
            // Arrange - Alterar apenas o nome da aula 1, mantendo o horário
            var dto = new AulaCreateDto
            {
                Nome = "Yoga Avançado",
                Categoria = 1,
                DiaSemana = 1,
                Hora = "18:00",
                DuracaoMinutos = 60,
                InstrutorId = 1,
                SalaId = 1
            };

            // Act
            var result = await _controller.Update(1, dto);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var aulaAtualizada = await _context.Aulas.FindAsync(1);
            Assert.Equal("Yoga Avançado", aulaAtualizada!.Nome);
        }

        /// <summary>
        /// Valida a rejeição quando a nova aula termina após o início de uma aula existente.
        /// </summary>
        [Fact]
        public async Task Create_DeveRejeitarSobreposicao_NoInicio()
        {
            // Arrange - Aula das 17:30 às 18:30 (conflita com o início da aula das 18:00)
            var dto = new AulaCreateDto
            {
                Nome = "Teste Início",
                Categoria = 99,
                DiaSemana = 1,
                Hora = "17:30",
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
        /// Valida a rejeição quando a nova aula começa antes do fim de uma aula existente.
        /// </summary>
        [Fact]
        public async Task Create_DeveRejeitarSobreposicao_NoFim()
        {
            // Arrange - Aula começa às 18:30 (aula existente termina às 19:00)
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
        /// Valida a criação sequencial de múltiplas aulas sem qualquer sobreposição.
        /// </summary>
        [Fact]
        public async Task Create_DeveAceitarMultiplasAulas_SemConflito()
        {
            // Arrange - Sequência de 3 aulas consecutivas
            var aulas = new[]
            {
                new AulaCreateDto { Nome = "Manhã 1", Categoria = 1, DiaSemana = 1, Hora = "09:00", DuracaoMinutos = 60, InstrutorId = 1, SalaId = 1 },
                new AulaCreateDto { Nome = "Manhã 2", Categoria = 1, DiaSemana = 1, Hora = "10:00", DuracaoMinutos = 60, InstrutorId = 1, SalaId = 1 },
                new AulaCreateDto { Nome = "Manhã 3", Categoria = 1, DiaSemana = 1, Hora = "11:00", DuracaoMinutos = 60, InstrutorId = 1, SalaId = 1 }
            };

            // Act & Assert
            foreach (var dto in aulas)
            {
                var result = await _controller.Create(dto);
                Assert.IsType<CreatedAtActionResult>(result);
            }

            var total = await _context.Aulas.CountAsync();
            Assert.Equal(4, total); // 1 inicial + 3 novas
        }

        /// <summary>
        /// Limpa os recursos e a base de dados em memória após a execução de cada teste.
        /// </summary>
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
