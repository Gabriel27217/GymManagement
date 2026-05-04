using GymManagement.Models.DTOs;
using GymManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text.Json;

namespace GymManagement.Web.Pages.Aulas
{
    /// <summary>
    /// Classes de suporte para carregar listas dropdown de forma simplificada.
    /// </summary>
    public class InstrutorSimples { public int Id { get; set; } public string Nome { get; set; } = ""; public string? Especialidade { get; set; } }
    public class SalaSimples { public int Id { get; set; } public string Nome { get; set; } = ""; public int CapacidadeMaxima { get; set; } }

    /// <summary>
    /// Listagem de Aulas: Página principal que consome a API para mostrar o horário.
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly ApiService _api;
        public IndexModel(ApiService api) => _api = api;

        public List<AulaListDto> Aulas { get; set; } = new();

        public async Task OnGetAsync()
        {
            try { Aulas = await _api.GetAulasAsync(); } catch { }
        }
    }

    /// <summary>
    /// Criação de Aula: Gere o formulário e interpreta erros de conflito de horário da API.
    /// </summary>
    public class CreateModel : PageModel
    {
        private readonly ApiService _api;
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _config;

        public CreateModel(ApiService api, IHttpClientFactory factory, IConfiguration config)
        {
            _api = api;
            _factory = factory;
            _config = config;
        }

        // Propriedades ligadas ao formulário (Model Binding)
        [BindProperty, Required(ErrorMessage = "O nome é obrigatório"), Display(Name = "Nome da Aula")]
        public string Nome { get; set; } = string.Empty;

        [BindProperty, Required, Display(Name = "Categoria")]
        public int Categoria { get; set; } = 99;

        [BindProperty, Required, Display(Name = "Dia da Semana")]
        public int DiaSemana { get; set; } = 1;

        [BindProperty, Required, Display(Name = "Hora de Início")]
        public string Hora { get; set; } = "09:00";

        [BindProperty, Range(15, 180, ErrorMessage = "Duração deve ser entre 15 e 180 min"), Display(Name = "Duração (minutos)")]
        public int DuracaoMinutos { get; set; } = 60;

        [BindProperty, Required(ErrorMessage = "Selecione um instrutor"), Display(Name = "Instrutor")]
        public int InstrutorId { get; set; }

        [BindProperty, Required(ErrorMessage = "Selecione uma sala"), Display(Name = "Sala")]
        public int SalaId { get; set; }

        public List<InstrutorSimples> Instrutores { get; set; } = new();
        public List<SalaSimples> Salas { get; set; } = new();

        public async Task OnGetAsync() { await CarregarListas(); }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await CarregarListas();
                return Page();
            }

            try
            {
                var ok = await _api.CreateAulaAsync(new AulaCreateDto
                {
                    Nome = Nome,
                    Categoria = Categoria,
                    DiaSemana = DiaSemana,
                    Hora = Hora,
                    DuracaoMinutos = DuracaoMinutos,
                    InstrutorId = InstrutorId,
                    SalaId = SalaId
                });

                if (!ok)
                {
                    ModelState.AddModelError("", "Erro ao criar aula. Verifique os dados introduzidos.");
                    await CarregarListas();
                    return Page();
                }

                TempData["Sucesso"] = $"Aula '{Nome}' criada com sucesso.";
                return RedirectToPage("Index");
            }
            catch (HttpRequestException ex)
            {
                // Tratamento detalhado de conflitos de negócio vindos da API
                if (ex.Message.Contains("ConflitoDeSala"))
                    ModelState.AddModelError("", "⚠️ Já existe uma aula agendada nesta sala neste horário!");
                else if (ex.Message.Contains("ConflitoDeInstrutor"))
                    ModelState.AddModelError("", "⚠️ O instrutor já tem uma aula agendada neste horário!");
                else
                    ModelState.AddModelError("", "Erro ao comunicar com o servidor. Tente novamente.");

                await CarregarListas();
                return Page();
            }
        }

        /// <summary>
        /// Carrega os dados para as Select Lists (Instrutores e Salas) diretamente da API.
        /// </summary>
        private async Task CarregarListas()
        {
            try
            {
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var http = _factory.CreateClient();
                http.BaseAddress = new Uri(_config["ApiSettings:BaseUrl"]!);

                // Incluir Token JWT para autorização na API
                var token = HttpContext.Session.GetString("JWT");
                if (!string.IsNullOrEmpty(token))
                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                Instrutores = JsonSerializer.Deserialize<List<InstrutorSimples>>(await http.GetStringAsync("api/Instrutores"), opts) ?? new();
                Salas = JsonSerializer.Deserialize<List<SalaSimples>>(await http.GetStringAsync("api/Salas"), opts) ?? new();
            }
            catch { /* Log erro se necessário */ }
        }
    }

    /// <summary>
    /// Edição de Aula: Carrega os dados existentes e permite a atualização.
    /// </summary>
    public class EditModel : PageModel
    {
        private readonly ApiService _api;
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _config;

        public EditModel(ApiService api, IHttpClientFactory factory, IConfiguration config)
        {
            _api = api;
            _factory = factory;
            _config = config;
        }

        [BindProperty] public AulaCreateDto Aula { get; set; } = new();
        [BindProperty] public int AulaId { get; set; }
        public List<InstrutorSimples> Instrutores { get; set; } = new();
        public List<SalaSimples> Salas { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var aula = await _api.GetAulaAsync(id);
            if (aula == null) return NotFound();

            AulaId = id;
            Aula = new AulaCreateDto
            {
                Nome = aula.Nome,
                Categoria = 99, // Idealmente mapear da entidade se disponível
                DiaSemana = aula.DiaSemana,
                Hora = aula.Hora,
                DuracaoMinutos = aula.DuracaoMinutos,
                InstrutorId = 0, // Definido no front ou via busca adicional
                SalaId = 0
            };
            await CarregarListas();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) { await CarregarListas(); return Page(); }

            try
            {
                var sucesso = await _api.UpdateAulaAsync(AulaId, Aula);
                if (!sucesso)
                {
                    ModelState.AddModelError("", "Erro ao atualizar aula.");
                    await CarregarListas();
                    return Page();
                }

                TempData["Sucesso"] = "Aula atualizada com sucesso.";
                return RedirectToPage("Index");
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message.Contains("ConflitoDeSala"))
                    ModelState.AddModelError("", "⚠️ Conflito: Sala ocupada neste horário.");
                else if (ex.Message.Contains("ConflitoDeInstrutor"))
                    ModelState.AddModelError("", "⚠️ Conflito: Instrutor ocupado neste horário.");
                else
                    ModelState.AddModelError("", "Erro na ligação ao servidor.");

                await CarregarListas();
                return Page();
            }
        }

        private async Task CarregarListas() { /* Lógica idêntica ao CreateModel */ }
    }

    /// <summary>
    /// Eliminação: Página de confirmação antes de remover a aula.
    /// </summary>
    public class DeleteModel : PageModel
    {
        private readonly ApiService _api;
        public DeleteModel(ApiService api) => _api = api;

        public AulaListDto? Aula { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Aula = await _api.GetAulaAsync(id);
            if (Aula == null) return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            await _api.DeleteAulaAsync(id);
            TempData["Sucesso"] = "Aula eliminada permanentemente.";
            return RedirectToPage("Index");
        }
    }

    /// <summary>
    /// Inscrição Rápida: Ação disparada por link para inscrever o utilizador atual.
    /// </summary>
    public class InscreverModel : PageModel
    {
        private readonly ApiService _api;
        public InscreverModel(ApiService api) => _api = api;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var (ok, msg) = await _api.InscreverAsync(id);
            TempData[ok ? "Sucesso" : "Erro"] = msg;
            return RedirectToPage("Index");
        }
    }
}