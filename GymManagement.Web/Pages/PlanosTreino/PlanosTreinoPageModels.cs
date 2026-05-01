using GymManagement.Models.DTOs;
using GymManagement.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GymManagement.Web.Pages.PlanosTreino
{
    public partial class IndexModel : PageModel
    {
        private readonly ApiService _api;
        public IndexModel(ApiService api) => _api = api;
        public List<PlanoTreinoDto> Planos { get; set; } = new();
        public async Task OnGetAsync() { try { Planos = await _api.GetPlanosAsync(); } catch { } }
    }

    public partial class CreateModel : PageModel
    {
        private readonly ApiService _api;
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _config;

        public CreateModel(ApiService api, IHttpClientFactory factory, IConfiguration config)
        { _api = api; _factory = factory; _config = config; }

        [BindProperty, Required, Display(Name = "Nome do Plano")]
        public string Nome { get; set; } = string.Empty;

        [BindProperty, Required, Display(Name = "Descrição")]
        public string Descricao { get; set; } = string.Empty;

        [BindProperty, Range(15, 180), Display(Name = "Duração (minutos)")]
        public int DuracaoMinutos { get; set; } = 60;

        [BindProperty, Display(Name = "Nível")]
        public int Nivel { get; set; } = 0;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var http = _factory.CreateClient();
            http.BaseAddress = new Uri(_config["ApiSettings:BaseUrl"]!);
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JWT"));

            var body = new { Nome, Descricao, DuracaoMinutos, Nivel, Ativo = true, DataCriacao = DateTime.Now };
            var resp = await http.PostAsync("api/PlanosTreino",
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

            if (!resp.IsSuccessStatusCode)
            { ModelState.AddModelError("", "Erro ao criar plano."); return Page(); }

            TempData["Sucesso"] = $"Plano '{Nome}' criado com sucesso.";
            return RedirectToPage("Index");
        }
    }

    public partial class EditModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _config;

        public EditModel(IHttpClientFactory factory, IConfiguration config)
        { _factory = factory; _config = config; }

        [BindProperty] public int PlanoId { get; set; }
        [BindProperty, Required, Display(Name = "Nome do Plano")] public string Nome { get; set; } = string.Empty;
        [BindProperty, Required, Display(Name = "Descrição")] public string Descricao { get; set; } = string.Empty;
        [BindProperty, Range(15, 180), Display(Name = "Duração (minutos)")] public int DuracaoMinutos { get; set; } = 60;
        [BindProperty, Display(Name = "Nível")] public int Nivel { get; set; } = 0;
        [BindProperty, Display(Name = "Objetivo")] public int Objetivo { get; set; } = 0;
        [BindProperty, Display(Name = "Ativo")] public bool Ativo { get; set; } = true;

        private HttpClient CriarCliente()
        {
            var http = _factory.CreateClient();
            http.BaseAddress = new Uri(_config["ApiSettings:BaseUrl"]!);
            var token = HttpContext.Session.GetString("JWT") ?? User.FindFirst("jwt")?.Value ?? "";
            if (!string.IsNullOrEmpty(token))
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return http;
        }

        public async Task<IActionResult> OnGetAsync([FromRoute] int id)
        {
            try
            {
                var http = CriarCliente();
                var resp = await http.GetAsync($"api/PlanosTreino/{id}");
                if (!resp.IsSuccessStatusCode)
                {
                    TempData["Erro"] = $"Não foi possível carregar o plano (HTTP {(int)resp.StatusCode}).";
                    return RedirectToPage("Index");
                }
                var json = await resp.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var root = doc.RootElement;
                PlanoId = root.GetProperty("id").GetInt32();
                Nome = root.GetProperty("nome").GetString() ?? "";
                Descricao = root.GetProperty("descricao").GetString() ?? "";
                DuracaoMinutos = root.GetProperty("duracaoMinutos").GetInt32();
                Nivel = root.GetProperty("nivel").GetInt32();
                Objetivo = root.GetProperty("objetivo").GetInt32();
                Ativo = root.GetProperty("ativo").GetBoolean();
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro ao ligar à API: {ex.Message}";
                return RedirectToPage("Index");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var http = CriarCliente();
            var body = new { Id = PlanoId, Nome, Descricao, DuracaoMinutos, Nivel, Objetivo, Ativo };
            var resp = await http.PutAsync($"api/PlanosTreino/{PlanoId}",
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

            if (!resp.IsSuccessStatusCode)
            { ModelState.AddModelError("", $"Erro ao guardar plano (HTTP {(int)resp.StatusCode})."); return Page(); }

            TempData["Sucesso"] = $"Plano '{Nome}' atualizado com sucesso.";
            return RedirectToPage("Index");
        }
    }

    public partial class DeleteModel : PageModel
    {
        private readonly ApiService _api;
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _config;

        public DeleteModel(ApiService api, IHttpClientFactory factory, IConfiguration config)
        { _api = api; _factory = factory; _config = config; }

        public PlanoTreinoDto? Plano { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var planos = await _api.GetPlanosAsync();
            Plano = planos.FirstOrDefault(p => p.Id == id);
            if (Plano == null) return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var http = _factory.CreateClient();
            http.BaseAddress = new Uri(_config["ApiSettings:BaseUrl"]!);
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JWT"));

            var resp = await http.DeleteAsync($"api/PlanosTreino/{id}");
            if (!resp.IsSuccessStatusCode)
            {
                var msg = JsonSerializer.Deserialize<Dictionary<string, string>>(
                    await resp.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                TempData["Erro"] = msg?.GetValueOrDefault("mensagem") ?? "Não foi possível eliminar o plano.";
            }
            else
                TempData["Sucesso"] = "Plano eliminado com sucesso.";

            return RedirectToPage("Index");
        }
    }
}
