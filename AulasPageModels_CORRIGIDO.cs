using GymManagement.Models.DTOs;
using GymManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text.Json;

namespace GymManagement.Web.Pages.Aulas
{
    public class InstrutorSimples { public int Id { get; set; } public string Nome { get; set; } = ""; public string? Especialidade { get; set; } }
    public class SalaSimples { public int Id { get; set; } public string Nome { get; set; } = ""; public int CapacidadeMaxima { get; set; } }

    public class IndexModel : PageModel
    {
        private readonly ApiService _api;
        public IndexModel(ApiService api) => _api = api;
        public List<AulaListDto> Aulas { get; set; } = new();
        public async Task OnGetAsync() { try { Aulas = await _api.GetAulasAsync(); } catch { } }
    }

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

        [BindProperty, Required, Display(Name = "Nome da Aula")] public string Nome { get; set; } = string.Empty;
        [BindProperty, Required, Display(Name = "Categoria")] public int Categoria { get; set; } = 99;
        [BindProperty, Required, Display(Name = "Dia da Semana")] public int DiaSemana { get; set; } = 1;
        [BindProperty, Required, Display(Name = "Hora")] public string Hora { get; set; } = "09:00";
        [BindProperty, Range(15, 180), Display(Name = "Duração (minutos)")] public int DuracaoMinutos { get; set; } = 60;
        [BindProperty, Required, Display(Name = "Instrutor")] public int InstrutorId { get; set; }
        [BindProperty, Required, Display(Name = "Sala")] public int SalaId { get; set; }

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
                    ModelState.AddModelError("", "Erro ao criar aula. Verifique se não há conflitos de horário.");
                    await CarregarListas(); 
                    return Page(); 
                }

                TempData["Sucesso"] = $"Aula '{Nome}' criada com sucesso.";
                return RedirectToPage("Index");
            }
            catch (HttpRequestException ex)
            {
                // Tentar extrair mensagem de erro da API
                if (ex.Message.Contains("ConflitoDeSala"))
                {
                    ModelState.AddModelError("", "⚠️ Já existe uma aula agendada nesta sala neste horário!");
                }
                else if (ex.Message.Contains("ConflitoDeInstrutor"))
                {
                    ModelState.AddModelError("", "⚠️ O instrutor já tem uma aula agendada neste horário!");
                }
                else
                {
                    ModelState.AddModelError("", "Erro ao comunicar com o servidor. Tente novamente.");
                }
                
                await CarregarListas();
                return Page();
            }
        }

        private async Task CarregarListas()
        {
            try
            {
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var http = _factory.CreateClient();
                http.BaseAddress = new Uri(_config["ApiSettings:BaseUrl"]!);
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JWT"));
                Instrutores = JsonSerializer.Deserialize<List<InstrutorSimples>>(await http.GetStringAsync("api/Instrutores"), opts) ?? new();
                Salas = JsonSerializer.Deserialize<List<SalaSimples>>(await http.GetStringAsync("api/Salas"), opts) ?? new();
            }
            catch { }
        }
    }

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
                Categoria = 99, 
                DiaSemana = aula.DiaSemana, 
                Hora = aula.Hora, 
                DuracaoMinutos = aula.DuracaoMinutos,
                InstrutorId = 0,  // Precisa ser carregado da API
                SalaId = 0        // Precisa ser carregado da API
            };
            await CarregarListas();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) 
            { 
                await CarregarListas(); 
                return Page(); 
            }

            try
            {
                var sucesso = await _api.UpdateAulaAsync(AulaId, Aula);
                
                if (!sucesso)
                {
                    ModelState.AddModelError("", "Erro ao atualizar aula. Verifique se não há conflitos de horário.");
                    await CarregarListas();
                    return Page();
                }

                TempData["Sucesso"] = "Aula atualizada com sucesso.";
                return RedirectToPage("Index");
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message.Contains("ConflitoDeSala"))
                {
                    ModelState.AddModelError("", "⚠️ Já existe uma aula agendada nesta sala neste horário!");
                }
                else if (ex.Message.Contains("ConflitoDeInstrutor"))
                {
                    ModelState.AddModelError("", "⚠️ O instrutor já tem uma aula agendada neste horário!");
                }
                else
                {
                    ModelState.AddModelError("", "Erro ao comunicar com o servidor.");
                }
                
                await CarregarListas();
                return Page();
            }
        }

        private async Task CarregarListas()
        {
            try
            {
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var http = _factory.CreateClient();
                http.BaseAddress = new Uri(_config["ApiSettings:BaseUrl"]!);
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JWT"));
                Instrutores = JsonSerializer.Deserialize<List<InstrutorSimples>>(await http.GetStringAsync("api/Instrutores"), opts) ?? new();
                Salas = JsonSerializer.Deserialize<List<SalaSimples>>(await http.GetStringAsync("api/Salas"), opts) ?? new();
            }
            catch { }
        }
    }

    public class DeleteModel : PageModel
    {
        private readonly ApiService _api;
        public DeleteModel(ApiService api) => _api = api;
        public AulaListDto? Aula { get; set; }
        public async Task<IActionResult> OnGetAsync(int id) { Aula = await _api.GetAulaAsync(id); if (Aula == null) return NotFound(); return Page(); }
        public async Task<IActionResult> OnPostAsync(int id) { await _api.DeleteAulaAsync(id); TempData["Sucesso"] = "Aula eliminada."; return RedirectToPage("Index"); }
    }

    public class InscreverModel : PageModel
    {
        private readonly ApiService _api;
        public InscreverModel(ApiService api) => _api = api;
        public async Task<IActionResult> OnGetAsync(int id) { var (ok, msg) = await _api.InscreverAsync(id); TempData[ok ? "Sucesso" : "Erro"] = msg; return RedirectToPage("Index"); }
    }
}
