using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Web.Pages.Instrutores
{
    public class InstrutorDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Especialidade { get; set; }
        public string? Telefone { get; set; }
        public bool Ativo { get; set; }
        public int TotalAulas { get; set; }
    }

    public partial class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _config;
        public IndexModel(IHttpClientFactory factory, IConfiguration config) { _factory = factory; _config = config; }
        public List<InstrutorDto> Instrutores { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                var http = CriarCliente();
                var json = await http.GetStringAsync("api/Instrutores");
                Instrutores = JsonSerializer.Deserialize<List<InstrutorDto>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
            catch { }
        }

        private HttpClient CriarCliente()
        {
            var http = _factory.CreateClient();
            http.BaseAddress = new Uri(_config["ApiSettings:BaseUrl"]!);
            var token = (HttpContext.Session.GetString("JWT") ?? User.FindFirst("jwt")?.Value ?? "");
            if (!string.IsNullOrEmpty(token))
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return http;
        }
    }

    public partial class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _config;
        public CreateModel(IHttpClientFactory factory, IConfiguration config) { _factory = factory; _config = config; }

        [BindProperty, Required, Display(Name = "Nome")] public string Nome { get; set; } = string.Empty;
        [BindProperty, Required, EmailAddress, Display(Name = "Email")] public string Email { get; set; } = string.Empty;
        [BindProperty, Display(Name = "Especialidade")] public string? Especialidade { get; set; }
        [BindProperty, Display(Name = "Telefone")] public string? Telefone { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            var http = CriarCliente();
            var body = new { Nome, Email, Especialidade, Telefone, Ativo = true };
            var resp = await http.PostAsync("api/Instrutores",
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));
            if (!resp.IsSuccessStatusCode) { ModelState.AddModelError("", "Erro ao criar instrutor."); return Page(); }
            TempData["Sucesso"] = $"Instrutor '{Nome}' criado com sucesso.";
            return RedirectToPage("Index");
        }

        private HttpClient CriarCliente()
        {
            var http = _factory.CreateClient();
            http.BaseAddress = new Uri(_config["ApiSettings:BaseUrl"]!);
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (HttpContext.Session.GetString("JWT") ?? User.FindFirst("jwt")?.Value ?? ""));
            return http;
        }
    }

    public partial class EditModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _config;
        public EditModel(IHttpClientFactory factory, IConfiguration config) { _factory = factory; _config = config; }

        [BindProperty] public int InstrutorId { get; set; }
        [BindProperty, Required, Display(Name = "Nome")] public string Nome { get; set; } = string.Empty;
        [BindProperty, Required, EmailAddress, Display(Name = "Email")] public string Email { get; set; } = string.Empty;
        [BindProperty, Display(Name = "Especialidade")] public string? Especialidade { get; set; }
        [BindProperty, Display(Name = "Telefone")] public string? Telefone { get; set; }
        [BindProperty, Display(Name = "Ativo")] public bool Ativo { get; set; } = true;

        public string? ErroMsg { get; set; }

        public async Task<IActionResult> OnGetAsync([FromRoute] int id)
        {
            try
            {
                var http = CriarCliente();
                var resp = await http.GetAsync($"api/Instrutores/{id}");
                if (!resp.IsSuccessStatusCode)
                {
                    TempData["Erro"] = $"Não foi possível carregar o instrutor (HTTP {(int)resp.StatusCode}).";
                    return RedirectToPage("Index");
                }
                var json = await resp.Content.ReadAsStringAsync();
                var i = JsonSerializer.Deserialize<InstrutorDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (i == null) { TempData["Erro"] = "Instrutor não encontrado."; return RedirectToPage("Index"); }
                InstrutorId = i.Id; Nome = i.Nome; Email = i.Email; Especialidade = i.Especialidade; Telefone = i.Telefone; Ativo = i.Ativo;
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
            var body = new { Id = InstrutorId, Nome, Email, Especialidade, Telefone, Ativo };
            await http.PutAsync($"api/Instrutores/{InstrutorId}",
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));
            TempData["Sucesso"] = "Instrutor atualizado com sucesso.";
            return RedirectToPage("Index");
        }

        private HttpClient CriarCliente()
        {
            var http = _factory.CreateClient();
            http.BaseAddress = new Uri(_config["ApiSettings:BaseUrl"]!);
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (HttpContext.Session.GetString("JWT") ?? User.FindFirst("jwt")?.Value ?? ""));
            return http;
        }
    }

    public partial class DeleteModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _config;
        public DeleteModel(IHttpClientFactory factory, IConfiguration config) { _factory = factory; _config = config; }
        public InstrutorDto? Instrutor { get; set; }

        public async Task<IActionResult> OnGetAsync([FromRoute] int id)
        {
            var http = CriarCliente();
            var resp = await http.GetAsync($"api/Instrutores/{id}");
            if (!resp.IsSuccessStatusCode) return NotFound();
            Instrutor = JsonSerializer.Deserialize<InstrutorDto>(await resp.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var http = CriarCliente();
            var resp = await http.DeleteAsync($"api/Instrutores/{id}");
            if (!resp.IsSuccessStatusCode)
            {
                var msg = JsonSerializer.Deserialize<Dictionary<string, string>>(await resp.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                TempData["Erro"] = msg?.GetValueOrDefault("mensagem") ?? "Não foi possível eliminar.";
            }
            else TempData["Sucesso"] = "Instrutor eliminado.";
            return RedirectToPage("Index");
        }

        private HttpClient CriarCliente()
        {
            var http = _factory.CreateClient();
            http.BaseAddress = new Uri(_config["ApiSettings:BaseUrl"]!);
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (HttpContext.Session.GetString("JWT") ?? User.FindFirst("jwt")?.Value ?? ""));
            return http;
        }
    }
}
