// ===== SALAS =====
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GymManagement.Web.Pages.Salas
{
    public class SalaDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int CapacidadeMaxima { get; set; }
        public string? Descricao { get; set; }
        public bool Ativa { get; set; }
        public int TotalAulas { get; set; }
    }

    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _f; private readonly IConfiguration _c;
        public IndexModel(IHttpClientFactory f, IConfiguration c) { _f = f; _c = c; }
        public List<SalaDto> Salas { get; set; } = new();
        public async Task OnGetAsync()
        {
            try { var h = Http(); var j = await h.GetStringAsync("api/Salas"); Salas = JsonSerializer.Deserialize<List<SalaDto>>(j, Opts()) ?? new(); } catch { }
        }
        private HttpClient Http() { var h = _f.CreateClient(); h.BaseAddress = new Uri(_c["ApiSettings:BaseUrl"]!); h.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JWT")); return h; }
        private static JsonSerializerOptions Opts() => new() { PropertyNameCaseInsensitive = true };
    }

    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _f; private readonly IConfiguration _c;
        public CreateModel(IHttpClientFactory f, IConfiguration c) { _f = f; _c = c; }
        [BindProperty, Required, Display(Name = "Nome")] public string Nome { get; set; } = string.Empty;
        [BindProperty, Required, Range(1, 500), Display(Name = "Capacidade Máxima")] public int CapacidadeMaxima { get; set; } = 20;
        [BindProperty, Display(Name = "Descrição")] public string? Descricao { get; set; }
        public void OnGet() { }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            var h = Http(); var body = new { Nome, CapacidadeMaxima, Descricao, Ativa = true };
            var resp = await h.PostAsync("api/Salas", new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));
            if (!resp.IsSuccessStatusCode) { ModelState.AddModelError("", "Erro ao criar sala."); return Page(); }
            TempData["Sucesso"] = $"Sala '{Nome}' criada."; return RedirectToPage("Index");
        }
        private HttpClient Http() { var h = _f.CreateClient(); h.BaseAddress = new Uri(_c["ApiSettings:BaseUrl"]!); h.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JWT")); return h; }
    }

    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _f; private readonly IConfiguration _c;
        public EditModel(IHttpClientFactory f, IConfiguration c) { _f = f; _c = c; }
        [BindProperty] public int SalaId { get; set; }
        [BindProperty, Required, Display(Name = "Nome")] public string Nome { get; set; } = string.Empty;
        [BindProperty, Required, Range(1,500), Display(Name = "Capacidade Máxima")] public int CapacidadeMaxima { get; set; }
        [BindProperty, Display(Name = "Descrição")] public string? Descricao { get; set; }
        [BindProperty] public bool Ativa { get; set; } = true;
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var h = Http(); var resp = await h.GetAsync($"api/Salas/{id}");
            if (!resp.IsSuccessStatusCode) return NotFound();
            var s = JsonSerializer.Deserialize<SalaDto>(await resp.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            SalaId = s.Id; Nome = s.Nome; CapacidadeMaxima = s.CapacidadeMaxima; Descricao = s.Descricao; Ativa = s.Ativa;
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            var h = Http(); var body = new { Id = SalaId, Nome, CapacidadeMaxima, Descricao, Ativa };
            await h.PutAsync($"api/Salas/{SalaId}", new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));
            TempData["Sucesso"] = "Sala atualizada."; return RedirectToPage("Index");
        }
        private HttpClient Http() { var h = _f.CreateClient(); h.BaseAddress = new Uri(_c["ApiSettings:BaseUrl"]!); h.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JWT")); return h; }
    }

    public class DeleteModel : PageModel
    {
        private readonly IHttpClientFactory _f; private readonly IConfiguration _c;
        public DeleteModel(IHttpClientFactory f, IConfiguration c) { _f = f; _c = c; }
        public SalaDto? Sala { get; set; }
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var h = Http(); var resp = await h.GetAsync($"api/Salas/{id}");
            if (!resp.IsSuccessStatusCode) return NotFound();
            Sala = JsonSerializer.Deserialize<SalaDto>(await resp.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(int id)
        {
            var h = Http(); var resp = await h.DeleteAsync($"api/Salas/{id}");
            if (!resp.IsSuccessStatusCode) { var m = JsonSerializer.Deserialize<Dictionary<string,string>>(await resp.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); TempData["Erro"] = m?.GetValueOrDefault("mensagem") ?? "Erro ao eliminar."; }
            else TempData["Sucesso"] = "Sala eliminada.";
            return RedirectToPage("Index");
        }
        private HttpClient Http() { var h = _f.CreateClient(); h.BaseAddress = new Uri(_c["ApiSettings:BaseUrl"]!); h.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JWT")); return h; }
    }
}

// ===== UTILIZADORES =====
namespace GymManagement.Web.Pages.Utilizadores
{
    using System.Net.Http.Headers;
    using System.Text.Json;

    public class UtilizadorListDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefone { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public DateTime DataRegisto { get; set; }
    }

    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _f; private readonly IConfiguration _c;
        public IndexModel(IHttpClientFactory f, IConfiguration c) { _f = f; _c = c; }
        public List<UtilizadorListDto> Utilizadores { get; set; } = new();
        public async Task OnGetAsync()
        {
            try { var h = Http(); var j = await h.GetStringAsync("api/Utilizadores"); Utilizadores = JsonSerializer.Deserialize<List<UtilizadorListDto>>(j, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new(); } catch { }
        }
        private HttpClient Http() { var h = _f.CreateClient(); h.BaseAddress = new Uri(_c["ApiSettings:BaseUrl"]!); h.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JWT")); return h; }
    }

    public class ToggleAtivoModel : PageModel
    {
        private readonly IHttpClientFactory _f; private readonly IConfiguration _c;
        public ToggleAtivoModel(IHttpClientFactory f, IConfiguration c) { _f = f; _c = c; }
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var h = _f.CreateClient(); h.BaseAddress = new Uri(_c["ApiSettings:BaseUrl"]!);
            h.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JWT"));
            await h.PutAsync($"api/Utilizadores/{id}/ativar", null);
            TempData["Sucesso"] = "Estado do utilizador alterado.";
            return RedirectToPage("Index");
        }
    }

    public class DeleteModel : PageModel
    {
        private readonly IHttpClientFactory _f; private readonly IConfiguration _c;
        public DeleteModel(IHttpClientFactory f, IConfiguration c) { _f = f; _c = c; }
        public UtilizadorListDto? Utilizador { get; set; }
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var h = _f.CreateClient(); h.BaseAddress = new Uri(_c["ApiSettings:BaseUrl"]!);
            h.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JWT"));
            var resp = await h.GetAsync($"api/Utilizadores/{id}");
            if (!resp.IsSuccessStatusCode) return NotFound();
            Utilizador = JsonSerializer.Deserialize<UtilizadorListDto>(await resp.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(int id)
        {
            var h = _f.CreateClient(); h.BaseAddress = new Uri(_c["ApiSettings:BaseUrl"]!);
            h.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JWT"));
            await h.DeleteAsync($"api/Utilizadores/{id}");
            TempData["Sucesso"] = "Utilizador eliminado.";
            return RedirectToPage("Index");
        }
    }
}

// ===== FREQUENCIAS =====
namespace GymManagement.Web.Pages.Frequencias
{
    using System.Net.Http.Headers;
    using System.Text.Json;

    public class FrequenciaListDto
    {
        public int Id { get; set; }
        public DateTime Entrada { get; set; }
        public DateTime? Saida { get; set; }
        public string? Observacoes { get; set; }
        public bool EmGinasio { get; set; }
        public string Duracao { get; set; } = string.Empty;
        public string Utilizador { get; set; } = string.Empty;
        public int UtilizadorId { get; set; }
    }

    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _f; private readonly IConfiguration _c;
        public IndexModel(IHttpClientFactory f, IConfiguration c) { _f = f; _c = c; }
        public List<FrequenciaListDto> Frequencias { get; set; } = new();
        public async Task OnGetAsync()
        {
            try { var h = Http(); var j = await h.GetStringAsync("api/Frequencias"); Frequencias = JsonSerializer.Deserialize<List<FrequenciaListDto>>(j, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new(); } catch { }
        }
        private HttpClient Http() { var h = _f.CreateClient(); h.BaseAddress = new Uri(_c["ApiSettings:BaseUrl"]!); h.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JWT")); return h; }
    }

    public class EntradaModel : PageModel
    {
        private readonly IHttpClientFactory _f; private readonly IConfiguration _c;
        public EntradaModel(IHttpClientFactory f, IConfiguration c) { _f = f; _c = c; }
        public async Task<IActionResult> OnGetAsync()
        {
            var h = _f.CreateClient(); h.BaseAddress = new Uri(_c["ApiSettings:BaseUrl"]!);
            h.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JWT"));
            var resp = await h.PostAsync("api/Frequencias/Entrada", new StringContent("null", System.Text.Encoding.UTF8, "application/json"));
            var body = JsonSerializer.Deserialize<Dictionary<string, string>>(await resp.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            TempData[resp.IsSuccessStatusCode ? "Sucesso" : "Erro"] = body?.GetValueOrDefault("mensagem") ?? "";
            return RedirectToPage("Index");
        }
    }

    public class SaidaModel : PageModel
    {
        private readonly IHttpClientFactory _f; private readonly IConfiguration _c;
        public SaidaModel(IHttpClientFactory f, IConfiguration c) { _f = f; _c = c; }
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var h = _f.CreateClient(); h.BaseAddress = new Uri(_c["ApiSettings:BaseUrl"]!);
            h.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JWT"));
            var resp = await h.PutAsync($"api/Frequencias/{id}/Saida", null);
            var body = JsonSerializer.Deserialize<Dictionary<string, string>>(await resp.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            TempData[resp.IsSuccessStatusCode ? "Sucesso" : "Erro"] = body?.GetValueOrDefault("mensagem") ?? "";
            return RedirectToPage("Index");
        }
    }

    public class DeleteModel : PageModel
    {
        private readonly IHttpClientFactory _f; private readonly IConfiguration _c;
        public DeleteModel(IHttpClientFactory f, IConfiguration c) { _f = f; _c = c; }
        public FrequenciaListDto? Frequencia { get; set; }
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var h = _f.CreateClient(); h.BaseAddress = new Uri(_c["ApiSettings:BaseUrl"]!);
            h.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JWT"));
            var resp = await h.GetAsync("api/Frequencias");
            var lista = JsonSerializer.Deserialize<List<FrequenciaListDto>>(await resp.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            Frequencia = lista.FirstOrDefault(f => f.Id == id);
            if (Frequencia == null) return NotFound();
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(int id)
        {
            var h = _f.CreateClient(); h.BaseAddress = new Uri(_c["ApiSettings:BaseUrl"]!);
            h.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("JWT"));
            await h.DeleteAsync($"api/Frequencias/{id}");
            TempData["Sucesso"] = "Registo eliminado.";
            return RedirectToPage("Index");
        }
    }
}
